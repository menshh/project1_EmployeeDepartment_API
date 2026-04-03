using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmployeeDepartment.Data;
using EmployeeDepartment.DTOs.Auth;
using EmployeeDepartment.Helpers;
using EmployeeDepartment.Interfaces;
using EmployeeDepartment.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeDepartment.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var exists = await _context.Users
            .AnyAsync(u => u.UserName == dto.UserName || u.Email == dto.Email);

        if (exists)
            throw new InvalidOperationException("Username or email already exists.");

        PasswordHelper.CreatePasswordHash(dto.Password, out var passwordHash, out var passwordSalt);

        var user = new AppUser
        {
            FullName = dto.FullName.Trim(),
            UserName = dto.UserName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role.Trim(),
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var userName = dto.UserName.Trim();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == userName);

        if (user == null || !PasswordHelper.VerifyPasswordHash(dto.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        return BuildAuthResponse(user);
    }

    private AuthResponseDto BuildAuthResponse(AppUser user)
    {
        var durationInMinutes = int.Parse(_configuration["Jwt:DurationInMinutes"]!);
        var expiresAt = DateTime.UtcNow.AddMinutes(durationInMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt,
            UserName = user.UserName,
            Role = user.Role
        };
    }
}
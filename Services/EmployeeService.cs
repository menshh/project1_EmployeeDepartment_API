using EmployeeDepartment.Data;
using EmployeeDepartment.DTOs.Employees;
using EmployeeDepartment.Interfaces;
using EmployeeDepartment.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDepartment.Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;

    public EmployeeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployeeReadDto>> GetAllAsync()
    {
        return await _context.Employees
            .AsNoTracking()
            .Select(e => new EmployeeReadDto
            {
                Id = e.Id,
                FullName = e.FullName,
                Email = e.Email,
                JobTitle = e.JobTitle,
                Salary = e.Salary,
                HireDate = e.HireDate,
                DepartmentName = e.Department != null ? e.Department.Name : string.Empty,
                Profile = e.Profile == null ? null : new EmployeeProfileReadDto
                {
                    Address = e.Profile.Address,
                    PhoneNumber = e.Profile.PhoneNumber,
                    DateOfBirth = e.Profile.DateOfBirth,
                    EmergencyContactName = e.Profile.EmergencyContactName,
                    EmergencyContactPhone = e.Profile.EmergencyContactPhone
                },
                Projects = e.Projects
                    .Select(p => new ProjectSummaryDto
                    {
                        Id = p.Id,
                        Name = p.Name
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<EmployeeReadDto?> GetByIdAsync(int id)
    {
        return await _context.Employees
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new EmployeeReadDto
            {
                Id = e.Id,
                FullName = e.FullName,
                Email = e.Email,
                JobTitle = e.JobTitle,
                Salary = e.Salary,
                HireDate = e.HireDate,
                DepartmentName = e.Department != null ? e.Department.Name : string.Empty,
                Profile = e.Profile == null ? null : new EmployeeProfileReadDto
                {
                    Address = e.Profile.Address,
                    PhoneNumber = e.Profile.PhoneNumber,
                    DateOfBirth = e.Profile.DateOfBirth,
                    EmergencyContactName = e.Profile.EmergencyContactName,
                    EmergencyContactPhone = e.Profile.EmergencyContactPhone
                },
                Projects = e.Projects
                    .Select(p => new ProjectSummaryDto
                    {
                        Id = p.Id,
                        Name = p.Name
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<EmployeeReadDto> CreateAsync(EmployeeCreateDto dto)
    {
        await ValidateEmployeeReferencesAsync(dto.DepartmentId, dto.ProjectIds, dto.Email, null);

        var projects = await LoadProjectsAsync(dto.ProjectIds);

        var employee = new Employee
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            JobTitle = dto.JobTitle.Trim(),
            Salary = dto.Salary,
            HireDate = dto.HireDate,
            DepartmentId = dto.DepartmentId,
            Projects = projects,
            Profile = new EmployeeProfile
            {
                Address = dto.Profile.Address.Trim(),
                PhoneNumber = dto.Profile.PhoneNumber.Trim(),
                DateOfBirth = dto.Profile.DateOfBirth,
                EmergencyContactName = dto.Profile.EmergencyContactName.Trim(),
                EmergencyContactPhone = dto.Profile.EmergencyContactPhone.Trim()
            }
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(employee.Id)
            ?? throw new InvalidOperationException("Employee was created but could not be loaded.");
    }

    public async Task<bool> UpdateAsync(int id, EmployeeUpdateDto dto)
    {
        await ValidateEmployeeReferencesAsync(dto.DepartmentId, dto.ProjectIds, dto.Email, id);

        var employee = await _context.Employees
            .Include(e => e.Profile)
            .Include(e => e.Projects)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null)
        {
            return false;
        }

        employee.FullName = dto.FullName.Trim();
        employee.Email = dto.Email.Trim().ToLower();
        employee.JobTitle = dto.JobTitle.Trim();
        employee.Salary = dto.Salary;
        employee.HireDate = dto.HireDate;
        employee.DepartmentId = dto.DepartmentId;

        if (employee.Profile is null)
        {
            employee.Profile = new EmployeeProfile();
        }

        employee.Profile.Address = dto.Profile.Address.Trim();
        employee.Profile.PhoneNumber = dto.Profile.PhoneNumber.Trim();
        employee.Profile.DateOfBirth = dto.Profile.DateOfBirth;
        employee.Profile.EmergencyContactName = dto.Profile.EmergencyContactName.Trim();
        employee.Profile.EmergencyContactPhone = dto.Profile.EmergencyContactPhone.Trim();

        employee.Projects.Clear();
        var projects = await LoadProjectsAsync(dto.ProjectIds);
        foreach (var project in projects)
        {
            employee.Projects.Add(project);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (employee is null)
        {
            return false;
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<List<Project>> LoadProjectsAsync(IEnumerable<int> projectIds)
    {
        var distinctProjectIds = projectIds.Distinct().ToList();
        if (!distinctProjectIds.Any())
        {
            return new List<Project>();
        }

        return await _context.Projects
            .Where(p => distinctProjectIds.Contains(p.Id))
            .ToListAsync();
    }

    private async Task ValidateEmployeeReferencesAsync(int departmentId, IEnumerable<int> projectIds, string email, int? currentEmployeeId)
    {
        var departmentExists = await _context.Departments.AnyAsync(d => d.Id == departmentId);
        if (!departmentExists)
        {
            throw new InvalidOperationException("Department was not found.");
        }

        var normalizedEmail = email.Trim().ToLower();
        var emailExists = await _context.Employees.AnyAsync(e => e.Email == normalizedEmail && (!currentEmployeeId.HasValue || e.Id != currentEmployeeId.Value));
        if (emailExists)
        {
            throw new InvalidOperationException("Employee email already exists.");
        }

        var distinctProjectIds = projectIds.Distinct().ToList();
        if (!distinctProjectIds.Any())
        {
            return;
        }

        var existingProjectCount = await _context.Projects.CountAsync(p => distinctProjectIds.Contains(p.Id));
        if (existingProjectCount != distinctProjectIds.Count)
        {
            throw new InvalidOperationException("One or more project IDs are invalid.");
        }
    }
}

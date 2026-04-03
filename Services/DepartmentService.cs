using EmployeeDepartment.Data;
using EmployeeDepartment.DTOs.Departments;
using EmployeeDepartment.Interfaces;
using EmployeeDepartment.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDepartment.Services;

public class DepartmentService : IDepartmentService
{
    private readonly AppDbContext _context;

    public DepartmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentReadDto>> GetAllAsync()
    {
        return await _context.Departments
            .AsNoTracking()
            .Select(d => new DepartmentReadDto
            {
                Id = d.Id,
                Name = d.Name,
                Location = d.Location,
                EmployeesCount = d.Employees.Count
            })
            .ToListAsync();
    }

    public async Task<DepartmentReadDto?> GetByIdAsync(int id)
    {
        return await _context.Departments
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DepartmentReadDto
            {
                Id = d.Id,
                Name = d.Name,
                Location = d.Location,
                EmployeesCount = d.Employees.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<DepartmentReadDto> CreateAsync(DepartmentCreateDto dto)
    {
        var exists = await _context.Departments.AnyAsync(d => d.Name == dto.Name);
        if (exists)
        {
            throw new InvalidOperationException("Department name already exists.");
        }

        var department = new Department
        {
            Name = dto.Name.Trim(),
            Location = dto.Location.Trim()
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        return new DepartmentReadDto
        {
            Id = department.Id,
            Name = department.Name,
            Location = department.Location,
            EmployeesCount = 0
        };
    }

    public async Task<bool> UpdateAsync(int id, DepartmentUpdateDto dto)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
        if (department is null)
        {
            return false;
        }

        var exists = await _context.Departments.AnyAsync(d => d.Id != id && d.Name == dto.Name);
        if (exists)
        {
            throw new InvalidOperationException("Department name already exists.");
        }

        department.Name = dto.Name.Trim();
        department.Location = dto.Location.Trim();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department is null)
        {
            return false;
        }

        if (department.Employees.Any())
        {
            throw new InvalidOperationException("Cannot delete a department that still has employees.");
        }

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();
        return true;
    }
}

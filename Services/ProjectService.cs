using EmployeeDepartment.Data;
using EmployeeDepartment.DTOs.Projects;
using EmployeeDepartment.Interfaces;
using EmployeeDepartment.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDepartment.Services;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _context;

    public ProjectService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectReadDto>> GetAllAsync()
    {
        return await _context.Projects
            .AsNoTracking()
            .Select(p => new ProjectReadDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Budget = p.Budget,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                EmployeesCount = p.Employees.Count
            })
            .ToListAsync();
    }

    public async Task<ProjectReadDto?> GetByIdAsync(int id)
    {
        return await _context.Projects
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProjectReadDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Budget = p.Budget,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                EmployeesCount = p.Employees.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProjectReadDto> CreateAsync(ProjectCreateDto dto)
    {
        if (dto.EndDate < dto.StartDate)
        {
            throw new InvalidOperationException("Project end date cannot be before start date.");
        }

        var project = new Project
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Budget = dto.Budget,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return new ProjectReadDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Budget = project.Budget,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            EmployeesCount = 0
        };
    }

    public async Task<bool> UpdateAsync(int id, ProjectUpdateDto dto)
    {
        if (dto.EndDate < dto.StartDate)
        {
            throw new InvalidOperationException("Project end date cannot be before start date.");
        }

        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
        if (project is null)
        {
            return false;
        }

        project.Name = dto.Name.Trim();
        project.Description = dto.Description.Trim();
        project.Budget = dto.Budget;
        project.StartDate = dto.StartDate;
        project.EndDate = dto.EndDate;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
        if (project is null)
        {
            return false;
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        return true;
    }
}

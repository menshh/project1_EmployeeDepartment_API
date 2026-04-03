using EmployeeDepartment.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDepartment.Jobs;

public class ProjectReminderJob
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProjectReminderJob> _logger;

    public ProjectReminderJob(AppDbContext context, ILogger<ProjectReminderJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CheckProjectsAsync()
    {
        var projects = await _context.Projects
            .AsNoTracking()
            .Select(p => new
            {
                p.Name,
                EmployeeCount = p.Employees.Count
            })
            .ToListAsync();

        foreach (var project in projects)
        {
            _logger.LogInformation(
                "Project reminder: project '{ProjectName}' currently has {EmployeeCount} assigned employees.",
                project.Name,
                project.EmployeeCount);
        }
    }
}

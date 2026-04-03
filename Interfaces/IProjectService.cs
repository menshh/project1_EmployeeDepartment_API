using EmployeeDepartment.DTOs.Projects;

namespace EmployeeDepartment.Interfaces;

public interface IProjectService
{
    Task<List<ProjectReadDto>> GetAllAsync();
    Task<ProjectReadDto?> GetByIdAsync(int id);
    Task<ProjectReadDto> CreateAsync(ProjectCreateDto dto);
    Task<bool> UpdateAsync(int id, ProjectUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}

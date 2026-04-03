using EmployeeDepartment.DTOs.Employees;

namespace EmployeeDepartment.Interfaces;

public interface IEmployeeService
{
    Task<List<EmployeeReadDto>> GetAllAsync();
    Task<EmployeeReadDto?> GetByIdAsync(int id);
    Task<EmployeeReadDto> CreateAsync(EmployeeCreateDto dto);
    Task<bool> UpdateAsync(int id, EmployeeUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}

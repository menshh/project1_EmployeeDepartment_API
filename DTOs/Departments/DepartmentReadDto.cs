namespace EmployeeDepartment.DTOs.Departments;

public class DepartmentReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int EmployeesCount { get; set; }
}

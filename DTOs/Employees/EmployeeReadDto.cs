namespace EmployeeDepartment.DTOs.Employees;

public class EmployeeReadDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public EmployeeProfileReadDto? Profile { get; set; }
    public List<ProjectSummaryDto> Projects { get; set; } = new();
}

public class EmployeeProfileReadDto
{
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
}

public class ProjectSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

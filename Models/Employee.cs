namespace EmployeeDepartment.Models;

public class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public EmployeeProfile? Profile { get; set; }
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}

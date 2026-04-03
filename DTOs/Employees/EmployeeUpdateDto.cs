using System.ComponentModel.DataAnnotations;

namespace EmployeeDepartment.DTOs.Employees;

public class EmployeeUpdateDto
{
    [Required]
    [MinLength(3)]
    [MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string JobTitle { get; set; } = string.Empty;

    [Range(0, 1000000)]
    public decimal Salary { get; set; }

    [Required]
    public DateTime HireDate { get; set; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [Required]
    public EmployeeProfileCreateDto Profile { get; set; } = new();

    public List<int> ProjectIds { get; set; } = new();
}

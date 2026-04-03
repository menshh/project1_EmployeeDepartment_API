using System.ComponentModel.DataAnnotations;

namespace EmployeeDepartment.DTOs.Departments;

public class DepartmentUpdateDto
{
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    [MaxLength(150)]
    public string Location { get; set; } = string.Empty;
}

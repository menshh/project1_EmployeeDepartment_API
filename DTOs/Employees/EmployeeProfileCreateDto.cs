using System.ComponentModel.DataAnnotations;

namespace EmployeeDepartment.DTOs.Employees;

public class EmployeeProfileCreateDto
{
    [Required]
    [MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Phone]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [MaxLength(100)]
    public string EmergencyContactName { get; set; } = string.Empty;

    [Required]
    [Phone]
    [MaxLength(20)]
    public string EmergencyContactPhone { get; set; } = string.Empty;
}

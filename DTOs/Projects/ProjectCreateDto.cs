using System.ComponentModel.DataAnnotations;

namespace EmployeeDepartment.DTOs.Projects;

public class ProjectCreateDto
{
    [Required]
    [MinLength(2)]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(5)]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 100000000)]
    public decimal Budget { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}

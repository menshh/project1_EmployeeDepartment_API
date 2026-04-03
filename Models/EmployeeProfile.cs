namespace EmployeeDepartment.Models;

public class EmployeeProfile
{
    public int Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
}

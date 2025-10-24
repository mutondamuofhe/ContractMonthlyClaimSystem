using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string EmployeeId { get; set; }
    public string RoleKind { get; set; } // Lecturer, ProgrammeCoordinator, AcademicManager
    public string Department { get; set; }
}

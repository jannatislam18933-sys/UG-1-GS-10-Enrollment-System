namespace EnrollmentSystemAPI.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Student, Clerk, Admin

public int? DepartmentID { get; set; }
public Department? Department { get; set; }
public int? DegreeProgramID { get; set; }

public DegreeProgram? DegreeProgram { get; set; }
public int CurrentSemester { get; set; } = 1;
public bool IsDeleted { get; set; } = false;
    }
}
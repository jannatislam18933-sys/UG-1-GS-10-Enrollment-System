namespace EnrollmentSystemAPI.Models
{
    public class DegreeProgram
    {
        public int DegreeProgramID { get; set; }

        // Example: BSCS, BSSE, BSAI
        public string ProgramName { get; set; } = string.Empty;

        // BS or MS
        public string DegreeLevel { get; set; } = string.Empty;

        // Department Relationship
        public int DepartmentID { get; set; }

        public Department Department { get; set; } = null!;
    }
}
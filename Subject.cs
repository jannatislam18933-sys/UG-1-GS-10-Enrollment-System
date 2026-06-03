using EnrollmentSystemAPI.Enums;

namespace EnrollmentSystemAPI.Models
{
    public class Subject
    {
        public int SubjectID { get; set; }

        public string SubjectCode { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;

        public int CreditHours { get; set; }

        // Semester Number
        public int Semester { get; set; }

        // BS or MS
        public DegreeLevel DegreeLevel { get; set; }

        // Degree program Relationship
        public int DegreeProgramID { get; set; }

        public DegreeProgram DegreeProgram { get; set; } = null!;
    }
}
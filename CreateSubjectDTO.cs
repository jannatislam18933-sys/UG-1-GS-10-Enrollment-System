using EnrollmentSystemAPI.Enums;

namespace EnrollmentSystemAPI.DTOs
{
    public class CreateSubjectDTO
    {
        public string SubjectCode { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;

        public int CreditHours { get; set; }

        public int Semester { get; set; }

        public DegreeLevel DegreeLevel { get; set; }

        public int DegreeProgramID { get; set; }
    }
}
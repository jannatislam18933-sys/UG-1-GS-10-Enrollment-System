namespace EnrollmentSystemAPI.Models
{
    public class Notice
    {
        public int NoticeID { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // NULL = Global Notice
        public int? DepartmentID { get; set; }

        public Department? Department { get; set; }

        public string CreatedByRole { get; set; } = string.Empty;
    }
}
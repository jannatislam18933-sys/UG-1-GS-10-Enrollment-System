namespace EnrollmentSystemAPI.Models
{
    public class Enrollment
    {
        public int EnrollmentID { get; set; }

        public int StudentID { get; set; }
        public User Student { get; set; } = null!;
        
        public int SubjectID { get; set; }
        public Subject Subject { get; set; } = null!;
        
        public int? FeeSlipID { get; set; }
        public FeeSlip? FeeSlip { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Pending";

        public string? RejectionReason { get; set; }

        public DateTime? ReviewedAt { get; set; }
        public User? ReviewedByClerk { get; set; }
        
        public int? ReviewedByClerkID { get; set; }
        
    }
}
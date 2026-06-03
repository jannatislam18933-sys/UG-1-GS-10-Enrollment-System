using EnrollmentSystemAPI.Enums;

namespace EnrollmentSystemAPI.Models
{
    public class FeeSlip
    {
        public int EnrollmentRequestID { get; set; }
        public int FeeSlipID { get; set; }
        // Student Relationship
        public int StudentID { get; set; }
        public User Student { get; set; } = null!;

        // File Information
        public string FilePath { get; set; } = string.Empty;

        public string? TransactionID { get; set; }
        public int Semester { get; set; }
        // Verification Status
        public FeeSlipStatus Status { get; set; } = FeeSlipStatus.Pending;
        public bool IsDeleted { get; set; } = false; 
        // Dates
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public DateTime? VerifiedAt { get; set; }

        // Clerk Verification
        public int? VerifiedByClerkID { get; set; }
        public string? RejectionReason { get; set; }

        public User? VerifiedByClerk { get; set; }
    }
}
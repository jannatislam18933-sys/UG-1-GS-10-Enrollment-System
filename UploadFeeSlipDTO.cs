using Microsoft.AspNetCore.Http;

namespace EnrollmentSystemAPI.DTOs
{
    public class UploadFeeSlipDTO
    {
        public IFormFile File { get; set; } = null!;

        public string? TransactionID { get; set; }
    }
}

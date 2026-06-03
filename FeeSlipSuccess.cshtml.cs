using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnrollmentSystemAPI.Pages.Student
{
    public class FeeSlipSuccessModel : PageModel
    {
        public string StudentName { get; set; } = "Ahmed Khan";
        public string FileName    { get; set; } = "fee-challan.pdf";
        public string UploadDate  { get; set; } = DateTime.Now.ToString("MMMM d, yyyy");
        public string? TransactionId { get; set; }

        public void OnGet()
        {
            StudentName   = HttpContext.Session.GetString("StudentName") ?? "Student";
            FileName      = TempData["UploadedFileName"]?.ToString() ?? "fee-challan.pdf";
            UploadDate    = TempData["UploadDate"]?.ToString()       ?? DateTime.Now.ToString("MMMM d, yyyy");
            TransactionId = TempData["TransactionId"]?.ToString();
        }
    }
}

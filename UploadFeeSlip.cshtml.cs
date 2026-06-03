using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnrollmentSystemAPI.Pages.Student
{
    public class UploadFeeSlipModel : PageModel
    {
        public string StudentName { get; set; } = "Ahmed Khan";
        public string? ErrorMessage { get; set; }

        [BindProperty] public IFormFile? FeeSlipFile { get; set; }
        [BindProperty] public string? TransactionId { get; set; }

        private static readonly string[] AllowedTypes = { "application/pdf", "image/jpeg", "image/png" };
        private const long MaxSize = 5 * 1024 * 1024; // 5MB

        public void OnGet()
        {
            StudentName = HttpContext.Session.GetString("StudentName") ?? "Student";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            StudentName = HttpContext.Session.GetString("StudentName") ?? "Student";

            if (FeeSlipFile == null || FeeSlipFile.Length == 0)
            {
                ErrorMessage = "Please select a fee slip file.";
                return Page();
            }

            if (!AllowedTypes.Contains(FeeSlipFile.ContentType))
            {
                ErrorMessage = "Only PDF, JPG, and PNG files are supported.";
                return Page();
            }

            if (FeeSlipFile.Length > MaxSize)
            {
                ErrorMessage = "File size must not exceed 5MB.";
                return Page();
            }

            // ── Save file and record in DB ───────────────────────────────
            // var filePath = await _fileService.SaveFeeSlipAsync(FeeSlipFile);
            // await _feeService.RecordUploadAsync(studentId, filePath, TransactionId);
            // ────────────────────────────────────────────────────────────

            // Store file info in TempData for success screen
            TempData["UploadedFileName"] = FeeSlipFile.FileName;
            TempData["UploadDate"]       = DateTime.Now.ToString("MMMM d, yyyy");
            TempData["TransactionId"]    = TransactionId;

            return RedirectToPage("/Student/FeeSlipSuccess");
        }
    }
}

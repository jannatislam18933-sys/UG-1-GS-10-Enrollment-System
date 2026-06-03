using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnrollmentSystemAPI.Pages.Account
{
    public class CheckEmailModel : PageModel
    {
        public string? SentToEmail { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            SentToEmail = TempData["ResetEmail"]?.ToString();
        }

        public async Task<IActionResult> OnPostAsync(string email)
        {
            SentToEmail = email;

            // ── Resend logic ─────────────────────────────────────────────
            // await _emailService.SendPasswordResetEmailAsync(email, token);
            // ────────────────────────────────────────────────────────────

            SuccessMessage = "Reset email resent successfully. Please check your inbox.";
            return Page();
        }
    }
}

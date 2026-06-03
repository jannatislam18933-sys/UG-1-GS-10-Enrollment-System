using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EnrollmentSystemAPI.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        // private readonly IEmailService _emailService;
        // public ForgotPasswordModel(IEmailService emailService) { _emailService = emailService; }

        [BindProperty]
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // ── Replace with actual email/reset logic ────────────────────
            // var user = await _userService.FindByEmailAsync(Email);
            // if (user != null)
            // {
            //     var token = await _userService.GeneratePasswordResetTokenAsync(user);
            //     await _emailService.SendPasswordResetEmailAsync(Email, token);
            // }
            // Always redirect to CheckEmail (don't reveal if email exists)
            // ────────────────────────────────────────────────────────────

            TempData["ResetEmail"] = Email;
            return RedirectToPage("/Account/CheckEmail");
        }
    }
}

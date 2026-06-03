using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EnrollmentSystemAPI.Pages.Student
{
    public class ProfileModel : PageModel
    {
        public string StudentName    { get; set; } = "Ahmed Khan";
        public string? ErrorMessage   { get; set; }
        public string? SuccessMessage { get; set; }

        [BindProperty] public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Full name is required.")]
            public string FullName       { get; set; } = "Ahmed Khan";

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Enter a valid email address.")]
            public string Email          { get; set; } = "ahmed.khan@university.edu";

            public string RegistrationNo { get; set; } = "FA21-BSE-042";
            public string Program        { get; set; } = "BS Computer Science";

            [RegularExpression(@"^\d{5}-\d{7}-\d{1}$", ErrorMessage = "CNIC format: 12345-1234567-1")]
            public string? CNIC          { get; set; } = "12345-1234567-1";

            [RegularExpression(@"^(\+92|0)[0-9]{10}$", ErrorMessage = "Enter a valid Pakistani phone number.")]
            public string? PhoneNumber   { get; set; } = "+92 300 1234567";

            [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
            public string? CurrentPassword { get; set; }

            [MinLength(6, ErrorMessage = "New password must be at least 6 characters.")]
            public string? NewPassword     { get; set; }

            [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
            public string? ConfirmPassword { get; set; }
        }

        public void OnGet()
        {
            StudentName = HttpContext.Session.GetString("StudentName") ?? "Student";

            // ── Load profile from DB ─────────────────────────────────────
            // var student = await _studentService.GetProfileAsync(studentId);
            // Input = new InputModel { FullName = student.FullName, ... };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            StudentName = HttpContext.Session.GetString("StudentName") ?? "Student";

            // Remove password fields from validation if not changing password
            if (string.IsNullOrEmpty(Input.CurrentPassword) &&
                string.IsNullOrEmpty(Input.NewPassword)     &&
                string.IsNullOrEmpty(Input.ConfirmPassword))
            {
                ModelState.Remove("Input.CurrentPassword");
                ModelState.Remove("Input.NewPassword");
                ModelState.Remove("Input.ConfirmPassword");
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please fix the errors below.";
                return Page();
            }

            // ── Validate current password if changing ────────────────────
            if (!string.IsNullOrEmpty(Input.NewPassword))
            {
                if (string.IsNullOrEmpty(Input.CurrentPassword))
                {
                    ErrorMessage = "Please enter your current password to change it.";
                    return Page();
                }
                // var verified = await _authService.VerifyPasswordAsync(studentId, Input.CurrentPassword);
                // if (!verified) { ErrorMessage = "Current password is incorrect."; return Page(); }
                // await _authService.ChangePasswordAsync(studentId, Input.NewPassword);
            }

            // ── Update profile in DB ─────────────────────────────────────
            // await _studentService.UpdateProfileAsync(studentId, Input);
            // HttpContext.Session.SetString("StudentName", Input.FullName);
            // ────────────────────────────────────────────────────────────

            SuccessMessage = "Profile updated successfully.";
            return Page();
        }
    }
}

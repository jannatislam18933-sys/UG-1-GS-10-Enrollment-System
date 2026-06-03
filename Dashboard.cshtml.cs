using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnrollmentSystemAPI.Pages.Student
{
    public class DashboardModel : PageModel
    {
        public string StudentName { get; set; } = "Ahmed Khan";
        public string EnrollmentStatus { get; set; } = "Open";
        public string FeeStatus { get; set; } = "Pending";
        public string FormStatus { get; set; } = "Not Started";

        public IActionResult OnGet()
{
    if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
    {
        return RedirectToPage("/Account/Login");
    }

    StudentName =
        HttpContext.Session.GetString("StudentName")
        ?? "Student";

    return Page();
}
        
    }
}

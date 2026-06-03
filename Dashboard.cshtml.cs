using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnrollmentSystemAPI.Pages.Admin
{
    public class AdminDashboardModel : PageModel
    {
        public string AdminName { get; set; } = "Admin";

        public IActionResult OnGet()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            {
                return RedirectToPage("/Account/Login");
            }

            AdminName =
                HttpContext.Session.GetString("AdminName")
                ?? "Admin";

            return Page();
        }
    }
}

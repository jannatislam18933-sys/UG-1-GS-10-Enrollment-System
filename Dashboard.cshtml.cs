using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace EnrollmentSystemAPI.Pages.Clerk
{
    public class ActivityItem { public string Name=""; public string Action=""; public string TimeAgo=""; }

    public class ClerkDashboardModel : PageModel
    {
        public string ClerkName { get; set; } = "Clerk";
        public List<ActivityItem> RecentActivity { get; set; } = new();

    public IActionResult OnGet()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            {
                return RedirectToPage("/Account/Login");
            }

            ClerkName =
                HttpContext.Session.GetString("ClerkName")
                ?? "Clerk";

            return Page();
        }
    }
}

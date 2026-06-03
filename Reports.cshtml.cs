using Microsoft.AspNetCore.Mvc.RazorPages;
namespace EnrollmentSystemAPI.Pages.Admin
{
    public class ReportsModel : PageModel
    {
        public string AdminName { get; set; } = "Admin";
        public void OnGet() { AdminName = HttpContext.Session.GetString("AdminName") ?? "Admin"; }
    }
}

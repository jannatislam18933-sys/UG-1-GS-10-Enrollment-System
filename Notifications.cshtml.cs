using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnrollmentSystemAPI.Pages.Student
{
    public class NotificationsModel : PageModel
    {
        public string StudentName { get; set; } = "Ahmed Khan";

        public void OnGet()
        {
            StudentName = HttpContext.Session.GetString("StudentName") ?? "Student";
            // Notices are loaded client-side via /api/notices
        }
    }
}

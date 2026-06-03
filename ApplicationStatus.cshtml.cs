using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnrollmentSystemAPI.Pages.Student
{
    public class ApplicationStatusModel : PageModel
    {
        public string StudentName { get; set; } = "Ahmed Khan";

        public void OnGet()
        {
            StudentName = HttpContext.Session.GetString("StudentName") ?? "Student";
            // Load real status from DB:
            // var status = await _enrollmentService.GetApplicationStatusAsync(studentId);
        }
    }
}

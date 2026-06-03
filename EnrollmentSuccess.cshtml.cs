using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnrollmentSystemAPI.Pages.Student
{
    public class EnrollmentSuccessModel : PageModel
    {
        public string StudentName      { get; set; } = "Ahmed Khan";
        public string Program          { get; set; } = "";
        public string Semester         { get; set; } = "";
        public string CourseCount      { get; set; } = "0";
        public string TotalCredits     { get; set; } = "0";
        public string FeeSlipFileName  { get; set; } = "";
        public string SubmissionDate   { get; set; } = "";
        public string ReferenceNumber  { get; set; } = "";

        public void OnGet()
        {
            StudentName     = HttpContext.Session.GetString("StudentName") ?? "Student";
            Program         = TempData["EnrollmentProgram"]?.ToString()     ?? "—";
            Semester        = TempData["EnrollmentSemester"]?.ToString()    ?? "—";
            CourseCount     = TempData["EnrollmentCourseCount"]?.ToString() ?? "0";
            TotalCredits    = TempData["EnrollmentCredits"]?.ToString()     ?? "0";
            FeeSlipFileName = TempData["EnrollmentFeeFile"]?.ToString()     ?? "fee-slip.pdf";
            SubmissionDate  = TempData["EnrollmentDate"]?.ToString()        ?? DateTime.Now.ToString("MMMM d, yyyy");

            // Generate a reference number (replace with DB-generated ID)
            var regNo = HttpContext.Session.GetString("RegistrationNo") ?? "STU";
            ReferenceNumber = $"ENR-{DateTime.Now.Year}-{regNo.Split('-').LastOrDefault() ?? "000"}";
        }
    }
}

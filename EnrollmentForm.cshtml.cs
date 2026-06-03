using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EnrollmentSystemAPI.Pages.Student{
    public class CourseItem
    {
        public string Code        { get; set; } = "";
        public string Name        { get; set; } = "";
        public int    Credits     { get; set; } = 3;
        public bool   IsMandatory { get; set; }
    }

    public class SupervisorItem
    {
        public int    Id         { get; set; }
        public string Name       { get; set; } = "";
        public string Department { get; set; } = "";
    }

    public class EnrollmentFormModel : PageModel
    {
        public string StudentName { get; set; } = "Ahmed Khan";
        public string? ErrorMessage { get; set; }

        public List<CourseItem>     Courses     { get; set; } = new();
        public List<SupervisorItem> Supervisors { get; set; } = new();
        public List<string>         Programs    { get; set; } = new();

        [BindProperty] public InputModel Input { get; set; } = new();

        private static readonly string[] AllowedTypes = { "application/pdf", "image/jpeg", "image/png" };
        private const long MaxFileSize = 5 * 1024 * 1024;

        public class InputModel
        {
            [Required(ErrorMessage = "Full name is required.")]
            public string FullName { get; set; } = "";

            [Required(ErrorMessage = "Father's name is required.")]
            public string FatherName { get; set; } = "";

            public string RegistrationNo { get; set; } = "FA21-BSE-042";

            [Required(ErrorMessage = "CNIC is required.")]
            [RegularExpression(@"^\d{5}-\d{7}-\d{1}$", ErrorMessage = "CNIC format: 12345-1234567-1")]
            public string CNIC { get; set; } = "";

            [Required(ErrorMessage = "Phone number is required.")]
            [RegularExpression(@"^(\+92|0)[0-9]{10}$", ErrorMessage = "Enter a valid Pakistani phone number.")]
            public string PhoneNumber { get; set; } = "";

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Enter a valid email address.")]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "Address is required.")]
            public string Address { get; set; } = "";

            [Required(ErrorMessage = "Please select your degree program.")]
            public string DegreeProgram { get; set; } = "";

            [Required(ErrorMessage = "Please select your section.")]
            public string Section { get; set; } = "";

            [Required(ErrorMessage = "Please select your semester.")]
            public string Semester { get; set; } = "";

            // Courses
            public List<string> SelectedCourses { get; set; } = new();
            public int?         SupervisorId    { get; set; }

            // Fee slip
            public IFormFile? FeeSlipFile   { get; set; }
            public string?    TransactionId { get; set; }

            // Declaration
            public bool Declaration { get; set; }
        }

        public void OnGet()
        {
            StudentName = HttpContext.Session.GetString("StudentName") ?? "Student";
            // Pre-fill from session/profile
            Input.FullName       = StudentName;
            Input.Email          = HttpContext.Session.GetString("StudentEmail") ?? "";
            Input.RegistrationNo = HttpContext.Session.GetString("RegistrationNo") ?? "FA21-BSE-042";
            LoadOptions();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            StudentName = HttpContext.Session.GetString("StudentName") ?? "Student";
            LoadOptions();

            // Validate fee slip
            if (Input.FeeSlipFile == null || Input.FeeSlipFile.Length == 0)
            {
                ErrorMessage = "Please attach your paid fee slip before submitting.";
                return Page();
            }
            if (!AllowedTypes.Contains(Input.FeeSlipFile.ContentType))
            {
                ErrorMessage = "Fee slip must be a PDF, JPG, or PNG file.";
                return Page();
            }
            if (Input.FeeSlipFile.Length > MaxFileSize)
            {
                ErrorMessage = "Fee slip file size must not exceed 5MB.";
                return Page();
            }
            if (!Input.Declaration)
            {
                ErrorMessage = "Please accept the declaration before submitting.";
                return Page();
            }

            // Merge mandatory courses
            var mandatoryCodes = Courses.Where(c => c.IsMandatory).Select(c => c.Code).ToList();
            var allSelected    = Input.SelectedCourses.Union(mandatoryCodes).Distinct().ToList();

            // ── Save to DB ───────────────────────────────────────────────
            // var filePath = await _fileService.SaveFeeSlipAsync(Input.FeeSlipFile);
            // await _enrollmentService.SubmitEnrollmentAsync(new EnrollmentSubmission
            // {
            //     StudentId      = currentStudentId,
            //     FullName       = Input.FullName,
            //     FatherName     = Input.FatherName,
            //     CNIC           = Input.CNIC,
            //     PhoneNumber    = Input.PhoneNumber,
            //     Email          = Input.Email,
            //     Address        = Input.Address,
            //     DegreeProgram  = Input.DegreeProgram,
            //     Section        = Input.Section,
            //     Semester       = Input.Semester,
            //     SelectedCourses= allSelected,
            //     SupervisorId   = Input.SupervisorId,
            //     FeeSlipPath    = filePath,
            //     TransactionId  = Input.TransactionId,
            // });
            // ────────────────────────────────────────────────────────────

            TempData["EnrollmentName"]        = Input.FullName;
            TempData["EnrollmentProgram"]     = Input.DegreeProgram;
            TempData["EnrollmentSemester"]    = Input.Semester;
            TempData["EnrollmentCourseCount"] = allSelected.Count.ToString();
            TempData["EnrollmentCredits"]     = allSelected.Sum(c =>
                Courses.FirstOrDefault(x => x.Code == c)?.Credits ?? 3).ToString();
            TempData["EnrollmentFeeFile"]     = Input.FeeSlipFile.FileName;
            TempData["EnrollmentDate"]        = DateTime.Now.ToString("MMMM d, yyyy");

            return RedirectToPage("/Student/EnrollmentSuccess");
        }

        private void LoadOptions()
        {
            // ── Replace with DB calls ────────────────────────────────────
            Programs = new List<string>
            {
                "BS Computer Science", "BS Software Engineering", "BS Information Technology",
                "BS Mathematics", "BS Physics", "BS Chemistry", "BS Botany", "BS Zoology",
                "MS Computer Science", "MS Software Engineering", "MS Data Science",
                "MPhil Mathematics", "MPhil Physics", "PhD Computer Science"
            };

            Courses = new List<CourseItem>
            {
                new() { Code="CS401", Name="Database Systems",        Credits=3, IsMandatory=true  },
                new() { Code="CS402", Name="Software Engineering",    Credits=3, IsMandatory=true  },
                new() { Code="CS403", Name="Computer Networks",       Credits=3, IsMandatory=true  },
                new() { Code="CS404", Name="Artificial Intelligence", Credits=3, IsMandatory=false },
                new() { Code="CS405", Name="Web Technologies",        Credits=3, IsMandatory=false },
                new() { Code="CS406", Name="Mobile App Development",  Credits=3, IsMandatory=false },
                new() { Code="CS407", Name="Cloud Computing",         Credits=3, IsMandatory=false },
            };

            Supervisors = new List<SupervisorItem>
            {
                new() { Id=1, Name="Dr. Asif Nawaz",   Department="Computer Science"      },
                new() { Id=2, Name="Dr. Sadia Riaz",   Department="Software Engineering"  },
                new() { Id=3, Name="Dr. Usman Ghani",  Department="Data Science"          },
                new() { Id=4, Name="Dr. Faiza Rehman", Department="Artificial Intelligence"},
            };
        }
    }
}

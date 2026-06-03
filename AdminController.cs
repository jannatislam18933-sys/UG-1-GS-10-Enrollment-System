using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnrollmentSystemAPI.Data;
using EnrollmentSystemAPI.Models;
using EnrollmentSystemAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using EnrollmentSystemAPI.Enums;
using System.Security.Claims;

namespace EnrollmentSystemAPI.Controllers
{
    [ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
   public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("create-clerk")]
        public IActionResult CreateClerk(CreateClerkDTO dto, string adminEmail)
        {
            var admin = _context.Users.FirstOrDefault(u => u.Email == adminEmail);

            if (admin == null || admin.Role != "Admin")
            {
                return Unauthorized(new { message = "Only admin can create clerk" });
            }

            var clerk = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Clerk",
                DepartmentID = dto.DepartmentID
            };

            _context.Users.Add(clerk);
            _context.SaveChanges();

            return Ok(new { message = "Clerk created successfully" });
        }
        [HttpGet("dashboard-summary")]
[HttpPost("open-enrollment")]
public async Task<IActionResult> OpenEnrollment()
{
    var settings = await _context.EnrollmentSettings.FirstOrDefaultAsync();
    if (settings == null)
    {
        settings = new EnrollmentSettings
        {
            IsEnrollmentOpen = true,
            OpenDate = DateTime.UtcNow
        };
        _context.EnrollmentSettings.Add(settings);
    }
    else
    {
        settings.IsEnrollmentOpen = true;
        settings.OpenDate = DateTime.UtcNow;
    }

    await _context.SaveChangesAsync();
    return Ok(new { message = "Enrollment opened successfully" });
}

[HttpPost("close-enrollment")]
public async Task<IActionResult> CloseEnrollment()
{
    var settings = await _context.EnrollmentSettings.FirstOrDefaultAsync();
    if (settings == null)
    {
        return BadRequest(new { message = "No enrollment cycle found" });
    }

    settings.IsEnrollmentOpen = false;
    settings.CloseDate = DateTime.UtcNow;

    await _context.SaveChangesAsync();
    return Ok(new { message = "Enrollment closed successfully" });
}

public IActionResult GetDashboardSummary()
{
    int totalStudents = _context.Users
        .Count(u => u.Role == "Student");

    int totalClerks = _context.Users
        .Count(u => u.Role == "Clerk");

    int totalDepartments = _context.Departments
        .Count();

    int totalPrograms = _context.DegreePrograms
        .Count();

    int totalSubjects = _context.Subjects
        .Count();

    int pendingFeeSlips = _context.FeeSlips
        .Count(f => f.Status == FeeSlipStatus.Pending);

    return Ok(new
    {
        TotalStudents = totalStudents,
        TotalClerks = totalClerks,
        TotalDepartments = totalDepartments,
        TotalPrograms = totalPrograms,
        TotalSubjects = totalSubjects,
        PendingFeeSlips = pendingFeeSlips
    });
}
[HttpGet("departments")]
public IActionResult GetDepartments()
{
    var departments = _context.Departments
        .Select(d => new
        {
            d.DepartmentID,
            d.Name
        })
        .ToList();

    return Ok(departments);
}
[HttpGet("clerks")]
public IActionResult GetClerks()
{
    var clerks = _context.Users
        .Include(u => u.Department)
        .Where(u => u.Role == "Clerk")
        .Select(u => new
        {
            u.UserID,
            u.Name,
            u.Email,

            Department = u.Department == null ? null : new
{
    u.Department.DepartmentID,
    u.Department.Name
}
        })
        .ToList();

    return Ok(clerks);
}
[HttpGet("students")]
public IActionResult GetStudents()
{
    var students = _context.Users
        .Include(u => u.Department)
        .Include(u => u.DegreeProgram)
        .Where(u => u.Role == "Student")
        .Select(u => new
        {
            u.UserID,
            u.Name,
            u.Email,
            u.CurrentSemester,

            Department = u.Department == null ? null : new
{
    u.Department.DepartmentID,
    u.Department.Name
},

DegreeProgram = u.DegreeProgram == null ? null : new
{
    u.DegreeProgram.DegreeProgramID,
    u.DegreeProgram.ProgramName
}
        })
        .ToList();

    return Ok(students);
}
[HttpGet("student-progress")]
public IActionResult GetStudentProgress()
{
    var students = _context.Users
        .Include(u => u.Department)
        .Include(u => u.DegreeProgram)
        .Where(u => u.Role == "Student")
        .Select(u => new
        {
            u.UserID,
            u.Name,
            u.Email,
            u.CurrentSemester,

            Department = u.Department == null ? null : new
            {
                u.Department.Name
            },

            DegreeProgram = u.DegreeProgram == null ? null : new
            {
                u.DegreeProgram.ProgramName,
                u.DegreeProgram.DegreeLevel
            }
        })
        .ToList();

    return Ok(students);
}
[HttpGet("enrollment-status")]
public IActionResult GetEnrollmentStatus()
{
    var settings = _context.EnrollmentSettings
        .FirstOrDefault();

    if (settings == null)
    {
        return Ok(new
        {
            IsEnrollmentOpen = false
        });
    }

    return Ok(new
    {
        settings.IsEnrollmentOpen,
        settings.OpenDate,
        settings.CloseDate
    });
}
[HttpPost("create-notice")]
public async Task<IActionResult> CreateNotice(
    CreateNoticeDTO dto)
{
    var notice = new Notice
    {
        Title = dto.Title,
        Description = dto.Description,
        DepartmentID = dto.DepartmentID,
        CreatedByRole = "Admin"
    };

    _context.Notices.Add(notice);

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Notice created successfully"
    });
}
[HttpGet("all-notices")]
public IActionResult GetAllNotices()
{
    var notices = _context.Notices
        .OrderByDescending(n => n.CreatedAt)
        .Select(n => new
        {
            n.NoticeID,
            n.Title,
            n.Description,
            n.CreatedAt,
            n.CreatedByRole,

            Department =
                n.Department != null
                ? n.Department.Name
                : "Global"
        })
        .ToList();

    return Ok(notices);
}
[HttpGet("all-feeslips")]
public IActionResult GetAllFeeSlips()
{
    var feeSlips = _context.FeeSlips
        .Where(f => !f.IsDeleted)
        .OrderByDescending(f => f.UploadedAt)
        .Select(f => new
        {
            f.FeeSlipID,

            StudentName =
                f.Student != null
                ? f.Student.Name
                : "Unknown",

            StudentEmail =
                f.Student != null
                ? f.Student.Email
                : "Unknown",

            Department =
                f.Student != null &&
                f.Student.Department != null
                ? f.Student.Department.Name
                : "Unknown",

            f.TransactionID,
            f.Status,
            f.UploadedAt,
            f.VerifiedAt
        })
        .ToList();

    return Ok(feeSlips);
}
[HttpGet("search-students")]
public IActionResult SearchStudents(
    string keyword)
{
    var students = _context.Users
        .Where(u =>
            u.Role == "Student" &&
            (
                u.Name.Contains(keyword) ||
                u.Email.Contains(keyword)
            )
        )
        .Select(u => new
        {
            u.UserID,
            u.Name,
            u.Email,
            u.CurrentSemester,

            Department =
                u.Department != null
                ? u.Department.Name
                : "Not Assigned",

            DegreeProgram =
                u.DegreeProgram != null
                ? u.DegreeProgram.ProgramName
                : "Not Assigned"
        })
        .ToList();

    return Ok(students);
}
[HttpGet("students-paginated")]
public IActionResult GetStudentsPaginated(
    int page = 1,
    int pageSize = 10)
{
    var query = _context.Users
        .Where(u => u.Role == "Student");

    var totalRecords = query.Count();

    var students = query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(u => new
        {
            u.UserID,
            u.Name,
            u.Email,
            u.CurrentSemester
        })
        .ToList();

    return Ok(new
    {
        page,
        pageSize,
        totalRecords,
        data = students
    });
}
[HttpDelete("delete-student/{id}")]
public async Task<IActionResult> SoftDeleteStudent(
    int id)
{
    var student = await _context.Users
        .FirstOrDefaultAsync(u =>
            u.UserID == id &&
            u.Role == "Student"
        );

    if (student == null)
    {
        return NotFound(new
        {
            message = "Student not found"
        });
    }

    student.IsDeleted = true;

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Student deleted successfully"
    });
}
[HttpGet("system-statistics")]
public IActionResult GetSystemStatistics()
{
    var totalStudents = _context.Users
        .Count(u =>
            u.Role == "Student" &&
            !u.IsDeleted);

    var totalClerks = _context.Users
        .Count(u => u.Role == "Clerk");

    var totalDepartments =
        _context.Departments.Count();

    var totalSubjects =
        _context.Subjects.Count();

    var totalEnrollments =
        _context.Enrollments.Count();

    var pendingFeeSlips =
        _context.FeeSlips.Count(f =>
            f.Status == Enums.FeeSlipStatus.Pending
        );

    return Ok(new
    {
        totalStudents,
        totalClerks,
        totalDepartments,
        totalSubjects,
        totalEnrollments,
        pendingFeeSlips
    });
}
    }
}

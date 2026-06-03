using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

using EnrollmentSystemAPI.Data;
using EnrollmentSystemAPI.DTOs;
using EnrollmentSystemAPI.Models;

namespace EnrollmentSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("upload-feeslip")]
        public async Task<IActionResult> UploadFeeSlip([FromForm] UploadFeeSlipDTO dto)
        {
            // Validate File
            if (dto.File == null || dto.File.Length == 0)
            {
                return BadRequest(new
                {
                    message = "No file uploaded"
                });
            }

            // Get Logged-in Student ID
            var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(studentIdClaim))
            {
                return Unauthorized();
            }

            int studentId = int.Parse(studentIdClaim);

            // Generate Unique File Name
            var fileName = Guid.NewGuid().ToString() +
                           Path.GetExtension(dto.File.FileName);

            // Create Full File Path
            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Uploads",
                "FeeSlips"
            );

            // Ensure Folder Exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fullPath = Path.Combine(folderPath, fileName);

            // Save File
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            // Save Record in Database
            var feeSlip = new FeeSlip
            {
                StudentID = studentId,
                FilePath = fileName,
                TransactionID = dto.TransactionID
            };

            _context.FeeSlips.Add(feeSlip);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Fee slip uploaded successfully"
            });
        
        }


[HttpPost("enroll-subject")]
public async Task<IActionResult> EnrollSubject(
    EnrollSubjectDTO dto)
{
    // Get Student ID
    var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(studentIdClaim))
    {
        return Unauthorized();
    }

    int studentId = int.Parse(studentIdClaim);

    // Get Student
    var student = await _context.Users
        .FirstOrDefaultAsync(u => u.UserID == studentId);

    if (student == null)
    {
        return NotFound(new
        {
            message = "Student not found"
        });
    }

    // Verify Subject Exists
    var subject = await _context.Subjects
        .FirstOrDefaultAsync(s =>
            s.SubjectID == dto.SubjectID &&
            s.DegreeProgramID == student.DegreeProgramID &&
            s.Semester == student.CurrentSemester
        );

    if (subject == null)
    {
        return BadRequest(new
        {
            message = "Invalid subject"
        });
    }

    // Prevent Duplicate Enrollment
    var alreadyEnrolled = await _context.Enrollments
        .AnyAsync(e =>
            e.StudentID == studentId &&
            e.SubjectID == dto.SubjectID
        );

    if (alreadyEnrolled)
    {
        return BadRequest(new
        {
            message = "Already enrolled in this subject"
        });
    }

    var enrollment = new Enrollment
    {
        StudentID = studentId,
        SubjectID = dto.SubjectID
    };

    _context.Enrollments.Add(enrollment);

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Subject enrolled successfully"
    });
}

[HttpGet("my-enrollments")]
public IActionResult GetMyEnrollments()
{
    // Get Logged-in Student ID
    var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(studentIdClaim))
    {
        return Unauthorized(new
        {
            message = "Invalid token"
        });
    }

    int studentId = int.Parse(studentIdClaim);

    // Fetch Student Enrollments
    var enrollments = _context.Enrollments
        .Include(e => e.Subject)
        .Where(e => e.StudentID == studentId)
        .Select(e => new
        {
            e.EnrollmentID,

            Subject = new
            {
                e.Subject.SubjectCode,
                e.Subject.SubjectName,
                e.Subject.CreditHours,
                e.Subject.Semester
            },

            e.EnrolledAt
        })
        .ToList();
// Check Enrollment Status
var enrollmentSettings = _context.EnrollmentSettings
    .FirstOrDefault();

if (enrollmentSettings == null ||
    !enrollmentSettings.IsEnrollmentOpen)
{
    return BadRequest(new
    {
        message = "Enrollment is currently closed"
    });
}
    return Ok(enrollments);
}
[HttpGet("my-feeslip-status")]
public IActionResult GetMyFeeSlipStatus()
{
    var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(studentIdClaim))
    {
        return Unauthorized();
    }

    int studentId = int.Parse(studentIdClaim);

    var feeSlips = _context.FeeSlips
        .Where(f => f.StudentID == studentId)
        .Select(f => new
        {
            f.FeeSlipID,
            f.TransactionID,
            f.Status,
            f.UploadedAt,
            f.VerifiedAt
        })
        .ToList();

    return Ok(feeSlips);
}
[HttpGet("profile")]
public async Task<IActionResult> GetProfile()
{
    var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(studentIdClaim))
    {
        return Unauthorized();
    }

    int studentId = int.Parse(studentIdClaim);

    var student = await _context.Users
        .Include(u => u.Department)
        .Include(u => u.DegreeProgram)
        .FirstOrDefaultAsync(u => u.UserID == studentId);

    if (student == null)
    {
        return NotFound(new
        {
            message = "Student not found"
        });
    }

    var enrollmentSettings = _context.EnrollmentSettings
    .FirstOrDefault();

if (enrollmentSettings == null ||
    !enrollmentSettings.IsEnrollmentOpen)
{
    return BadRequest(new
    {
        message = "Enrollment is currently closed"
    });
}

    return Ok(new
    {
        student.UserID,
        student.Name,
        student.Email,
        student.Role,

        Department =
            student.Department != null
            ? student.Department.Name
            : "Not Assigned",

        DegreeProgram =
            student.DegreeProgram != null
            ? student.DegreeProgram.ProgramName
            : "Not Assigned",

        student.CurrentSemester
    });
}
[HttpGet("semester-subjects")]
public IActionResult GetSemesterSubjects()
{
    // Get Student ID from JWT
    var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(studentIdClaim))
    {
        return Unauthorized(new
        {
            message = "Invalid token"
        });
    }

    int studentId = int.Parse(studentIdClaim);

    // Get Student
    var student = _context.Users
        .FirstOrDefault(u => u.UserID == studentId);

    if (student == null)
    {
        return NotFound(new
        {
            message = "Student not found"
        });
    }

    // Ensure Degree Program Exists
    if (student.DegreeProgramID == null)
    {
        return BadRequest(new
        {
            message = "Student degree program not assigned"
        });
    }

    // Fetch Semester Subjects
    var subjects = _context.Subjects
        .Where(s =>
            s.DegreeProgramID == student.DegreeProgramID &&
            s.Semester == student.CurrentSemester
        )
        .Select(s => new
        {
            s.SubjectID,

            Subject = new
            {
                s.SubjectCode,
                s.SubjectName,
                s.CreditHours,
                s.Semester
            }
        })
        .ToList();

    return Ok(subjects);
}
[HttpGet("academic-summary")]
public IActionResult GetAcademicSummary()
{
    // Get Student ID
    var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(studentIdClaim))
    {
        return Unauthorized(new
        {
            message = "Invalid token"
        });
    }

    int studentId = int.Parse(studentIdClaim);

    // Fetch Student
    var student = _context.Users
        .Include(u => u.Department)
        .Include(u => u.DegreeProgram)
        .FirstOrDefault(u => u.UserID == studentId);

    if (student == null)
    {
        return NotFound(new
        {
            message = "Student not found"
        });
    }

    return Ok(new
    {
        student.UserID,
        student.Name,
        student.Email,
        student.CurrentSemester,

        Department = student.Department == null ? null : new
        {
            student.Department.DepartmentID,
            student.Department.Name
        },

        DegreeProgram = student.DegreeProgram == null ? null : new
        {
            student.DegreeProgram.DegreeProgramID,
            student.DegreeProgram.ProgramName,
            student.DegreeProgram.DegreeLevel
        }
    });
}
[HttpGet("notices")]
public IActionResult GetNotices()
{
    // Get Student ID
    var studentIdClaim =
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(studentIdClaim))
    {
        return Unauthorized();
    }

    int studentId = int.Parse(studentIdClaim);

    // Get Student
    var student = _context.Users
        .FirstOrDefault(u => u.UserID == studentId);

    if (student == null)
    {
        return NotFound();
    }

    // Get Global + Department Notices
    var notices = _context.Notices
        .Where(n =>
            n.DepartmentID == null ||
            n.DepartmentID == student.DepartmentID
        )
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
[HttpPut("reupload-feeslip/{id}")]
public async Task<IActionResult> ReuploadFeeSlip(
    int id,
    [FromForm] UploadFeeSlipDTO dto)
{
    // Get Student ID
    var studentIdClaim =
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(studentIdClaim))
    {
        return Unauthorized();
    }

    int studentId = int.Parse(studentIdClaim);

    // Find Existing Fee Slip
    var feeSlip = await _context.FeeSlips
        .FirstOrDefaultAsync(f =>
            f.FeeSlipID == id &&
            f.StudentID == studentId &&
            f.Status == Enums.FeeSlipStatus.Rejected
        );

    if (feeSlip == null)
    {
        return BadRequest(new
        {
            message = "Rejected fee slip not found"
        });
    }

    // Validate File
    if (dto.File == null || dto.File.Length == 0)
    {
        return BadRequest(new
        {
            message = "No file uploaded"
        });
    }

    // Generate New File Name
    var fileName =
        Guid.NewGuid().ToString() +
        Path.GetExtension(dto.File.FileName);

    var folderPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "Uploads",
        "FeeSlips"
    );

    if (!Directory.Exists(folderPath))
    {
        Directory.CreateDirectory(folderPath);
    }

    var fullPath = Path.Combine(folderPath, fileName);

    using (var stream = new FileStream(fullPath, FileMode.Create))
    {
        await dto.File.CopyToAsync(stream);
    }

    // Update Existing Record
    feeSlip.FilePath = fileName;
    feeSlip.TransactionID = dto.TransactionID;
    feeSlip.Status = Enums.FeeSlipStatus.Pending;
    feeSlip.VerifiedAt = null;
    feeSlip.VerifiedByClerkID = null;
    feeSlip.UploadedAt = DateTime.Now;

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Fee slip re-uploaded successfully"
    });
}
[HttpGet("view-feeslip/{fileName}")]
public IActionResult ViewFeeSlip(
    string fileName)
{
    var folderPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "Uploads",
        "FeeSlips"
    );

    var fullPath = Path.Combine(folderPath, fileName);

    if (!System.IO.File.Exists(fullPath))
    {
        return NotFound(new
        {
            message = "File not found"
        });
    }

    var contentType = "application/pdf";

    return PhysicalFile(
        fullPath,
        contentType
    );
}
[HttpGet("dashboard")]
public IActionResult GetDashboard()
{
    var studentIdClaim =
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(studentIdClaim))
    {
        return Unauthorized();
    }

    int studentId = int.Parse(studentIdClaim);

    var student = _context.Users
        .FirstOrDefault(u =>
            u.UserID == studentId);

    if (student == null)
    {
        return NotFound();
    }

    var enrolledSubjects =
        _context.Enrollments.Count(e =>
            e.StudentID == studentId);

    var pendingFeeSlips =
        _context.FeeSlips.Count(f =>
            f.StudentID == studentId &&
            f.Status == Enums.FeeSlipStatus.Pending
        );

    var approvedFeeSlips =
        _context.FeeSlips.Count(f =>
            f.StudentID == studentId &&
            f.Status == Enums.FeeSlipStatus.Verified
        );

    var notices = _context.Notices.Count(n =>
        n.DepartmentID == null ||
        n.DepartmentID == student.DepartmentID
    );

    return Ok(new
    {
        student.Name,
        student.CurrentSemester,
        enrolledSubjects,
        pendingFeeSlips,
        approvedFeeSlips,
        notices
    });
}
    }
}
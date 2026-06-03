using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using System.Security.Claims;

using EnrollmentSystemAPI.Data;
using EnrollmentSystemAPI.Models;
using EnrollmentSystemAPI.DTOs;
using EnrollmentSystemAPI.Enums;

using Microsoft.EntityFrameworkCore;

namespace EnrollmentSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Clerk")]
    public class ClerkController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClerkController(AppDbContext context)
        {
            _context = context;
        }

        // GET STUDENTS OF CLERK DEPARTMENT
        [HttpGet("students")]
        public IActionResult GetDepartmentStudents()
        {
            var departmentClaim = User.FindFirst("DepartmentID")?.Value;

            if (string.IsNullOrEmpty(departmentClaim))
            {
                return BadRequest(new
                {
                    message = "Department not found in token"
                });
            }

            int departmentId = int.Parse(departmentClaim);

            var students = _context.Users
                .Where(u =>
                    u.Role == "Student" &&
                    u.DepartmentID == departmentId
                )
                .Select(u => new
                {
                    u.UserID,
                    u.Name,
                    u.Email,
                    u.DepartmentID
                })
                .ToList();

            return Ok(students);
        }

        // GET PENDING FEE SLIPS
        [HttpGet("pending-feeslips")]
        public IActionResult GetPendingFeeSlips()
        {
            var departmentClaim = User.FindFirst("DepartmentID")?.Value;

            if (string.IsNullOrEmpty(departmentClaim))
            {
                return Unauthorized();
            }

            int departmentId = int.Parse(departmentClaim);

            var feeSlips = _context.FeeSlips
                .Include(f => f.Student)
                .Where(f =>
                    f.Status == FeeSlipStatus.Pending &&
                    f.Student.DepartmentID == departmentId
                )
                .Select(f => new
                {
                    f.FeeSlipID,
                    StudentName = f.Student.Name,
                    StudentEmail = f.Student.Email,
                    f.TransactionID,
                    f.FilePath,
                    f.UploadedAt,
                    f.Status
                })
                .ToList();

            return Ok(feeSlips);
        }

        // VERIFY OR REJECT FEE SLIP
        [HttpPut("verify-feeslip/{id}")]
        public async Task<IActionResult> VerifyFeeSlip(
            int id,
            VerifyFeeSlipDTO dto)
        {
            var clerkIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(clerkIdClaim))
            {
                return Unauthorized();
            }
            

            int clerkId = int.Parse(clerkIdClaim);

            var departmentClaim = User.FindFirst("DepartmentID")?.Value;

            if (string.IsNullOrEmpty(departmentClaim))
            {
                return Unauthorized();
            }

            int departmentId = int.Parse(departmentClaim);

            var feeSlip = await _context.FeeSlips
                .Include(f => f.Student)
                .FirstOrDefaultAsync(f => f.FeeSlipID == id);

            if (feeSlip == null)
            {
                return NotFound(new
                {
                    message = "Fee slip not found"
                });
            }

            // SECURITY CHECK
            if (feeSlip.Student.DepartmentID != departmentId)
            {
                return Forbid();
            }

            feeSlip.Status = dto.Status;
            feeSlip.VerifiedAt = DateTime.Now;
            feeSlip.VerifiedByClerkID = clerkId;
            
            if (dto.Status == FeeSlipStatus.Verified)
{
    var enrollments =
        await _context.Enrollments
        .Where(e =>
            e.StudentID ==
            feeSlip.StudentID)
        .ToListAsync();

    foreach (var enrollment in enrollments)
    {
        enrollment.Status =
            "ReadyForReview";
    }
}

            if(dto.Status == FeeSlipStatus.Verified)
           {
            feeSlip.RejectionReason = null;
           }  
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Fee slip updated successfully"
            });
        }
[HttpPut("reject-feeslip/{id}")]
public async Task<IActionResult> RejectFeeSlip(
    int id,
    RejectFeeSlipDTO dto)
{
    var clerkIdClaim =
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(clerkIdClaim))
        return Unauthorized();

    int clerkId = int.Parse(clerkIdClaim);

    var feeSlip =
        await _context.FeeSlips
            .FirstOrDefaultAsync(f =>
                f.FeeSlipID == id);

    if (feeSlip == null)
        return NotFound();

    feeSlip.Status = FeeSlipStatus.Rejected;

    feeSlip.RejectionReason = dto.Reason;

    feeSlip.VerifiedAt = DateTime.Now;

    feeSlip.VerifiedByClerkID = clerkId;

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Fee slip rejected"
    });
}

[HttpGet("pending-enrollments")]
public IActionResult GetPendingEnrollments()
{
    var departmentClaim =
        User.FindFirst("DepartmentID")?.Value;

    if (string.IsNullOrEmpty(departmentClaim))
    {
        return Unauthorized();
    }

    int departmentId = int.Parse(departmentClaim);

    var enrollments = _context.Enrollments
        .Include(e => e.Student)
        .Include(e => e.Subject)
        .Where(e =>
            e.Student.DepartmentID == departmentId)
        .Select(e => new
        {
            Id = e.EnrollmentID.ToString(),

            StudentName = e.Student.Name,

            Email = e.Student.Email,

            RegistrationNo =
                e.Student.UserID.ToString(),

            Program =
                e.Student.DegreeProgram != null
                    ? e.Student.DegreeProgram.ToString()
                    : "N/A",

            CourseCount =
                _context.Enrollments.Count(x =>
                    x.StudentID ==
                    e.StudentID),

            FeeStatus =
                _context.FeeSlips
                .Where(f =>
                    f.StudentID ==
                    e.StudentID)
                .OrderByDescending(f =>
                    f.UploadedAt)
                .Select(f =>
                    f.Status.ToString())
                .FirstOrDefault() ??
                "Pending",

            Status = e.Status,

            SubmissionDate =
                e.EnrolledAt
                .ToString("dd MMM yyyy"),

            ReviewedDate =
                e.ReviewedAt.HasValue
                ? e.ReviewedAt.Value
                    .ToString("dd MMM yyyy")
                : null,

            FeeSlipUrl = ""
        })
        .ToList();

    return Ok(enrollments);
}

[HttpPut("approve-enrollment/{id}")]
public async Task<IActionResult> ApproveEnrollment(int id)
{
    var clerkId =
        int.Parse(User.FindFirst(
            ClaimTypes.NameIdentifier)!.Value);

    var enrollment =
        await _context.Enrollments
        .FirstOrDefaultAsync(e =>
            e.EnrollmentID == id);

    if (enrollment == null)
    {
        return NotFound();
    }

    enrollment.Status = "Approved";
    enrollment.ReviewedAt = DateTime.Now;
    enrollment.ReviewedByClerkID = clerkId;

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Enrollment approved"
    });
}

[HttpPut("reject-enrollment/{id}")]
public async Task<IActionResult> RejectEnrollment(
    int id,
    string reason)
{
    var clerkId =
        int.Parse(User.FindFirst(
            ClaimTypes.NameIdentifier)!.Value);

    var enrollment =
        await _context.Enrollments
        .FirstOrDefaultAsync(e =>
            e.EnrollmentID == id);

    if (enrollment == null)
    {
        return NotFound();
    }

    enrollment.Status = "Rejected";
    enrollment.RejectionReason = reason;
    enrollment.ReviewedAt = DateTime.Now;
    enrollment.ReviewedByClerkID = clerkId;

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Enrollment rejected"
    });
}

[HttpPost("create-degree-program")]
public async Task<IActionResult> CreateDegreeProgram(
    CreateDegreeProgramDTO dto)
{
    // Get Clerk Department from JWT
    var departmentClaim = User.FindFirst("DepartmentID")?.Value;

    if (string.IsNullOrEmpty(departmentClaim))
    {
        return Unauthorized();
    }

    int departmentId = int.Parse(departmentClaim);

    var degreeProgram = new DegreeProgram
    {
        ProgramName = dto.ProgramName,
        DegreeLevel = dto.DegreeLevel,
        DepartmentID = departmentId
    };

    _context.DegreePrograms.Add(degreeProgram);

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Degree program created successfully"
    });
}
[HttpPost("create-subject")]
public async Task<IActionResult> CreateSubject(
    CreateSubjectDTO dto)
{
    // Get Clerk Department
    var departmentClaim = User.FindFirst("DepartmentID")?.Value;

    if (string.IsNullOrEmpty(departmentClaim))
    {
        return Unauthorized();
    }

    int departmentId = int.Parse(departmentClaim);

    // Verify Degree Program belongs to clerk department
    var degreeProgram = await _context.DegreePrograms
        .FirstOrDefaultAsync(d =>
            d.DegreeProgramID == dto.DegreeProgramID &&
            d.DepartmentID == departmentId
        );

    if (degreeProgram == null)
    {
        return BadRequest(new
        {
            message = "Invalid degree program"
        });
    }

    var subject = new Subject
    {
        SubjectCode = dto.SubjectCode,
        SubjectName = dto.SubjectName,
        CreditHours = dto.CreditHours,
        Semester = dto.Semester,
        DegreeLevel = dto.DegreeLevel,
        DegreeProgramID = dto.DegreeProgramID
    };

    _context.Subjects.Add(subject);

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Subject created successfully"
    });
}
[HttpGet("dashboard-summary")]
public IActionResult DashboardSummary()
{
    var departmentClaim =
        User.FindFirst("DepartmentID")?.Value;

    if (string.IsNullOrEmpty(departmentClaim))
    {
        return Unauthorized();
    }

    int departmentId = int.Parse(departmentClaim);

    var totalStudents = _context.Users
        .Count(u =>
            u.Role == "Student" &&
            u.DepartmentID == departmentId &&
            !u.IsDeleted);

    var totalSubjects = _context.Subjects
        .Count(s =>
            s.DegreeProgram.DepartmentID ==
            departmentId);

    var pendingFeeSlips =
        _context.FeeSlips.Count(f =>
            f.Status == Enums.FeeSlipStatus.Pending &&
            f.Student.DepartmentID == departmentId
        );

    var verifiedFeeSlips =
        _context.FeeSlips.Count(f =>
            f.Status == Enums.FeeSlipStatus.Verified &&
            f.Student.DepartmentID == departmentId
        );

    var notices =
        _context.Notices.Count(n =>
            n.DepartmentID == departmentId
        );


    return Ok(new
    {
        totalStudents,
        totalSubjects,
        pendingFeeSlips,
        verifiedFeeSlips,
        notices
    });
}

[HttpGet("department-subjects")]
public IActionResult GetDepartmentSubjects()
{
    // Get Clerk Department from JWT
    var departmentClaim = User.FindFirst("DepartmentID")?.Value;

    if (string.IsNullOrEmpty(departmentClaim))
    {
        return Unauthorized(new
        {
            message = "Department not found in token"
        });
    }

    int departmentId = int.Parse(departmentClaim);

    // Fetch Department Subjects
    var subjects = _context.Subjects
        .Include(s => s.DegreeProgram)
        .Where(s =>
            s.DegreeProgram.DepartmentID == departmentId
        )
        .Select(s => new
        {
            s.SubjectID,
            s.SubjectCode,
            s.SubjectName,
            s.CreditHours,
            s.Semester,

            DegreeProgram = new
            {
                s.DegreeProgram.ProgramName,
                s.DegreeProgram.DegreeLevel
            }
        })
        .ToList();

    return Ok(subjects);
}
[HttpGet("verified-feeslips")]
public IActionResult GetVerifiedFeeSlips()
{
    // Get Clerk Department from JWT
    var departmentClaim = User.FindFirst("DepartmentID")?.Value;

    if (string.IsNullOrEmpty(departmentClaim))
    {
        return Unauthorized(new
        {
            message = "Department not found in token"
        });
    }

    int departmentId = int.Parse(departmentClaim);

    // Fetch Verified Fee Slips
    var feeSlips = _context.FeeSlips
        .Include(f => f.Student)
        .Where(f =>
            f.Status == FeeSlipStatus.Verified &&
            f.Student.DepartmentID == departmentId
        )
        .Select(f => new
        {
            f.FeeSlipID,

            Student = new
            {
                f.Student.Name,
                f.Student.Email
            },

            f.TransactionID,
            f.FilePath,
            f.UploadedAt,
            f.VerifiedAt
        })
        .ToList();

    return Ok(feeSlips);
}
[HttpGet("rejected-feeslips")]
public IActionResult GetRejectedFeeSlips()
{
    // Get Clerk Department from JWT
    var departmentClaim = User.FindFirst("DepartmentID")?.Value;

    if (string.IsNullOrEmpty(departmentClaim))
    {
        return Unauthorized(new
        {
            message = "Department not found in token"
        });
    }

    int departmentId = int.Parse(departmentClaim);

    // Fetch Rejected Fee Slips
    var feeSlips = _context.FeeSlips
        .Include(f => f.Student)
        .Where(f =>
            f.Status == FeeSlipStatus.Rejected &&
            f.Student.DepartmentID == departmentId
        )
        .Select(f => new
        {
            f.FeeSlipID,

            Student = new
            {
                f.Student.Name,
                f.Student.Email
            },

            f.TransactionID,
            f.FilePath,
            f.UploadedAt,
            f.VerifiedAt
        })
        .ToList();

    return Ok(feeSlips);
}
[HttpPut("promote-semester")]
public async Task<IActionResult> PromoteSemester(
    PromoteSemesterDTO dto)
{
    // Get Clerk Department
    var departmentClaim = User.FindFirst("DepartmentID")?.Value;

    if (string.IsNullOrEmpty(departmentClaim))
    {
        return Unauthorized(new
        {
            message = "Department not found in token"
        });
    }

    int departmentId = int.Parse(departmentClaim);

    // Find Student
    var student = await _context.Users
        .FirstOrDefaultAsync(u =>
            u.UserID == dto.StudentID &&
            u.Role == "Student"
        );

    if (student == null)
    {
        return NotFound(new
        {
            message = "Student not found"
        });
    }

    // Department Security Check
    if (student.DepartmentID != departmentId)
    {
        return Forbid();
    }

    // Validation
    if (dto.NewSemester < 1 || dto.NewSemester > 8)
    {
        return BadRequest(new
        {
            message = "Invalid semester"
        });
    }

    // Update Semester
    student.CurrentSemester = dto.NewSemester;

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Student promoted successfully",
        student.Name,
        student.CurrentSemester
    });
}
[HttpPost("create-notice")]
public async Task<IActionResult> CreateNotice(
    CreateNoticeDTO dto)
{
    // Get Clerk Department
    var departmentClaim =
        User.FindFirst("DepartmentID")?.Value;

    if (string.IsNullOrEmpty(departmentClaim))
    {
        return Unauthorized();
    }

    int departmentId = int.Parse(departmentClaim);

    var notice = new Notice
    {
        Title = dto.Title,
        Description = dto.Description,

        // Clerk can ONLY create department notices
        DepartmentID = departmentId,

        CreatedByRole = "Clerk"
    };

    _context.Notices.Add(notice);

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Notice created successfully"
    });
}
[HttpGet("department-notices")]
public IActionResult GetDepartmentNotices()
{
    var departmentClaim =
        User.FindFirst("DepartmentID")?.Value;

    if (string.IsNullOrEmpty(departmentClaim))
    {
        return Unauthorized();
    }

    int departmentId = int.Parse(departmentClaim);

    var notices = _context.Notices
        .Where(n => n.DepartmentID == departmentId)
        .OrderByDescending(n => n.CreatedAt)
        .Select(n => new
        {
            n.NoticeID,
            n.Title,
            n.Description,
            n.CreatedAt
        })
        .ToList();

    return Ok(notices);
}
    }
    
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnrollmentSystemAPI.Pages.Clerk
{
    /// <summary>
    /// Represents a single enrollment submission in the clerk's review queue.
    /// Every property maps directly to a column or modal field in EnrollmentReview.cshtml.
    /// </summary>
    public class EnrollmentItem
    {
        /// <summary>Primary key / unique ID used in the Review modal and API calls.</summary>
        public string Id { get; set; } = "";

        /// <summary>Displayed in the Name column and passed to openReviewModal().</summary>
        public string StudentName { get; set; } = "";

        /// <summary>Shown as a muted sub-line beneath the student name.</summary>
        public string Email { get; set; } = "";

        /// <summary>Displayed in the Reg No column (monospace style).</summary>
        public string RegistrationNo { get; set; } = "";

        /// <summary>Shown in the Program column and passed to openReviewModal().</summary>
        public string Program { get; set; } = "";

        /// <summary>
        /// Number of courses the student enrolled in.
        /// Shown in the Courses column and in the modal as "{CourseCount} courses selected".
        /// </summary>
        public int CourseCount { get; set; }

        /// <summary>
        /// Fee slip verification state.
        /// Expected values: "Verified" | "Pending" | "Rejected"
        /// Controls the Fee Status pill colour and the modal fee badge.
        /// </summary>
        public string FeeStatus { get; set; } = "Pending";

        /// <summary>
        /// Enrollment form review state.
        /// Expected values: "Pending" | "Approved" | "Rejected" | "FeeReview"
        /// Controls the Form Status pill and whether the Review button is shown.
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>Date the student submitted the form. Shown in the Submission Date column.</summary>
        public string SubmissionDate { get; set; } = "";

        /// <summary>
        /// Date the clerk reviewed the submission.
        /// Shown in the Actions column when Status is Approved or Rejected.
        /// Leave null / empty for unreviewed items.
        /// </summary>
        public string? ReviewedDate { get; set; }

        /// <summary>
        /// URL to the fee slip file stored on the server.
        /// Used in the modal's "View Fee Slip" link.
        /// Leave empty if the file is not yet accessible.
        /// </summary>
        public string FeeSlipUrl { get; set; } = "";
    }

    public class EnrollmentReviewModel : PageModel
    {
        // ── Page data ────────────────────────────────────────────────────
        public string ClerkName { get; set; } = "Clerk";

        /// <summary>
        /// The full list of enrollment submissions rendered in the table.
        /// Filter tabs (Pending / Approved / Rejected / FeeReview) work client-side
        /// on this list via data-status attributes — no extra server trips needed.
        /// </summary>
        public List<EnrollmentItem> Enrollments { get; set; } = new();
        private readonly IHttpClientFactory _clientFactory;
        public EnrollmentReviewModel(
        IHttpClientFactory clientFactory)
        {
        _clientFactory = clientFactory;
        }

        // ── GET ──────────────────────────────────────────────────────────
        public async Task<IActionResult> OnGet()        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
    {
        return RedirectToPage("/Account/Login");
    }
            ClerkName = HttpContext.Session.GetString("ClerkName") ?? "Clerk";

            // ── Replace with your real DB / service call ─────────────────
            // Enrollments = await _enrollmentService.GetQueueAsync(clerkDepartmentId);
            // ─────────────────────────────────────────────────────────────

    var client =
    _clientFactory.CreateClient("EnrollmentAPI");
    var token = HttpContext.Session.GetString("JwtToken");

    client.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue(
        "Bearer",
        token
    );
            
        var result = await client.GetFromJsonAsync<List<EnrollmentItem>>(
        "/api/clerk/pending-feeslips"
         );

Enrollments = result ?? new();
            return Page();

        }

        // ── POST handlers (called from the frontend via fetch, not form POST) ──
        // The modal uses JavaScript fetch() to call /api/enrollment/{id}/approve
        // or /api/enrollment/{id}/reject, so no OnPost is needed here.
        // Add your approve/reject logic in an API controller like this:
        //
        // [HttpPost("/api/enrollment/{id}/approve")]
        // public async Task<IActionResult> Approve(string id)
        // {
        //     await _enrollmentService.ApproveAsync(id, clerkId);
        //     await _notificationService.NotifyStudentApprovedAsync(id);
        //     return Ok();
        // }
        //
        // [HttpPost("/api/enrollment/{id}/reject")]
        // public async Task<IActionResult> Reject(string id, [FromBody] RejectRequest req)
        // {
        //     await _enrollmentService.RejectAsync(id, req.Reason, clerkId);
        //     await _notificationService.NotifyStudentRejectedAsync(id, req.Reason);
        //     return Ok();
        // }
    }
}

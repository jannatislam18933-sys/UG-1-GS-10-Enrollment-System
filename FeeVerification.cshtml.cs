using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

namespace EnrollmentSystemAPI.Pages.Clerk
{
    public class FeeSubmissionItem
    {
        public string Id             { get; set; } = "";
        public string StudentName    { get; set; } = "";
        public string Email          { get; set; } = "";
        public string RegistrationNo { get; set; } = "";
        public string UploadDate     { get; set; } = "";
        public string? TransactionId { get; set; }
        public string Status         { get; set; } = "Pending";
        public string? ReviewedDate  { get; set; }
        public string FeeSlipUrl     { get; set; } = "";
    }

    public class FeeVerificationModel : PageModel
    {
        public string ClerkName { get; set; } = "Clerk";
        public List<FeeSubmissionItem> Submissions { get; set; } = new();
        private readonly IHttpClientFactory _clientFactory;
        public FeeVerificationModel(
         IHttpClientFactory clientFactory)
        {
         _clientFactory = clientFactory;
        }
        
        
public async Task<IActionResult> OnGet()        {
        var client = 
        _clientFactory.CreateClient("EnrollmentAPI");

if (string.IsNullOrEmpty(
    HttpContext.Session.GetString("JwtToken")))
{
    return RedirectToPage("/Account/Login");
}

var token =
    HttpContext.Session.GetString("JwtToken");

client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue(
        "Bearer",
        token
    );
    var result =
    await client.GetFromJsonAsync<
        List<FeeSubmissionItem>>
(
    "/api/clerk/pending-feeslips"
);

Submissions = result ?? new();
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
    {
        return RedirectToPage("/Account/Login");
    }
            ClerkName = HttpContext.Session.GetString("ClerkName") ?? "Clerk";
            // ── Replace with DB ──────────────────────────────────────────
            
            return Page();

        }
    }
}

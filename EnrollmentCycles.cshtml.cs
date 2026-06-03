using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EnrollmentSystemAPI.Pages.Admin
{
    public class CycleItem
    {
        public string Id               { get; set; } = "";
        public string Name             { get; set; } = "";
        public string StartDate        { get; set; } = "";
        public string EndDate          { get; set; } = "";
        public string Status           { get; set; } = "Closed";
        public bool   EnrollmentOpen   { get; set; }
        public int    TotalEnrollments { get; set; }
        public int    DaysLeft         { get; set; }
    }

    public class EnrollmentCyclesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EnrollmentCyclesModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public string AdminName { get; set; } = "Admin";
        public List<CycleItem> Cycles { get; set; } = new();
        public bool IsEnrollmentOpen  { get; set; }
        public string? ErrorMessage   { get; set; }

        public async Task OnGetAsync()
        {
            AdminName = HttpContext.Session.GetString("AdminName") ?? "Admin";
            var token  = HttpContext.Session.GetString("JwtToken") ?? "";
            var client = BuildClient(token);

            // ── GET /api/Admin/enrollment-status ─────────────────────────
            try
            {
                var statusRes = await client.GetAsync("/api/Admin/enrollment-status");
                if (statusRes.IsSuccessStatusCode)
                {
                    var json   = await statusRes.Content.ReadAsStringAsync();
                    var status = JsonSerializer.Deserialize<JsonElement>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    IsEnrollmentOpen = status.TryGetProperty("isEnrollmentOpen", out var open)
                        && open.GetBoolean();

                    // Build a single cycle item from the enrollment settings
                    // (your backend stores one active cycle in EnrollmentSettings)
                    string openDate  = status.TryGetProperty("openDate",  out var od) && od.ValueKind != JsonValueKind.Null
                        ? DateTime.Parse(od.GetString()!).ToString("MMM dd, yyyy") : "—";
                    string closeDate = status.TryGetProperty("closeDate", out var cd) && cd.ValueKind != JsonValueKind.Null
                        ? DateTime.Parse(cd.GetString()!).ToString("MMM dd, yyyy") : "—";

                    int daysLeft = 0;
                    if (status.TryGetProperty("closeDate", out var cdRaw)
                        && cdRaw.ValueKind != JsonValueKind.Null)
                    {
                        var end = DateTime.Parse(cdRaw.GetString()!);
                        daysLeft = Math.Max(0, (int)(end - DateTime.UtcNow).TotalDays);
                    }

                    if (IsEnrollmentOpen)
                    {
                        Cycles.Add(new CycleItem
                        {
                            Id             = "current",
                            Name           = "Current Enrollment Period",
                            StartDate      = openDate,
                            EndDate        = closeDate,
                            Status         = "Active",
                            EnrollmentOpen = true,
                            DaysLeft       = daysLeft
                        });
                    }
                    else if (Cycles.Count == 0)
                    {
                        // No active cycle — show a closed entry
                        Cycles.Add(new CycleItem
                        {
                            Id        = "last",
                            Name      = "Last Enrollment Period",
                            StartDate = openDate,
                            EndDate   = closeDate,
                            Status    = "Closed"
                        });
                    }
                }
            }
            catch
            {
                ErrorMessage = "Could not load enrollment status from server.";
            }

            // ── GET /api/Admin/system-statistics (for TotalEnrollments) ──
            try
            {
                var statsRes = await client.GetAsync("/api/Admin/system-statistics");
                if (statsRes.IsSuccessStatusCode)
                {
                    var json  = await statsRes.Content.ReadAsStringAsync();
                    var stats = JsonSerializer.Deserialize<JsonElement>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    int total = stats.TryGetProperty("totalEnrollments", out var te)
                        ? te.GetInt32() : 0;

                    // Apply to the active cycle card
                    var active = Cycles.FirstOrDefault(c => c.Status == "Active");
                    if (active != null) active.TotalEnrollments = total;
                }
            }
            catch { /* stats load failed silently */ }
        }

        private HttpClient BuildClient(string token)
        {
            var client = _httpClientFactory.CreateClient("EnrollmentAPI");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}
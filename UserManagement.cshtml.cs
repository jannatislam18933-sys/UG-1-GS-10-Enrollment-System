using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EnrollmentSystemAPI.Pages.Admin
{
    public class UserItem
    {
        public string Id             { get; set; } = "";
        public string Name           { get; set; } = "";
        public string Email          { get; set; } = "";
        public bool   Active         { get; set; } = true;
        public string RegistrationNo { get; set; } = "";
        public string Program        { get; set; } = "";
        public string Department     { get; set; } = "";
        public string EmployeeId     { get; set; } = "";
        public string Role           { get; set; } = "Admin";
    }

    public class UserManagementModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserManagementModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public string AdminName { get; set; } = "Admin";
        public List<UserItem> Students   { get; set; } = new();
        public List<UserItem> Clerks     { get; set; } = new();
        public List<UserItem> Admins     { get; set; } = new();
        public List<DeptItem> Departments { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public class DeptItem
        {
            public int    DepartmentID { get; set; }
            public string Name         { get; set; } = "";
        }

        public async Task OnGetAsync()
        {
            AdminName = HttpContext.Session.GetString("AdminName") ?? "Admin";
            var token  = HttpContext.Session.GetString("JwtToken") ?? "";
            var client = BuildClient(token);

            // ── Load students from GET /api/Admin/students ───────────────
            try
            {
                var studentsRes = await client.GetAsync("/api/Admin/students");
                if (studentsRes.IsSuccessStatusCode)
                {
                    var json = await studentsRes.Content.ReadAsStringAsync();
                    var raw  = JsonSerializer.Deserialize<List<JsonElement>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    Students = raw?.Select(s => new UserItem
                    {
                        Id      = s.TryGetProperty("userID",  out var id)  ? id.ToString()  : "",
                        Name    = s.TryGetProperty("name",    out var n)   ? n.GetString()  ?? "" : "",
                        Email   = s.TryGetProperty("email",   out var e)   ? e.GetString()  ?? "" : "",
                        Program = s.TryGetProperty("degreeProgram", out var dp) && dp.ValueKind != JsonValueKind.Null
                                    ? (dp.TryGetProperty("programName", out var pn) ? pn.GetString() ?? "" : "")
                                    : "",
                        Department = s.TryGetProperty("department", out var d) && d.ValueKind != JsonValueKind.Null
                                    ? (d.TryGetProperty("name", out var dn) ? dn.GetString() ?? "" : "")
                                    : "",
                        Active = !( s.TryGetProperty("isDeleted", out var del) && del.GetBoolean() )
                    }).ToList() ?? new();
                }
            }
            catch { ErrorMessage = "Failed to load students."; }

            // ── Load clerks from GET /api/Admin/clerks ───────────────────
            try
            {
                var clerksRes = await client.GetAsync("/api/Admin/clerks");
                if (clerksRes.IsSuccessStatusCode)
                {
                    var json = await clerksRes.Content.ReadAsStringAsync();
                    var raw  = JsonSerializer.Deserialize<List<JsonElement>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    Clerks = raw?.Select(c => new UserItem
                    {
                        Id         = c.TryGetProperty("userID", out var id) ? id.ToString() : "",
                        Name       = c.TryGetProperty("name",   out var n)  ? n.GetString() ?? "" : "",
                        Email      = c.TryGetProperty("email",  out var e)  ? e.GetString() ?? "" : "",
                        Department = c.TryGetProperty("department", out var d) && d.ValueKind != JsonValueKind.Null
                                        ? (d.TryGetProperty("name", out var dn) ? dn.GetString() ?? "" : "")
                                        : "",
                        Active = true
                    }).ToList() ?? new();
                }
            }
            catch { /* clerks load failed silently */ }

            // ── Load departments for the Add Clerk modal dropdown ────────
            try
            {
                var deptRes = await client.GetAsync("/api/Department");
                if (deptRes.IsSuccessStatusCode)
                {
                    var json = await deptRes.Content.ReadAsStringAsync();
                    Departments = JsonSerializer.Deserialize<List<DeptItem>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
            }
            catch { /* dept load failed silently */ }

            // Admins — no dedicated endpoint in your controller, keep static
            Admins = new()
            {
                new() { Id="a1", Name="Dr. Ali Ahmed",
                        Email="ali.admin@university.edu",
                        Role="Super Admin", Active=true }
            };
        }

        // ── Helper: build HttpClient with JWT attached ───────────────────
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
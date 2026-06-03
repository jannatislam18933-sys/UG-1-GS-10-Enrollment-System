using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace EnrollmentSystemAPI.Pages.Account
{
    [IgnoreAntiforgeryToken]
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public LoginModel(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email address is required.")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required.")]
            [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
            public string Password { get; set; } = string.Empty;

            public bool RememberMe { get; set; }
        }

        // Matches your API's login response shape exactly
        private class LoginApiResponse
        {
            public string? message     { get; set; }
            public string? token       { get; set; }
            public string? Name        { get; set; }
            public string? Email       { get; set; }
            public string? Role        { get; set; }
            public int?    DepartmentID { get; set; }
        }

        public void OnGet()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
                Response.Redirect("/Student/Dashboard");
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine($"EMAIL = '{Input.Email}'");
            Console.WriteLine($"PASSWORD = '{Input.Password}'");
            Console.WriteLine("POST METHOD REACHED");
            if (!ModelState.IsValid)
{
    Console.WriteLine("MODEL INVALID");

    foreach (var item in ModelState)
    {
        foreach (var error in item.Value.Errors)
        {
            Console.WriteLine($"{item.Key}: {error.ErrorMessage}");
        }
    }

    return Page();
}

            try
            {
                // Build body matching your LoginDTO
                var loginDto = new { Email = Input.Email, Password = Input.Password };
                var content  = new StringContent(
                    JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");

                // Call your API
                Console.WriteLine("CREATING CLIENT");

                var client = _httpClientFactory.CreateClient("EnrollmentAPI");
                Console.WriteLine("CLIENT CREATED");

                Console.WriteLine($"BASE URL = {client.BaseAddress}");

                Console.WriteLine("CALLING LOGIN API");

                var response = await client.PostAsync("/api/auth/login", content);

                    Console.WriteLine($"STATUS CODE = {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errBody = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errObj = JsonSerializer.Deserialize<JsonElement>(errBody);
                        ErrorMessage = errObj.TryGetProperty("message", out var m)
                            ? m.GetString() ?? "Login failed."
                            : "Invalid email or password.";
                    }
                    catch { ErrorMessage = "Invalid email or password."; }
                    return Page();
                }

                var body   = await response.Content.ReadAsStringAsync();
                Console.WriteLine(body);
                var result = JsonSerializer.Deserialize<LoginApiResponse>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.token == null)
                {
                    ErrorMessage = "Authentication failed. Please try again.";
                    return Page();
                }
                
                // Store JWT + user info in session
                HttpContext.Session.SetString("JwtToken",     result.token);
                HttpContext.Session.SetString("StudentName", result.Name ?? "");               
                HttpContext.Session.SetString("StudentEmail", result.Email ?? Input.Email);
                HttpContext.Session.SetString("UserRole",     result.Role  ?? "Student");
                HttpContext.Session.SetString("DepartmentID", result.DepartmentID?.ToString() ?? "");

                // Route to correct portal based on role
                return result.Role?.ToLower() switch
                {
                    "admin" => RedirectToPage("/Admin/Dashboard"),
                    "clerk" => RedirectToPage("/Clerk/Dashboard"),
                    _       => RedirectToPage("/Student/Dashboard")
                };
                
            }
            catch (Exception ex)
{
    ErrorMessage = ex.ToString();
    return Page();
}
        }
    }
}
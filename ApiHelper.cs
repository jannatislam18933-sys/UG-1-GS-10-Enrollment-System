using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EnrollmentSystemAPI.Helpers
{
    /// <summary>
    /// Reusable helper for Razor Page PageModels to call the backend API.
    /// Automatically attaches the JWT token from session to every request.
    ///
    /// USAGE in any PageModel:
    ///
    ///   private readonly ApiHelper _api;
    ///   public MyPageModel(IHttpClientFactory factory, IHttpContextAccessor accessor)
    ///   {
    ///       _api = new ApiHelper(factory, accessor);
    ///   }
    ///
    ///   // GET:  var result = await _api.GetAsync<List<MyDto>>("/api/myendpoint");
    ///   // POST: var result = await _api.PostAsync<ResponseDto>("/api/myendpoint", requestBody);
    /// </summary>
    public class ApiHelper
    {
        private readonly IHttpClientFactory    _factory;
        private readonly IHttpContextAccessor  _accessor;

        public ApiHelper(IHttpClientFactory factory, IHttpContextAccessor accessor)
        {
            _factory  = factory;
            _accessor = accessor;
        }

        // ── GET ──────────────────────────────────────────────────────────
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            var client = BuildClient();
            var res    = await client.GetAsync(endpoint);
            if (!res.IsSuccessStatusCode) return default;
            var body = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        // ── POST ─────────────────────────────────────────────────────────
        public async Task<(bool Success, T? Data, string? Error)> PostAsync<T>(
            string endpoint, object requestBody)
        {
            var client  = BuildClient();
            var json    = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res     = await client.PostAsync(endpoint, content);
            var body    = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                string? errorMsg = null;
                try
                {
                    var err = JsonSerializer.Deserialize<JsonElement>(body);
                    errorMsg = err.TryGetProperty("message", out var m)
                        ? m.GetString() : "Request failed.";
                }
                catch { errorMsg = "Request failed."; }
                return (false, default, errorMsg);
            }

            var data = JsonSerializer.Deserialize<T>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return (true, data, null);
        }

        // ── Attach JWT from session ───────────────────────────────────────
        private HttpClient BuildClient()
        {
            var client = _factory.CreateClient("EnrollmentAPI");
            var token  = _accessor.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}

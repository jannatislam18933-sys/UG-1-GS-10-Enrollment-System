using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;

namespace EnrollmentSystemAPI.Pages.Account
{
     public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            await HttpContext.SignOutAsync();

            HttpContext.Session.Clear();
            
            return RedirectToPage("/Account/Login");
        }
    }
}

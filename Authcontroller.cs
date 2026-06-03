using Microsoft.AspNetCore.Mvc;
using EnrollmentSystemAPI.Data;
using EnrollmentSystemAPI.Models;
using EnrollmentSystemAPI.DTOs;
using EnrollmentSystemAPI.Services;

namespace EnrollmentSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        public AuthController(AppDbContext context, TokenService tokenService)
{
    _context = context;
    _tokenService = tokenService;
}

        [HttpPost("register")]
public IActionResult Register(RegisterDTO dto)
{
    var user = new User
    {
        Name = dto.Name,
        Email = dto.Email,
        
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        Role = "Student",
        DepartmentID = dto.DepartmentID   //  REQUIRED
    };

    _context.Users.Add(user);
    _context.SaveChanges();

    return Ok(new { message = "User registered successfully" });
}

        // LOGIN  METHOD 
        [HttpPost("login")]
        public IActionResult Login(LoginDTO dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);

            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }

           if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return BadRequest(new { message = "Invalid password" });
            }

           var token = _tokenService.CreateToken(user);

return Ok(new
{
    message = "Login successful",
    token = token,
    user.Name,
    user.Email,
    user.Role,
    user.DepartmentID
});
        }
    }
}
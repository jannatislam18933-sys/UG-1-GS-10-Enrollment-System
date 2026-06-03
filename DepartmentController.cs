using Microsoft.AspNetCore.Mvc;
using EnrollmentSystemAPI.Data;
using EnrollmentSystemAPI.Models;

namespace EnrollmentSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepartmentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult CreateDepartment(Department dept)
        {
            _context.Departments.Add(dept);
            _context.SaveChanges();

            return Ok(new { message = "Department created" });
        }

        [HttpGet]
        public IActionResult GetDepartments()
        {
            return Ok(_context.Departments.ToList());
        }
    }
}
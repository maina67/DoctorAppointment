using Microsoft.AspNetCore.Mvc;
using DoctorAppointmentSystem.Models;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class AdminAuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminAuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] Admin admin)
    {
        if (_context.Admins.Any(a => a.Email.ToLower() == admin.Email.ToLower()))
        {
            return BadRequest(new { message = "Admin already exists" });
        }

        _context.Admins.Add(admin);
        _context.SaveChanges();

        return Ok(new { message = "Admin registered successfully" });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] Admin admin)
    {
        var existing = _context.Admins
            .FirstOrDefault(a => a.Email.ToLower() == admin.Email.ToLower()
                              && a.Password == admin.Password);

        if (existing == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var token = GenerateFakeJwt(existing.Email, "Admin");

        return Ok(new
        {
            token,
            adminID = existing.AdminID,
            email = existing.Email,
            role = "Admin",
            message = "Admin login successful"
        });
    }

    private string GenerateFakeJwt(string email, string role)
    {
        var payload = new { sub = email, role };
        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        return "fakeheader." + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json)) + ".fakesignature";
    }
}

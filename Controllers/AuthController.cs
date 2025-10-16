using DoctorAppointmentSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace DoctorAppointmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Registration endpoint
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto data)
        {
            // Check if email already exists
            if (_context.Patients.Any(p => p.Email == data.Email))
                return BadRequest("Email already exists.");

            // Hash the password (if using hashing for security, which is recommended)
            var passwordHasher = new PasswordHasher<Patient>();
            var hashedPassword = passwordHasher.HashPassword(new Patient(), data.Password); // Use a new Patient instance

            // Create new patient with hashed password
            var newPatient = new Patient
            {
                Name = data.FirstName + " " + data.LastName,
                Contact = data.PhoneNumber,
                Email = data.Email,
                Password = hashedPassword // Save the hashed password
            };

            // Add the patient to the database
            _context.Patients.Add(newPatient);
            _context.SaveChanges();

            return Ok(new { name = newPatient.Name, message = "Registration successful!" });
        }

        // Login endpoint with JWT Token
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto login)
        {
            var user = _context.Patients.FirstOrDefault(p => p.Email == login.Email);

            if (user == null)
                return Unauthorized(new { message = "Invalid email." });

            // Verify the password
            var passwordHasher = new PasswordHasher<Patient>();
            var result = passwordHasher.VerifyHashedPassword(user, user.Password, login.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid password." });

            // Generate JWT Token
            var token = GenerateJwtToken(user);

            // Return token and user info in the response
            return Ok(new { token, name = user.Name, email = user.Email });
        }

        // Admin login endpoint (if you need it for future functionality)
        [HttpPost("admin-login")]
        public IActionResult AdminLogin([FromBody] AdminLoginDto adminLogin)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.Email == adminLogin.Email);

            if (admin == null)
                return Unauthorized(new { message = "Invalid admin email." });

            // Verify the password
            var passwordHasher = new PasswordHasher<Admin>();
            var result = passwordHasher.VerifyHashedPassword(admin, admin.Password, adminLogin.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid admin password." });

            // Generate JWT Token for Admin
            var token = GenerateJwtToken(admin);

            return Ok(new { token, name = admin.Name });
        }

        // Doctor Login endpoint with JWT Token
        [HttpPost("doctor-login")]
        public async Task<IActionResult> DoctorLogin([FromBody] DoctorLoginDto login)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == login.Email);
            if (doctor == null)
                return Unauthorized(new { message = "Invalid email or password" });

            if (!BCrypt.Net.BCrypt.Verify(login.Password, doctor.Password))
                return Unauthorized(new { message = "Invalid email or password" });

            // ✅ Generate JWT token
            var token = GenerateJwtToken(doctor, "doctor");

            return Ok(new
            {
                message = "Doctor login successful",
                doctorId = doctor.DoctorID,
                token
            });
        }

        // Updated GenerateJwtToken method to handle different roles (patient, doctor)
        private string GenerateJwtToken(dynamic user, string role)
        {
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Role, role)  // Add user role (patient or doctor)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Helper method to generate JWT Token
        private string GenerateJwtToken(Patient user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Helper method to generate JWT Token for Admin
        private string GenerateJwtToken(Admin admin)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, admin.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, admin.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateDoctorJwtToken(Doctor doctor)
        {
            var claims = new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, doctor.Email),
                    new Claim("DoctorID", doctor.DoctorID.ToString()), // ✅ Embed DoctorID in the token
                    new Claim(ClaimTypes.Name, doctor.Name),
                    new Claim(ClaimTypes.Role, "Doctor")
                };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }

    // Data transfer objects for login, registration, and admin login
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AdminLoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class DoctorLoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

}

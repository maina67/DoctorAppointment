using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoctorAppointmentSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoctorAppointmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [FormatFilter]
    public class DoctorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorsController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Get all doctors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetDoctors()
        {
            var doctors = await _context.Doctors
                .Select(d => new { d.DoctorID, d.Name })
                .ToListAsync();

            return Ok(doctors);
        }

        // ✅ Get a single doctor by ID (explicit route name avoids conflicts)
        [HttpGet("details/{DoctorID}")]
        public async Task<IActionResult> GetDoctorById(int DoctorID)
        {
            var doctor = await _context.Doctors
                .Where(d => d.DoctorID == DoctorID)
                .Select(d => new
                {
                    d.DoctorID,
                    d.Name,
                    d.Specialization,
                    d.Email,
                    d.Contact
                })
                .FirstOrDefaultAsync();

            if (doctor == null)
                return NotFound(new { message = "Doctor not found" });

            return Ok(doctor);
        }

        // ✅ Register doctor with password hashing
        [HttpPost("register")]
        public async Task<IActionResult> RegisterDoctor([FromBody] Doctor doctor)
        {
            try
            {
                if (!doctor.Name.StartsWith("Dr."))
                    doctor.Name = "Dr. " + doctor.Name;

                doctor.Password = BCrypt.Net.BCrypt.HashPassword(doctor.Password);

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Doctor registered successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error registering doctor", error = ex.Message });
            }
        }

        // ✅ Get all appointments for a specific doctor
        [HttpGet("{DoctorID}/appointments")]
        public async Task<IActionResult> GetDoctorAppointments(int DoctorID)
        {
            try
            {
                var doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorID == DoctorID);
                if (!doctorExists)
                    return NotFound(new { message = "Doctor not found" });

                var appointments = await _context.Appointments
                    .Where(a => a.DoctorID == DoctorID)
                    .Select(a => new
                    {
                        a.AppointmentID,
                        a.PatientID,
                        a.Date,
                        a.Time,
                        a.Status
                    })
                    .ToListAsync();

                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching appointments", error = ex.Message });
            }
        }

        // ✅ Mark appointment as completed
        [HttpPut("complete-appointment/{appointmentId}")]
        public async Task<IActionResult> CompleteAppointment(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                return NotFound(new { message = "Appointment not found." });

            appointment.Status = "Completed";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Appointment marked as completed." });
        }

        // ✅ List all doctors for admin
        [HttpGet("list")]
        public async Task<IActionResult> ListDoctors()
        {
            var doctors = await _context.Doctors
                .Select(d => new
                {
                    d.DoctorID,
                    d.Name,
                    d.Specialization,
                    d.Email,
                    d.Contact
                })
                .ToListAsync();

            return Ok(doctors);
        }

        // ✅ Delete doctor by ID
        [HttpDelete("{DoctorID}")]
        public async Task<IActionResult> DeleteDoctor(int DoctorID)
        {
            var doctor = await _context.Doctors.FindAsync(DoctorID);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found" });

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Doctor deleted successfully" });
        }

        // ✅ Get Doctor by Email
        [HttpGet("byemail/{email}")]
        public async Task<IActionResult> GetDoctorByEmail(string email)
        {
            var doctor = await _context.Doctors
                .Where(d => d.Email == email)
                .Select(d => new
                {
                    d.DoctorID,
                    d.Name,
                    d.Email,
                    d.Specialization
                })
                .FirstOrDefaultAsync();

            if (doctor == null)
                return NotFound(new { message = "Doctor not found" });

            return Ok(doctor);
        }
    }
}

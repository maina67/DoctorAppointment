using Microsoft.AspNetCore.Mvc;
using DoctorAppointmentSystem.Models;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Get all appointments
        [HttpGet]
        public IActionResult GetAll()
        {
            var appointments = _context.Appointments.ToList();
            return Ok(new
            {
                success = true,
                data = appointments
            });
        }

        // ✅ Book an appointment
        [HttpPost]
        public IActionResult Schedule([FromBody] Appointment appointment)
        {
            if (appointment == null || appointment.DoctorID == 0 || appointment.PatientID == 0 ||
                string.IsNullOrEmpty(appointment.Time) || appointment.Date == default)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid appointment data. Please fill all required fields."
                });
            }

            var doctor = _context.Doctors.Find(appointment.DoctorID);
            if (doctor == null)
            {
                return BadRequest(new { success = false, message = "Doctor not found." });
            }

            var patient = _context.Patients.Find(appointment.PatientID);
            if (patient == null)
            {
                return BadRequest(new { success = false, message = "Patient not found." });
            }

            try
            {
                appointment.Status = "Pending";
                appointment.Date = DateTime.SpecifyKind(appointment.Date, DateTimeKind.Utc);

                _context.Appointments.Add(appointment);
                _context.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Appointment booked successfully.",
                    appointment
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to book appointment: " + ex.Message,
                    error = ex.InnerException?.Message
                });
            }
        }

        // ✅ Update appointment status (accept only { status: "..." })
        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] StatusUpdateRequest request)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Appointment not found."
                });
            }

            appointment.Status = request.Status;
            _context.SaveChanges();

            return Ok(new
            {
                success = true,
                message = "Appointment status updated successfully.",
                appointment
            });
        }

        // ✅ Cancel an appointment
        [HttpDelete("{id}")]
        public IActionResult Cancel(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Appointment not found."
                });
            }

            _context.Appointments.Remove(appointment);
            _context.SaveChanges();

            return Ok(new
            {
                success = true,
                message = "Appointment cancelled."
            });
        }

        // ✅ Get appointments with patient & doctor names
        [HttpGet("with-details")]
        public IActionResult GetAppointmentsWithDetails()
        {
            var appointments = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Select(a => new
                {
                    AppointmentId = a.AppointmentID,
                    Date = a.Date,
                    Time = a.Time,
                    Status = a.Status,
                    PatientName = a.Patient != null ? a.Patient.Name : "N/A",
                    DoctorName = a.Doctor != null ? a.Doctor.Name : "N/A"
                })
                .ToList();

            return Ok(new
            {
                success = true,
                data = appointments
            });
        }
    }

    // ✅ DTO for status update
    public class StatusUpdateRequest
    {
        public string Status { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using DoctorAppointmentSystem.Models;
using System.Linq;

namespace DoctorAppointmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PatientController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_context.Patients.ToList());

        [HttpPost]
        public IActionResult Add(Patient patient)
        {
            _context.Patients.Add(patient);
            _context.SaveChanges();
            return Ok(patient);
        }

        [HttpGet("{id}/appointments")]
        public IActionResult GetPatientAppointments(int id)
        {
            var appointments = _context.Appointments
                .Where(a => a.PatientID == id)
                .ToList();

            return Ok(appointments);
        }

        [HttpGet("byemail/{email}")]
        public IActionResult GetPatientByEmail(string email)
        {
            var patient = _context.Patients.FirstOrDefault(p => p.Email == email);
            if (patient == null)
            {
                return NotFound(new { message = "Patient not found" });
            }

            return Ok(new { patientID = patient.PatientID });
        }

    }
}

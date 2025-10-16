using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentSystem.Models
{
    public class Doctor
    {
        public int DoctorID { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Specialization { get; set; } = string.Empty;

        public string Contact { get; set; } = string.Empty;

        [Column("email")]  // Ensuring the 'Email' property maps to the 'email' column in the database
        [Required]
        [EmailAddress]  // Optional: You can add the EmailAddress attribute to ensure email format validation
        public string Email { get; set; } = string.Empty;

        [Column("password")]
        public string Password { get; set; } = string.Empty;

    }
}

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentSystem.Models
{
    public class Appointment
    {
        public int AppointmentID { get; set; }
        public int PatientID { get; set; }
        public int DoctorID { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";

        // Navigation properties
        [ForeignKey("PatientID")]
        public Patient? Patient { get; set; } // Made nullable

        [ForeignKey("DoctorID")]
        public Doctor? Doctor { get; set; } // Made nullable
    }
}

using System;
namespace arztin.Models
{
	public class AppointmentRequest
	{
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public DateTime AppointmentTime { get; set; }
    }
}


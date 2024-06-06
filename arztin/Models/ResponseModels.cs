using System;
namespace arztin.Models
{
    public class CommonResponse
    {
        public string? Message { get; set; }
        public int HTTPStatus { get; set; }
        public object? Error { get; set; }
    }
    public class AllDoctorsResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Speciality { get; set; }
        public string? Location { get; set; }
        public string? Timing { get; set; }
        public string? Fees { get; set; }
        public string? Currency { get; set; }
        public int Experience { get; set; }
        public double Rating { get; set; }
    }
    public class PendingAppointmentsResponse
    {
        public int AppointmentId { get; set; }
        public string? PatientName { get; set; }
        public DateTime AppointmentTime { get; set; }
        public string? PatientProfilePhoto { get; set; }
    }
}


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
        public string? ProfilePhoto { get; set; }
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
    public class SignInResponse
    {
        public string access_token { get; set; } = "";
        public long expires_at { get; set; }
        public long expires_in { get; set; }
        public string refresh_token { get; set; } = "";
        public string token_type { get; set; } = "";
        public UserResponse? user { get; set; }
    }
    public class UserResponse
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string Email { get; set; } = "";
        public bool IsAdmin { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UserRole { get; set; }
        public string? Provider { get; set; }
        public bool IsActive { get; set; }
    }
    public class TokenResponse
    {
        public string AccessToken { get; set; } = "";
        public long ExpiresAt { get; set; }
        public long ExpiresIn { get; set; }
        public string RefreshToken { get; set; } = "";
        public string TokenType { get; set; } = "";
    }
    public class DashboardResponse
    {
        public int TotalAppointments { get; set; }
        public int UpcomingAppointments { get; set; }

    }
}


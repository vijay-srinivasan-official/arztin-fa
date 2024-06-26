﻿using System;
namespace arztin.Models
{
	public class AppointmentRequest
	{
        public int DoctorId { get; set; }
        public int? PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? PatientEmail { get; set; }
        public string? PatientPhone { get; set; }
        public string? AppointmentDate { get; set; }
        public string? AppointmentTime { get; set; }
    }
    public class DoctorRequest
    {
        public int Id { get; set; }
    }
    public class ApproveAppointmentRequest
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
    }
    public class RejectAppointmentRequest
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public required string RejectedReason { get; set; }
    }
    public class TimeSlotRequest
    {
        public int DoctorId { get; set; }
        public DateTime Date { get; set; }
    }
    public class SignInRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
    public class DashboardRequest
    {
        public int Id { get; set; }
    }
}


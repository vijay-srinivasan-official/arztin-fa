using System;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.ComponentModel.DataAnnotations.Schema;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace arztin.Models
{
	public class ArztinDataModels
	{
        public class Users
        {
            [Key]
            public int UserId { get; set; }

            [Required]
            public required string UserType { get; set; } // 'Patient' or 'Doctor'

            [Required]
            public required string UserRole { get; set; } // 'Admin' or 'User'

            [Required]
            [EmailAddress]
            public required string Email { get; set; }

            [Required]
            public required string Name { get; set; }

            [Phone]
            public required string Phone { get; set; }

            [Required]
            public required string PasswordHash { get; set; }

            [Required]
            public DateTime CreatedOn { get; set; }
        }

        public class Doctors
        {
            [Key]
            public int DoctorId { get; set; }

            public string? Name { get; set; }

            public string? Title { get; set; }

            public string? Specialty { get; set; }

            public string? Location { get; set; }

            public string? Timing { get; set; }

            public string? Fees { get; set; }

            public string? Currency { get; set; }

            public int Experience { get; set; }

            public double Rating { get; set; }

            [Required]
            public DateTime CreatedOn { get; set; }

            [Required]
            public int CreatedBy { get; set; }
        }

        public class Appointments
        {
            [Key]
            public int AppointmentId { get; set; }

            [Required]
            public int DoctorId { get; set; }

            [Required]
            public int PatientId { get; set; }

            public DateTime AppointmentTime { get; set; }

            [Required]
            public required string Status { get; set; } // 'Pending', 'Accepted', 'Rejected'

            public required string Comment { get; set; }

            [Required]
            public DateTime CreatedOn { get; set; }

            [Required]
            public int CreatedBy { get; set; }
        }

        public class Reviews
        {
            [Key]
            public int ReviewId { get; set; }

            [Required]
            public int DoctorId { get; set; }

            [Required]
            public int PatientId { get; set; }

            [Range(1, 5)]
            public int Rating { get; set; }

            public required string Comment { get; set; }

            [Required]
            public DateTime CreatedOn { get; set; }

            [Required]
            public int CreatedBy { get; set; }
        }
    }
}


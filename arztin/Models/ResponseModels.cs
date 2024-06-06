using System;
namespace arztin.Models
{
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
        public float Rating { get; set; }
    }
}


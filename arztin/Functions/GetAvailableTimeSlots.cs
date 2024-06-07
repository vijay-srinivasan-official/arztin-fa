using arztin.DataDomain;
using arztin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static arztin.Models.ArztinDataModels;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace arztin.Functions
{
    public class GetAvailableTimeSlots
    {
        private readonly ArztinDbContext _dbContext;
        private readonly ILogger<GetAvailableTimeSlots> _logger;

        public GetAvailableTimeSlots(ArztinDbContext dbContext, ILogger<GetAvailableTimeSlots> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [Function("GetAvailableTimeSlots")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest httpReq)
        {
            try
            {
                _logger.LogInformation($"GetAvailableTimeSlots: Started");

                string req;
                using (StreamReader reader = new(httpReq.Body))
                {
                    req = await reader.ReadToEndAsync();
                }

                TimeSlotRequest timeSlotRequest = JsonConvert.DeserializeObject<TimeSlotRequest>(req)!;

                if (timeSlotRequest == null)
                {
                    _logger.LogInformation("GetAvailableTimeSlots: Completed with Error");
                    return new BadRequestObjectResult("Invalid request");
                }

                var availableSlots = await GetAvailableTimeSlot(timeSlotRequest.DoctorId, timeSlotRequest.Date);
                return new OkObjectResult(availableSlots);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public async Task<List<DateTime>> GetAvailableTimeSlot(int doctorId, DateTime date)
        {
            var doctor = await _dbContext.Doctors.FindAsync(doctorId);
            if (doctor == null)
                throw new ArgumentException("Invalid doctor ID");

            var timings = doctor.Timing!.Split(" - ");
            if (timings.Length != 2)
                throw new ArgumentException("Invalid timing format");

            if (!DateTime.TryParseExact(timings[0], "h:mmtt", null, System.Globalization.DateTimeStyles.None, out DateTime startTime))
                throw new ArgumentException("Invalid start time format");

            if (!DateTime.TryParseExact(timings[1], "h:mmtt", null, System.Globalization.DateTimeStyles.None, out DateTime endTime))
                throw new ArgumentException("Invalid end time format");

            startTime = date.Date + startTime.TimeOfDay;
            endTime = date.Date + endTime.TimeOfDay;

            var bookedSlots = await _dbContext.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentTime.Date == date.Date && a.Status != "Rejected")
                .Select(a => a.AppointmentTime)
                .ToListAsync();

            var availableSlots = new List<DateTime>();
            DateTime currentTime = DateTime.Now;

            for (var slot = startTime; slot < endTime; slot = slot.AddHours(1))
            {
                if (!bookedSlots.Contains(slot) && (date.Date != currentTime.Date || slot > currentTime))
                {
                    availableSlots.Add(slot);
                }
            }

            return availableSlots;
        }
    }
}


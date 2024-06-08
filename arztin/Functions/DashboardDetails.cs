using arztin.DataDomain;
using arztin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace arztin.Functions
{
    public class DashboardDetails
    {
        private readonly ArztinDbContext _dbContext;
        private readonly ILogger<DashboardDetails> _logger;

        public DashboardDetails(ArztinDbContext dbContext, ILogger<DashboardDetails> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [Function("DashboardDetails")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest httpReq)
        {
            _logger.LogInformation($"DashboardDetails: Started");

            string req;
            using (StreamReader reader = new(httpReq.Body))
            {
                req = await reader.ReadToEndAsync();
            }

            DashboardRequest dashboardRequest = JsonConvert.DeserializeObject<DashboardRequest>(req)!;
            if (dashboardRequest == null)
            {
                _logger.LogInformation("DashboardDetails: Completed with Error");
                return new BadRequestObjectResult("Invalid request");
            }

            DashboardResponse response = new();

            int totalAppointments = await _dbContext.Appointments.Where(x => x.DoctorId == dashboardRequest.Id).CountAsync();
            int pendingAppointments = await _dbContext.Appointments.Where(x => x.DoctorId == dashboardRequest.Id && x.Status.ToLower() == "pending").CountAsync();
            int upcomingAppointments = await _dbContext.Appointments.Where(x => x.DoctorId == dashboardRequest.Id && x.Status.ToLower() != "pending" && x.AppointmentTime > DateTime.Now).CountAsync();

            response = new()
            {
                TotalAppointments = totalAppointments,
                PendingAppointments = pendingAppointments,
                UpcomingAppointments = upcomingAppointments
            };

            _logger.LogInformation("DashboardDetails: Completed successfully");
            return new OkObjectResult(response);

        }
    }
}


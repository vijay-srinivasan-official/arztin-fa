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
    public class UpcomingAppointments
    {
        private readonly ArztinDbContext _dbContext;
        private readonly ILogger<UpcomingAppointments> _logger;

        public UpcomingAppointments(ArztinDbContext dbContext, ILogger<UpcomingAppointments> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [Function("UpcomingAppointments")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest httpReq)
        {
            try
            {
                _logger.LogInformation($"UpcomingAppointments: Started");

                string req;
                using (StreamReader reader = new(httpReq.Body))
                {
                    req = await reader.ReadToEndAsync();
                }

                DoctorRequest doctorRequest = JsonConvert.DeserializeObject<DoctorRequest>(req)!;
                if (doctorRequest == null)
                {
                    _logger.LogInformation("UpcomingAppointments: Completed with Error");
                    return new BadRequestObjectResult("Invalid request");
                }

                var upcomingAppointments = await _dbContext.Appointments.Where(x => x.DoctorId == doctorRequest.Id && x.Status.ToLower() != "pending" && x.AppointmentTime > DateTime.Now).OrderByDescending(x => x.DoctorId).ToListAsync();

                var appointmentList = new List<PendingAppointmentsResponse>();
                foreach (var appointment in upcomingAppointments)
                {
                    var pendingAppointment = new PendingAppointmentsResponse
                    {
                        AppointmentId = appointment.AppointmentId,
                        AppointmentTime = appointment.AppointmentTime,
                        AppointmentStatus = appointment.Status,
                        PatientName = (from user in _dbContext.Users
                                       where (user.UserId == appointment.PatientId)
                                       select user.Name).Single(),
                        PatientProfilePhoto = (from user in _dbContext.Users
                                               where (user.UserId == appointment.PatientId)
                                               select user.ProfilePhoto).Single(),
                    };
                    appointmentList.Add(pendingAppointment);
                }

                _logger.LogInformation("UpcomingAppointments: Completed successfully");
                return new OkObjectResult(appointmentList);
            }
            catch (Exception ex)
            {
                CommonResponse commonResponse = new()
                {
                    HTTPStatus = 500,
                    Error = ex.Message,
                    Message = "Failure"
                };
                _logger.LogError($"Error booking appointment: {ex.Message}");
                _logger.LogInformation("UpcomingAppointments: Completed with error");
                return new OkObjectResult(commonResponse);
            }
        }
    }
}


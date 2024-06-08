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
    public class GetAllAppointments
    {
        private readonly ArztinDbContext _dbContext;
        private readonly ILogger<GetAllAppointments> _logger;

        public GetAllAppointments(ArztinDbContext dbContext, ILogger<GetAllAppointments> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [Function("GetAllAppointments")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest httpReq)
        {
            try
            {
                _logger.LogInformation($"GetAllAppointments: Started");

                string req;
                using (StreamReader reader = new(httpReq.Body))
                {
                    req = await reader.ReadToEndAsync();
                }

                DoctorRequest doctorRequest = JsonConvert.DeserializeObject<DoctorRequest>(req)!;
                if (doctorRequest == null)
                {
                    _logger.LogInformation("GetAllAppointments: Completed with Error");
                    return new BadRequestObjectResult("Invalid request");
                }

                var GetAllAppointments = await _dbContext.Appointments.Where(x => x.DoctorId == doctorRequest.Id).OrderByDescending(x => x.DoctorId).ToListAsync();

                var appointmentList = new List<PendingAppointmentsResponse>();
                foreach (var appointment in GetAllAppointments)
                {
                    var allAppointments = new PendingAppointmentsResponse
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
                    appointmentList.Add(allAppointments);
                }

                _logger.LogInformation("GetAllAppointments: Completed successfully");
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
                _logger.LogInformation("GetAllAppointments: Completed with error");
                return new OkObjectResult(commonResponse);
            }
        }
    }
}


using System.Text.RegularExpressions;
using arztin.DataDomain;
using arztin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static arztin.Models.ArztinDataModels;

namespace arztin.Functions
{
    public class GetPendingAppointments
    {
        private readonly ArztinDbContext _dbContext;
        private readonly ILogger<GetPendingAppointments> _logger;

        public GetPendingAppointments(ArztinDbContext dbContext, ILogger<GetPendingAppointments> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [Function("GetPendingAppointments")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest httpReq)
        {
            try
            {
                _logger.LogInformation($"GetPendingAppointments: Started");

                string req;
                using (StreamReader reader = new(httpReq.Body))
                {
                    req = await reader.ReadToEndAsync();
                }

                DoctorRequest doctorRequest = JsonConvert.DeserializeObject<DoctorRequest>(req)!;
                if (doctorRequest == null)
                {
                    _logger.LogInformation("GetPendingAppointments: Completed with Error");
                    return new BadRequestObjectResult("Invalid request");
                }

                var pendingAppointments = await _dbContext.Appointments.Where(x => x.DoctorId == doctorRequest.Id && x.Status.ToLower() == "pending").OrderByDescending(x => x.DoctorId).ToListAsync();

                var appointmentList = new List<PendingAppointmentsResponse>();
                foreach (var appointment in pendingAppointments)
                {
                    var pendingAppointment = new PendingAppointmentsResponse
                    {
                        AppointmentId = appointment.AppointmentId,
                        AppointmentTime = appointment.AppointmentTime,
                        PatientName = (from user in _dbContext.Users
                                       where (user.UserId == appointment.PatientId)
                                       select user.Name).Single(),
                        PatientProfilePhoto = (from user in _dbContext.Users
                                               where (user.UserId == appointment.PatientId)
                                               select user.ProfilePhoto).Single(),
                    };
                    appointmentList.Add(pendingAppointment);
                }

                _logger.LogInformation("GetPendingAppointments: Completed successfully");
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
                _logger.LogInformation("GetPendingAppointments: Completed with error");
                return new OkObjectResult(commonResponse);
            }
        }
    }
}


using arztin.DataDomain;
using arztin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static arztin.Models.ArztinDataModels;

namespace arztin.Functions
{
    public class BookAppointment
    {
        private readonly ArztinDbContext _dbContext;
        private readonly ILogger<BookAppointment> _logger;

        public BookAppointment(ArztinDbContext dbContext, ILogger<BookAppointment> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [Function("BookAppointment")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest httpReq)
        {
            try
            {
                _logger.LogInformation($"BookAppointment: Started");

                string req;
                using (StreamReader reader = new(httpReq.Body))
                {
                    req = await reader.ReadToEndAsync();
                }

                AppointmentRequest appointmentRequest = JsonConvert.DeserializeObject<AppointmentRequest>(req)!;
                if (appointmentRequest == null)
                {
                    _logger.LogInformation("BookAppointment: Completed with Error");
                    return new BadRequestObjectResult("Invalid request");
                }

                CommonResponse response = new();
                Appointments appointment = new()
                {
                    DoctorId = appointmentRequest.DoctorId,
                    PatientId = appointmentRequest.PatientId,
                    AppointmentTime = appointmentRequest.AppointmentTime,
                    Status = "Pending",
                    Comment = "Pending for Approval",
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = appointmentRequest.PatientId
                };
                await _dbContext.Appointments.AddAsync(appointment);
                await _dbContext.SaveChangesAsync();
                response.Message = "Success";
                response.Error = null;
                response.HTTPStatus = 200;
                _logger.LogInformation("BookAppointment: Completed Successfully");
                return new OkObjectResult(response);
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
                _logger.LogInformation("BookAppointment: Completed with error");
                return new OkObjectResult(commonResponse);
            }
        }
    }
}


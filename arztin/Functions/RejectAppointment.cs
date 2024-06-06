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
    public class RejectAppointment
    {
        private readonly ArztinDbContext _dbContext;
        private readonly ILogger<RejectAppointment> _logger;

        public RejectAppointment(ArztinDbContext dbContext, ILogger<RejectAppointment> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [Function("RejectAppointment")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest httpReq)
        {
            CommonResponse response = new();
            try
            {
                _logger.LogInformation($"RejectAppointment: Started");

                string req;
                using (StreamReader reader = new(httpReq.Body))
                {
                    req = await reader.ReadToEndAsync();
                }

                RejectAppointmentRequest rejectAppointmentRequest = JsonConvert.DeserializeObject<RejectAppointmentRequest>(req)!;
                if (rejectAppointmentRequest == null)
                {
                    _logger.LogInformation("RejectAppointment: Completed with Error");
                    return new BadRequestObjectResult("Invalid request");
                }

                var request = await _dbContext.Appointments.Where(x => x.AppointmentId == rejectAppointmentRequest.AppointmentId && x.Status.ToLower() == "pending").FirstOrDefaultAsync();

                if (request != null)
                {
                    if (request.DoctorId != rejectAppointmentRequest.DoctorId)
                    {
                        response = new()
                        {
                            HTTPStatus = 401,
                            Error = "You are not authorized to approve/reject this request.",
                            Message = "Failure"
                        };
                        _logger.LogInformation("RejectAppointment: Completed with error");
                        return new OkObjectResult(response);
                    }

                    if (request.Status.ToLower() == "rejectd")
                    {
                        response = new()
                        {
                            HTTPStatus = 204,
                            Error = "You have already rejectd this request.",
                            Message = "Failure"
                        };
                        _logger.LogInformation("RejectAppointment: Completed with error");
                        return new OkObjectResult(response);
                    }

                    request.Status = "Rejectd";
                    request.Comment = rejectAppointmentRequest.RejectedReason;
                    _dbContext.Appointments.Update(request);

                    await _dbContext.SaveChangesAsync();

                    response.Message = "Success";
                    response.Error = null;
                    response.HTTPStatus = 200;
                    _logger.LogInformation("RejectAppointment: Completed Successfully");
                    return new OkObjectResult(response);
                }

                response.Error = "No such appointment found!";
                response.Message = "Failure";
                response.HTTPStatus = 404;
                _logger.LogInformation("RejectAppointment: Completed with error");
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                response = new()
                {
                    HTTPStatus = 500,
                    Error = ex.Message,
                    Message = "Failure"
                };
                _logger.LogError($"Error booking appointment: {ex.Message}");
                _logger.LogInformation("RejectAppointment: Completed with error");
                return new OkObjectResult(response);
            }
        }
    }
}


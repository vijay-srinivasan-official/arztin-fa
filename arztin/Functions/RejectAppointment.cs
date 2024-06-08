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
        private readonly EmailService _emailService;
        private readonly ILogger<RejectAppointment> _logger;

        public RejectAppointment(ArztinDbContext dbContext, EmailService emailService, ILogger<RejectAppointment> logger)
        {
            _dbContext = dbContext;
            _emailService = emailService;
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

                    var subject = "Important: Appointment Rejection";

                    var userDetail = (from user in _dbContext.Users
                                      where (user.UserId == request.PatientId)
                                      select user).Single();
                    var message = $@"
                                    <!DOCTYPE html>
                                    <html lang='en'>
                                    <head>
                                        <meta charset='UTF-8'>
                                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                                        <title>Appointment Confirmation</title>
                                        <style>
                                            body {{
                                                font-family: Arial, sans-serif;
                                                background-color: #f4f4f4;
                                                margin: 0;
                                                padding: 0;
                                            }}
                                            .container {{
                                                width: 80%;
                                                margin: auto;
                                                background-color: #ffffff;
                                                padding: 20px;
                                                box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                                            }}
                                            .logo {{
                                                height: 35px;
                                            }}
                                            .header {{
                                                text-align: center;
                                                padding: 10px 0;
                                            }}
                                            .header img {{
                                                max-width: 150px;
                                            }}
                                            .content {{
                                                margin: 20px 0;
                                            }}
                                            .footer {{
                                                text-align: center;
                                                padding: 10px 0;
                                                font-size: 12px;
                                                color: #888888;
                                            }}
                                        </style>
                                    </head>
                                    <body>
                                        <div class='container'>
                                            <div class='header'>

                                            </div>
                                            <div class='content'>
                                                <h2>Appointment Rejected</h2>
                                                <p>Dear {userDetail.Name},</p>
                                                <p>We are sorry to inform you that your appointment has been rejected by our doctor.</p>
                                                <p><strong>Appointment Details:</strong></p>
                                                <p>Date & Time: {request.AppointmentTime}</p>
                                                <p>Reason: {rejectAppointmentRequest.RejectedReason}</p>
                                                <p>If you have any questions or need to reschedule, please do not hesitate to contact us.</p>
                                                <p>Thank you for choosing our services.</p>
                                                <p>Sincerely,</p>
                                                <p>Arztin Company</p>
                                            </div>
                                            <div class='footer'>
                                                <p>Arztin Company | contact@arztin.site</p>
                                            </div>
                                        </div>
                                    </body>
                                    </html>
                                    ";
                    _emailService.SendEmail(userDetail.Email, subject, message);

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


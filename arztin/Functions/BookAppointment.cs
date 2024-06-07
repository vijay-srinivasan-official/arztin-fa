using System.Globalization;
using System.Text;
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

                string dateTimeString = $"{appointmentRequest.AppointmentDate} {appointmentRequest.AppointmentTime}";
                string inputFormat = "yyyy-MM-dd h:mm tt";
                CultureInfo provider = CultureInfo.InvariantCulture;
                DateTime appointmentDateTime = DateTime.ParseExact(dateTimeString, inputFormat, provider);
                appointmentDateTime.AddHours(-5).AddMinutes(-30);

                bool slotCheck = await _dbContext.Appointments.Where(x => x.DoctorId == appointmentRequest.DoctorId && x.AppointmentTime == appointmentDateTime).AnyAsync();

                if (slotCheck)
                {
                    response.Message = "Failure";
                    response.Error = "Slot already booked, please choose a different slot";
                    response.HTTPStatus = 200;
                    _logger.LogInformation("BookAppointment: Completed with error");
                    return new OkObjectResult(response);
                }

                var patientId = appointmentRequest.PatientId;

                if (appointmentRequest.PatientId == null)
                {
                    if(appointmentRequest.PatientEmail == null || appointmentRequest.PatientName == null || appointmentRequest.PatientPhone == null)
                    {
                        response = new()
                        {
                            Message = "Failure",
                            Error = "Patient details cannot be empty",
                            HTTPStatus = 200
                        };
                        _logger.LogInformation("BookAppointment: Completed with error");
                        return new OkObjectResult(response);
                    }

                    bool patientEmailExists = await _dbContext.Users.Where(x => x.Email == appointmentRequest.PatientEmail).AnyAsync();

                    //if(patientEmailExists)
                    //{
                    //    response = new()
                    //    {
                    //        Message = "Failure",
                    //        Error = "Patient email already exists. Login to book appointment.",
                    //        HTTPStatus = 200
                    //    };
                    //    _logger.LogInformation("BookAppointment: Completed with error");
                    //    return new OkObjectResult(response);
                    //}

                    //Register new patient as user in system

                    if (!patientEmailExists)
                    {
                        Users user = new()
                        {
                            Name = appointmentRequest.PatientName!,
                            Email = appointmentRequest.PatientEmail!,
                            Phone = appointmentRequest.PatientPhone!,
                            PasswordHash = GenerateRandom(10),
                            UserType = "anonymous",
                            UserRole = "patient",
                            CreatedOn = DateTime.UtcNow,
                        };

                        await _dbContext.Users.AddAsync(user);
                        await _dbContext.SaveChangesAsync();
                    }

                    patientId = await _dbContext.Users.Where(x => x.Email == appointmentRequest.PatientEmail).Select(x => x.UserId).FirstOrDefaultAsync();
                }

                bool doctorExists = await _dbContext.Users.Where(x => x.UserId == appointmentRequest.DoctorId).AnyAsync();
                bool patientExists = await _dbContext.Users.Where(x => x.UserId == patientId).AnyAsync();

                if(!doctorExists || !patientExists)
                {
                    response.Message = "Failure";
                    response.Error = doctorExists ? "Patient doesn't exists in the system" : "Doctor doesn't exists in the system";
                    response.HTTPStatus = 200;
                    _logger.LogInformation("BookAppointment: Completed with error");
                    return new OkObjectResult(response);
                }

                Appointments appointment = new()
                {
                    DoctorId = appointmentRequest.DoctorId,
                    PatientId = (int)(appointmentRequest.PatientId ?? patientId)!,
                    AppointmentTime = appointmentDateTime,
                    Status = "Pending",
                    Comment = "Pending for Approval",
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = (int)(appointmentRequest.PatientId ?? patientId)!
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

        private static string GenerateRandom(int length)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder stringBuilder = new();
            Random random = new();

            for (int i = 0; i < length; i++)
            {
                int nextIndex = random.Next(validChars.Length);
                stringBuilder.Append(validChars[nextIndex]);
            }

            return stringBuilder.ToString();
        }
    }
}


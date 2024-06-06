using System.Text.Json;
using arztin.DataDomain;
using arztin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace arztin.Functions
{
    public class GetAllDoctors
    {
        private readonly ArztinDbContext _dbContext;
        private readonly ILogger<GetAllDoctors> _logger;

        public GetAllDoctors(ArztinDbContext dbContext, ILogger<GetAllDoctors> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [Function("GetAllDoctors")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest httpReq)
        {
            try
            {
                _logger.LogInformation($"GetAllDoctors: Started");

                var doctors = await _dbContext.Doctors.OrderByDescending(x => x.DoctorId).ToListAsync();

                if (doctors != null && doctors.Count > 0)
                {
                    var doctorDetailsList = new List<AllDoctorsResponse>();
                    foreach (var doctor in doctors)
                    {
                        var doctorDetails = new AllDoctorsResponse
                        {
                            Id = doctor.DoctorId,
                            Name = doctor.Name,
                            Title = doctor.Title,
                            Timing = doctor.Timing,
                            Fees = doctor.Fees,
                            Speciality = doctor.Specialty,
                            Experience = doctor.Experience,
                            Currency = doctor.Currency,
                            Location = doctor.Location,
                            Rating = doctor.Rating,

                        };
                        doctorDetailsList.Add(doctorDetails);
                    }
                    _logger.LogInformation("GetAllDoctors: Completed Successfully");
                    return new OkObjectResult(doctorDetailsList.ToArray());
                }
                else
                {
                    _logger.LogInformation("GetAllDoctors: Completed Successfully");
                    return new OkObjectResult(Array.Empty<AllDoctorsResponse>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unhandled error: {ex.Message}");
                _logger.LogInformation("GetAllDoctors: Completed with error");
                return new StatusCodeResult(500); // Internal Server Error
            }
        }
    }
}


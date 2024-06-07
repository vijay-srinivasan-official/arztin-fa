using arztin.Business.Authentication;
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
    public class SignIn
    {
        private readonly ArztinDbContext _dbContext;
        private readonly ILogger<SignIn> _logger;

        public SignIn(ArztinDbContext dbContext, ILogger<SignIn> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [Function("SignIn")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest httpReq)
        {
            _logger.LogInformation(message: "SignIn: Started with req {0}", httpReq);
            CommonResponse commonResponse = new();
            SignInResponse signInResponse = new();
            try
            {
                string req;
                using (StreamReader reader = new(httpReq.Body))
                {
                    req = await reader.ReadToEndAsync();
                }

                SignInRequest signInRequest = JsonConvert.DeserializeObject<SignInRequest>(req)!;
                var response = await _dbContext.Users.Where(u => u.Email == signInRequest.Email && u.PasswordHash == signInRequest.Password).FirstOrDefaultAsync();

                if (response != null)
                {
                    NewToken newToken = new(_dbContext);
                    TokenResponse token = await newToken.New(response.UserId);

                    signInResponse.access_token = token.AccessToken;
                    signInResponse.expires_at = token.ExpiresAt;
                    signInResponse.expires_in = token.ExpiresIn;
                    signInResponse.token_type = token.TokenType;
                    signInResponse.refresh_token = token.RefreshToken;

                    signInResponse.user = new UserResponse
                    {
                        Id = response.UserId,
                        UserName = response.Name!,
                        Email = response.Email!,
                        IsActive = true,
                        IsAdmin = response.UserRole.ToLower() == "admin",
                        UserRole = response.UserRole!,
                        Provider = "email",
                        CreatedOn = response.CreatedOn!
                    };

                    _logger.LogInformation("SignIn: Completed");
                    return new OkObjectResult(signInResponse!);
                }

                commonResponse = new()
                {
                    Message = "Failure",
                    Error = "Incorrect email or password.",
                    HTTPStatus = 401
                };
                _logger.LogInformation("SignIn: Completed");

                return new OkObjectResult(commonResponse!);
            }
            catch (Exception ex)
            {
                //SignUpException errorObject = JsonConvert.DeserializeObject<SignUpException>(ex.Message)!;
                _logger.LogError($"Error signing up: {ex.Message}");
                commonResponse.Error = ex.Message;
                commonResponse.Message = "Failure";
                _logger.LogInformation("SignIn: Completed with error");
                return new OkObjectResult(commonResponse);
            }
        }
    }
}


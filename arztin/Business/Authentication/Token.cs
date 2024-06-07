using System;
using arztin.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using arztin.DataDomain;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace arztin.Business.Authentication
{
    public class NewToken
    {
        private readonly ArztinDbContext _dbContext;

        public NewToken(ArztinDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TokenResponse> New(int UserId)
        {
            TokenResponse token = new();
            var tokenHandler = new JwtSecurityTokenHandler();

            //Create new token
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == UserId!);

            if (user == null)
            {
                return token;
            }

            string secretKey = GenerateRandom(32);
            string refreshToken = GenerateRandom(22);

            // Create claims
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user?.Name!),
                new Claim("legal_name", user?.Name!),
                new Claim("provider", "email"),
                new Claim("role", user?.UserRole!.ToString()!),
                //new Claim(ClaimTypes.Role, user?.IsAdmin.ToString()!),
                new Claim("id", user?.UserId.ToString()!),
                new Claim("aud", "authenticated"),
                new Claim("iss", "https://api.arztin.com/auth/v1"),
                new Claim("email", user?.Email!),
                new Claim("phone", user?.Phone.ToString()??""),
                new Claim("is_anonymous", false.ToString()),
            };

            // Define token expiration time
            var tokenExpiration = DateTime.UtcNow.AddHours(1);

            TimeSpan timeSpan = tokenExpiration - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Create token credentials, including signing key and algorithm
            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = tokenExpiration,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            // Create the JWT token
            var jwt = tokenHandler.CreateToken(tokenDescriptor);

            token = new()
            {
                AccessToken = tokenHandler.WriteToken(jwt),
                ExpiresAt = (long)timeSpan.TotalSeconds,
                ExpiresIn = 3600,
                RefreshToken = refreshToken,
                TokenType = "bearer"
            };

            return token;
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


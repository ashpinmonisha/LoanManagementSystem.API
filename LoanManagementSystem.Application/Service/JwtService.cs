
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LoanManagementSystem.Application.Service
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Existing Employee JWT Token
        public string GenerateToken(
            int employeeId,
            string employeeCode,
            string employeeName,
            string role)
        {
            var claims = new[]
            {
                new Claim("EmployeeId", employeeId.ToString()),
                new Claim("EmployeeCode", employeeCode),
                new Claim("EmployeeName", employeeName),
                new Claim(ClaimTypes.Role, role)
            };

            return GenerateJwtToken(claims);
        }

        // Customer JWT Token
        public string GenerateCustomerToken(
            int customerId,
            string email)
        {
            var claims = new[]
            {
                new Claim(
                    "CustomerId",
                    customerId.ToString()),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    customerId.ToString()),

                new Claim(
                    ClaimTypes.Email,
                    email),

                new Claim(
                    ClaimTypes.Role,
                    "Customer")
            };

            return GenerateJwtToken(claims);
        }

        // Common JWT Token Generation
        private string GenerateJwtToken(
            IEnumerable<Claim> claims)
        {
            var secretKey = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException(
                    "JWT Key is missing from configuration.");
            }

            if (string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience))
            {
                throw new InvalidOperationException(
                    "JWT Issuer or Audience is missing.");
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}

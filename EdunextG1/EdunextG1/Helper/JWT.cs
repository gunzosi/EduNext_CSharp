using EdunextG1.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;

namespace EdunextG1.Helper
{
    public class JWT
    {
        private readonly IConfiguration _configuration;

        public JWT(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerateToken(User user) 
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Keys"]);
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new(ClaimTypes.Name,user.UserName),
                    new(ClaimTypes.Role, user.Role),
                    new(ClaimTypes.Email, user.Email)
                }),
                // ------------------------- TIME TO LIVE -------------------------
                Expires = DateTime.UtcNow.AddHours(1),
                // ------------------------- TIME TO LIVE -------------------------

                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken() 
        {
            return Guid.NewGuid().ToString();
        }
    }
}

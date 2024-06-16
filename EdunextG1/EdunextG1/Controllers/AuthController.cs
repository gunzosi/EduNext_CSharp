using EdunextG1.Data;
using EdunextG1.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using EdunextG1.Models;

namespace EdunextG1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IConfiguration _configuration;

        public AuthController(DatabaseContext databaseContext, IConfiguration configuration)
        {
            _databaseContext = databaseContext;
            _configuration = configuration;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            try
            {
                var user = new User
                {
                    UserName = registerDTO.Username,
                    Email = registerDTO.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password),
                    Role = "User" // Default role
                };

                _databaseContext.Users.Add(user);
                await _databaseContext.SaveChangesAsync();

                return Ok(new
                {
                    data = user,
                    status = 200,
                    message = "User registered successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            try
            {
                // Check if the email and password are provided
                if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
                {
                    return BadRequest(new { message = "Email and Password cannot be null or empty" });
                }

                // Find the user by email
                var user = await _databaseContext.Users
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                // Validate the user existence and password
                if (user == null || string.IsNullOrWhiteSpace(user.Password) || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                {
                    return Unauthorized(new { message = "Invalid Email or Password" });
                }

                // Create JWT token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JWT:Key"]);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim(ClaimTypes.Email, user.Email)
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // Generate refresh token
                user.RefreshToken = Guid.NewGuid().ToString();
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _databaseContext.SaveChangesAsync();

                return Ok(new
                {
                    Token = tokenString,
                    RefreshToken = user.RefreshToken
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenRefresh tokenRefresh)
        {
            try
            {
                var user = await _databaseContext.Users.FirstOrDefaultAsync(u =>
                    u.RefreshToken == tokenRefresh.RefreshToken &&
                    u.RefreshTokenExpiryTime > DateTime.UtcNow
                );

                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid or expired refresh token." });
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JWT:Key"]);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim(ClaimTypes.Email, user.Email)
                    }),
                    Expires = DateTime.UtcNow.AddHours(24),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                user.RefreshToken = Guid.NewGuid().ToString();
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _databaseContext.SaveChangesAsync();

                return Ok(new
                {
                    Token = tokenString,
                    RefreshToken = user.RefreshToken
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

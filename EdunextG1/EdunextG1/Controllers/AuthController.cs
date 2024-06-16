using EdunextG1.Data;
using EdunextG1.DTO;
using EdunextG1.Helper;
using EdunextG1.Repository.IRepository;
using EdunextG1.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EdunextG1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;
        private readonly JWT _jwtHelper;
        private readonly IUserService _userService;

        public AuthController(DatabaseContext databaseContext, JWT jwtHelper, IUserService userService)
        {
            _databaseContext = databaseContext;
            _jwtHelper = jwtHelper;
            _userService = userService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            try
            {
                var user = await _userService.RegisterAsync(registerDTO);
                return Ok(new
                {
                    data = user,
                    status = 200,
                    message = "User registered successfully"
                });
            } catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            try
            {
                var user = await _databaseContext.Users
                    .FirstOrDefaultAsync(u => u.UserName == loginDto.Username);

                if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                {
                    return Unauthorized(new
                    {
                        message = "Invalid Username or Password"
                    });
                }

                var tokenString = _jwtHelper.GenerateToken(user);
                user.RefreshToken = _jwtHelper.GenerateRefreshToken();
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
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
        public async Task<IActionResult> RefreshToken (TokenRefresh tokenRefresh)
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

                var tokenString = _jwtHelper.GenerateToken(user);
                user.RefreshToken = _jwtHelper.GenerateRefreshToken();
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
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

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EdunextG1.Data;
using EdunextG1.Models;
using EdunextG1.DTO;
using EdunextG1.Services.IServices;
using EdunextG1.Helper;

namespace EdunextG1.Services
{
    public class UserService : IUserService
    {
        private readonly DatabaseContext _context;
        private readonly IConfiguration _configuration;
        private readonly JWT _jwtHelper;

        public UserService(DatabaseContext context, IConfiguration configuration, JWT jwtHelper)
        {
            _context = context;
            _configuration = configuration;
            _jwtHelper = jwtHelper;
        }

        public async Task<UserDTO> RegisterAsync(RegisterDTO registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                throw new Exception("Email is already taken.");
            }

            var user = new User
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = "User" // Mặc định tất cả người dùng mới sẽ là User
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDTO
            {
                Username = user.UserName,
                Role = user.Role
            };
        }

        public async Task<string> LoginAsync(LoginDTO loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                throw new Exception("Invalid email or password.");
            }

            // Tạo JWT token sử dụng JwtHelper
            return _jwtHelper.GenerateToken(user);
        }
    }
}

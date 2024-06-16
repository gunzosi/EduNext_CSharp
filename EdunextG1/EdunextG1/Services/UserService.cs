using EdunextG1.Data;
using EdunextG1.DTO;
using EdunextG1.Models;
using EdunextG1.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace EdunextG1.Services
{
    public class UserService : IUserService
    {
        private readonly DatabaseContext _dbContext;
        private readonly IConfiguration configuration;

        public UserService(DatabaseContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            this.configuration = configuration;
        }

        // Register a new user
        public async Task<UserDTO> RegisterAsync(RegisterDTO registerDTO)
        {
            if (await _dbContext.Users.AnyAsync(u => u.UserName == registerDTO.Username))
            {
                throw new Exception("Username already exists");
            }

            var user = new User
            {
                UserName = registerDTO.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password),
                Email = registerDTO.Email,
                Role = "User" // Set Default "User" Role
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();


            return new UserDTO
            {
                Username = user.UserName,
                Role = user.Role
            };
        }

        public async Task<string?> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == loginDTO.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.Password))
            {
                throw new Exception("Invalid Username or Password");
            }

            /**
             * @JWT - sẽ trả về JWT ở đây
             * JWT class được viết trong Helper/JWT.cs
             */
            return null;
        }
    }
}

using EdunextG1.DTO;

namespace EdunextG1.Services.IServices
{
    public interface IUserService
    {
        Task<UserDTO> RegisterAsync(RegisterDTO registerDTO);
        Task<string> LoginAsync(LoginDTO loginDTO);
    }
}

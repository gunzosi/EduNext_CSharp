using EdunextG1.Models;
using EdunextG1.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EdunextG1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository) {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userRepository.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userRepository.GetUserById(id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(int id, User user)
        {
            if (id != user.Id) {
                return BadRequest(new { message = "Invalid User ID" });
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var userExisted = await _userRepository.GetUserById(id);
                    if (userExisted == null)
                    {
                        return NotFound(new { message = "User not found" });
                    }
                    var userUpdated = await _userRepository.UpdateUser(user);
                    return Ok(new
                    {
                        data = userUpdated,
                        status = 201,
                        message = "User updated successfully"
                    });
                }
                return BadRequest(new { message = "Invalid User Data" });
            } catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }

        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var userExisted = await _userRepository.GetUserById(id);
                if (userExisted == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                await _userRepository.DeleteUser(id);
                return Ok(new { message = "User deleted successfully" });

            } catch (Exception ex)
            {
                return StatusCode(500, new {message = ex.Message});
            }
        }

    }
}

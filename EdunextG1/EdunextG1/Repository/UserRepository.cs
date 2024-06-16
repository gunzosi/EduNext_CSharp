using Azure.Identity;
using EdunextG1.Data;
using EdunextG1.DTO;
using EdunextG1.Models;
using EdunextG1.Repository.IRepository;
using EdunextG1.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EdunextG1.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext _databaseContext;

        public UserRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _databaseContext.Users.ToListAsync();
        }

        public async Task<User> GetUserById(int id)
        {
            return await _databaseContext.Users.FindAsync(id);
        }

        public async Task<User> AddUser(User user)
        {
            _databaseContext.Users.Add(user);
            await _databaseContext.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateUser(User user)
        {
            // ModelState.IsValid is not used in this context because it is used in the controller
            // ModelState.IsValid is used to check if the model is valid in the controller
            // In this we use, .Entry(user).State for checking the state of the entity
            _databaseContext.Entry(user).State = EntityState.Modified;
            await _databaseContext.SaveChangesAsync();
            return user;
        }

        public async Task DeleteUser(int id)
        {
            var user = await _databaseContext.Users.FindAsync(id);
            if (user != null)
            {
                _databaseContext.Users.Remove(user);
                await _databaseContext.SaveChangesAsync();
            }
        }
    }
}

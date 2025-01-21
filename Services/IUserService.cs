using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;


namespace backend.Services
{
    public interface IUserService
    {
    Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByIdAsync(int id);              
        Task<List<User>> GetAllUsersAsync();              
        Task<User> CreateUserAsync(User user);            
        Task<User?> UpdateUserAsync(int id, User user);   
        Task<bool> DeleteUserAsync(int id);              
    }
}

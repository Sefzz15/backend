using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Get User By ID
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        // Get All Users
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

public async Task<User?> GetUserByUsernameAsync(string username)
{
    return await _context.Users
        .FirstOrDefaultAsync(u => u.uname == username);
}

        // Create user using hashing
        public async Task<User> CreateUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // Hash password
            user.upass = BCrypt.Net.BCrypt.HashPassword(user.upass);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        // Update user
        public async Task<User?> UpdateUserAsync(int id, User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return null;

            // Update user properties
            user.uname = updatedUser.uname;

            // if password is not empty, hash it
            if (!string.IsNullOrEmpty(updatedUser.upass))
            {
                user.upass = BCrypt.Net.BCrypt.HashPassword(updatedUser.upass);
            }

            await _context.SaveChangesAsync();
            return user;
        }

        // Delete user
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

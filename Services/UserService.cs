using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class UserService(AppDbContext context)
{
    public async Task<IEnumerable<User>> GetAllUsers()
    {
        return await context.Users.ToListAsync();
    }

    public async Task<User?> GetUserById(int id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<int?> GetUserIdByUsernameAsync(string username)
    {
        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Uname == username);

        return user?.Uid; // Return the user ID or null if not found.
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Uname == username);
    }

    public async Task AddUser(User user)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await context.Users.AnyAsync(u => u.Uname == username);
    }


    public async Task UpdateUser(User user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    public async Task DeleteUser(int id)
    {
        User? user = await context.Users.FindAsync(id);
        if (user != null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        // Check if the user exists and the password matches
        return await context.Users
            .FirstOrDefaultAsync(u => u.Uname == username && u.Upass == password);
    }
}
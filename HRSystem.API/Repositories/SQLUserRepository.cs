using HRSystem.API.Data;
using HRSystem.API.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Repositories
{
    public class SQLUserRepository : IUserRepository
    {
        private readonly UserManager<User> userManager;
        private readonly HRSystemDBContext dbContext;

        public SQLUserRepository(UserManager<User> userManager, HRSystemDBContext dbContext)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await dbContext.Users.ToListAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await dbContext.Users.FindAsync(id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> AddAsync(User user, string password)
        {
            // Check for duplicate email (case-insensitive)
            var normalizedEmail = userManager.NormalizeEmail(user.Email);
            if (await dbContext.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail))
                throw new InvalidOperationException("A user with this email already exists.");

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));

            return user;
        }

        public async Task<User> UpdateAsync(Guid id, User user)
        {
            var existingUser = await dbContext.Users.FindAsync(id);
            if (existingUser == null)
                throw new KeyNotFoundException("User not found.");

            existingUser.Fullname = user.Fullname;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.UserType = user.UserType;
            existingUser.AccessLevel = user.AccessLevel;
            existingUser.Specialty = user.Specialty;

            dbContext.Users.Update(existingUser);
            await dbContext.SaveChangesAsync();
            return existingUser;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await dbContext.Users.FindAsync(id);
            if (user == null)
                return false;

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
            return true;
        }
    }
}

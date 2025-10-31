using HRSystem.API.Models.Domain;

namespace HRSystem.API.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User> AddAsync(User user, string password);
        Task<User> UpdateAsync(Guid id, User user);
        Task<bool> DeleteAsync(Guid id);
    }
}
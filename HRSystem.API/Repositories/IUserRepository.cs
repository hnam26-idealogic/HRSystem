using HRSystem.API.Models.Domain;

namespace HRSystem.API.Repositories
{
    public interface IUserRepository
    {
    Task<(IEnumerable<User> Items, int TotalCount)> GetAllAsync(int page = 1, int size = 10);
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User> AddAsync(User user, string password);
        Task<User> UpdateAsync(Guid id, User user);
        Task<bool> DeleteAsync(Guid id);
    }
}
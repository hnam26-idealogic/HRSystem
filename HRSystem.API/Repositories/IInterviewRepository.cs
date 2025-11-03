using HRSystem.API.Models.Domain;

namespace HRSystem.API.Repositories
{
    public interface IInterviewRepository
    {
    Task<(IEnumerable<Interview> Items, int TotalCount)> GetAllAsync(int page = 1, int size = 10);
        Task<Interview?> GetByIdAsync(Guid id);
        Task<Interview> AddAsync(Interview interview);
        Task<Interview> UpdateAsync(Guid id, Interview interview);
        Task<bool> DeleteAsync(Guid id);
    }
}

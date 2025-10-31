using HRSystem.API.Models.Domain;

namespace HRSystem.API.Repositories
{
    public interface IInterviewRepository
    {
        Task<IEnumerable<Interview>> GetAllAsync();
        Task<Interview?> GetByIdAsync(Guid id);
        Task<Interview> AddAsync(Interview interview);
        Task<Interview> UpdateAsync(Guid id, Interview interview);
        Task<bool> DeleteAsync(Guid id);
    }
}

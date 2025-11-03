using HRSystem.API.Models.Domain;

namespace HRSystem.API.Repositories
{
    public interface ICandidateRepository
    {
    Task<IEnumerable<Candidate>> GetAllAsync(int page = 1, int size = 10);
        Task<Candidate?> GetByIdAsync(Guid id);
        Task<Candidate?> GetByEmailAsync(string email);
        Task<Candidate> AddAsync(Candidate candidate);
        Task<Candidate> UpdateAsync(Guid id, Candidate candidate);
        Task<bool> DeleteAsync(Guid id);
    }
}

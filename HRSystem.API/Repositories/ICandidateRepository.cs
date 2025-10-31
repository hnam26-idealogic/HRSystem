using HRSystem.API.Models.Domain;

namespace HRSystem.API.Repositories
{
    public interface ICandidateRepository
    {
        Task<IEnumerable<Candidate>> GetAllAsync();
        Task<Candidate?> GetByIdAsync(Guid id);
        Task<Candidate?> GetByEmailAsync(string email);
        Task<Candidate> AddAsync(Candidate candidate);
        Task<Candidate> UpdateAsync(Guid id, Candidate candidate);
        Task<bool> DeleteAsync(Guid id);
    }
}

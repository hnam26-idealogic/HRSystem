using HRSystem.API.Models.Domain;

namespace HRSystem.API.Services
{
    public interface IAzureSearchService
    {
        Task<(IEnumerable<Candidate> Items, int TotalCount)> SearchCandidatesAsync(string query, int page = 1, int size = 10);
        Task IndexCandidateAsync(Candidate candidate);
        Task UpdateCandidateIndexAsync(Candidate candidate);
        Task DeleteCandidateFromIndexAsync(Guid candidateId);
    }
}

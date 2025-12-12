using HRSystem.UI.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace HRSystem.UI.Services
{
    public interface ICandidateService
    {
        Task<bool> AddAsync(AddCandidateRequestDto dto, IBrowserFile resumeFile);
        Task<bool> DeleteAsync(Guid id);
        Task<PagedResult<CandidateDto>> GetAllAsync(int page = 1, int size = 10);
        Task<CandidateDto> GetByIdAsync(Guid id);
        Task<string> GetResumeUrlAsync(Guid candidateId);
        Task<PagedResult<CandidateDto>> SearchAsync(string query, int page = 1, int size = 10);
        Task<bool> UpdateAsync(Guid id, UpdateCandidateRequestDto dto, IBrowserFile resumeFile);
    }
}
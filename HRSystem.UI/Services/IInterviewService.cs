using HRSystem.UI.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace HRSystem.UI.Services
{
    public interface IInterviewService
    {
        Task<bool> AddAsync(AddInterviewRequestDto dto, IBrowserFile recordingFile);
        Task<bool> DeleteAsync(Guid id);
        Task<PagedResult<InterviewDto>> GetAllAsync(int page = 1, int size = 10);
        Task<InterviewDto> GetByIdAsync(Guid id);
        Task<PagedResult<InterviewDto>> SearchAsync(string query, int page = 1, int size = 10);
        Task<bool> UpdateAsync(Guid id, UpdateInterviewRequestDto dto, IBrowserFile recordingFile);
    }
}
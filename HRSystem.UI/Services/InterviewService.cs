using HRSystem.UI.DTOs;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using HRSystem.UI.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace HRSystem.UI.Services
{
    public class InterviewService : IInterviewService
    {
        private readonly HttpClient httpClient;
        private readonly ITokenService tokenService;

        public InterviewService(HttpClient httpClient, ITokenService tokenService)
        {
            this.httpClient = httpClient;
            this.tokenService = tokenService;
        }

        public async Task<List<InterviewDto>> GetAllAsync(int page = 1, int size = 10)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            var response = await httpClient.GetAsync($"api/Interviews?p={page}&size={size}");
            var rawJson = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return new List<InterviewDto>();
            var result = await response.Content.ReadFromJsonAsync<PagedResult<InterviewDto>>();

            return result?.Items ?? new List<InterviewDto>();
        }

        public async Task<InterviewDto> GetByIdAsync(Guid id)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            var interviewDto = await httpClient.GetFromJsonAsync<InterviewDto>($"api/Interviews/{id}");
            return interviewDto;
        }

        public async Task<bool> AddAsync(AddInterviewRequestDto dto, IBrowserFile recordingFile)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(dto.Job ?? string.Empty), nameof(dto.Job));
            form.Add(new StringContent(dto.CandidateId.ToString()), nameof(dto.CandidateId));
            form.Add(new StringContent(dto.InterviewerEmail ?? string.Empty), nameof(dto.InterviewerEmail));
            form.Add(new StringContent(dto.HrEmail ?? string.Empty), nameof(dto.HrEmail));
            form.Add(new StringContent(dto.InterviewedAt.ToString("o")), nameof(dto.InterviewedAt));
            form.Add(new StringContent(dto.Status ?? "Scheduled"), nameof(dto.Status));
            if (dto.English.HasValue) form.Add(new StringContent(dto.English.Value.ToString()), nameof(dto.English));
            if (dto.Technical.HasValue) form.Add(new StringContent(dto.Technical.Value.ToString()), nameof(dto.Technical));
            if (dto.Recommend.HasValue) form.Add(new StringContent(dto.Recommend.Value.ToString()), nameof(dto.Recommend));

            // If recording file is present, add it
            if (recordingFile != null)
            {
                var stream = recordingFile.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(recordingFile.ContentType);
                form.Add(fileContent, "Recording", recordingFile.Name);
            }
            var response = await httpClient.PostAsync("api/Interviews", form);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateInterviewRequestDto dto, IBrowserFile recordingFile)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(dto.Job ?? string.Empty), "Job");
            content.Add(new StringContent(dto.CandidateId.ToString()), "CandidateId");
            content.Add(new StringContent(dto.InterviewerEmail ?? string.Empty), "InterviewerEmail");
            content.Add(new StringContent(dto.HrEmail ?? string.Empty), "HrEmail");
            content.Add(new StringContent(dto.InterviewedAt.ToString("o")), "InterviewedAt");
            content.Add(new StringContent(dto.Status ?? "Scheduled"), "Status");
            content.Add(new StringContent(dto.English.ToString()), "English");
            content.Add(new StringContent(dto.Technical.ToString()), "Technical");
            content.Add(new StringContent(dto.Recommend.ToString()), "Recommend");
            // If recording file is present, add it
            if (recordingFile != null)
            {
                var stream = recordingFile.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(recordingFile.ContentType);
                content.Add(fileContent, "Recording", recordingFile.Name);
            }
            var response = await httpClient.PutAsync($"api/Interviews/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            var response = await httpClient.DeleteAsync($"api/Interviews/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<InterviewDto>> SearchAsync(string query, int page = 1, int size = 10)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            var response = await httpClient.GetAsync($"api/Interviews/search?query={Uri.EscapeDataString(query)}&p={page}&size={size}");
            if (!response.IsSuccessStatusCode) return new List<InterviewDto>();
            var result = await response.Content.ReadFromJsonAsync<PagedResult<InterviewDto>>();
            return result?.Items ?? new List<InterviewDto>();
        }
    }
}

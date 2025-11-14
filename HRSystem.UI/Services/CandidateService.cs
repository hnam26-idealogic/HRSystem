using HRSystem.UI.DTOs;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace HRSystem.UI.Services
{
    public class CandidateService
    {
        private readonly HttpClient httpClient;
        private readonly ITokenService tokenService;

        public CandidateService(HttpClient httpClient, ITokenService tokenService)
        {
            this.httpClient = httpClient;
            this.tokenService = tokenService;
        }

        public async Task<List<CandidateDto>> GetAllAsync(int page = 1, int size = 10)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            var response = await httpClient.GetAsync($"/api/Candidate?p={page}&size={size}");
            if (!response.IsSuccessStatusCode) return new List<CandidateDto>();
            var result = await response.Content.ReadFromJsonAsync<PagedResult<CandidateDto>>();
            return result?.Items ?? new List<CandidateDto>();
        }

        public async Task<CandidateDto> GetByIdAsync(Guid id)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            return await httpClient.GetFromJsonAsync<CandidateDto>($"/api/Candidate/{id}");
        }

        public async Task<bool> AddAsync(AddCandidateRequestDto dto, IBrowserFile resumeFile)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(dto.Fullname), "Fullname");
            content.Add(new StringContent(dto.Email), "Email");
            content.Add(new StringContent(dto.Phone), "Phone");
            if (resumeFile != null)
            {
                var stream = resumeFile.OpenReadStream();
                content.Add(new StreamContent(stream), "Resume", resumeFile.Name);
            }

            var response = await httpClient.PostAsync("/api/Candidate", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateCandidateRequestDto dto, IBrowserFile resumeFile)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(dto.Fullname), "Fullname");
            content.Add(new StringContent(dto.Phone), "Phone");
            if (resumeFile != null)
            {
                var stream = resumeFile.OpenReadStream();
                content.Add(new StreamContent(stream), "Resume", resumeFile.Name);
            }

            var response = await httpClient.PutAsync($"/api/Candidate/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            var response = await httpClient.DeleteAsync($"/api/Candidate/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<CandidateDto>> SearchAsync(string query, int page = 1, int size = 10)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            var response = await httpClient.GetAsync($"/api/Candidate/search?query={Uri.EscapeDataString(query)}&p={page}&size={size}");
            if (!response.IsSuccessStatusCode) return new List<CandidateDto>();
            var result = await response.Content.ReadFromJsonAsync<PagedResult<CandidateDto>>();
            return result?.Items ?? new List<CandidateDto>();
        }

        public async Task<string> GetResumeUrlAsync(Guid candidateId)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            var response = await httpClient.GetAsync($"/api/Candidate/{candidateId}/resume-url");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}

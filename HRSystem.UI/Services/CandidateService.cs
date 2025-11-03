using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using HRSystem.UI.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace HRSystem.UI.Services
{
    public class CandidateService
    {
        private readonly HttpClient httpClient;

        public CandidateService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<CandidateDto>> GetAllAsync()
        {
            return await httpClient.GetFromJsonAsync<List<CandidateDto>>("/api/Candidate");
        }

        public async Task<CandidateDto> GetByIdAsync(Guid id)
        {
            return await httpClient.GetFromJsonAsync<CandidateDto>($"/api/Candidate/{id}");
        }

        public async Task<bool> AddAsync(AddCandidateRequestDto dto, IBrowserFile resumeFile)
        {
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
            var response = await httpClient.DeleteAsync($"/api/Candidate/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}

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
    public class InterviewService
    {
        private readonly HttpClient httpClient;
        private readonly ITokenService jwtService;

        public InterviewService(HttpClient httpClient, ITokenService jwtService)
        {
            this.httpClient = httpClient;
            this.jwtService = jwtService;
        }

        public async Task<List<InterviewDto>> GetAllAsync(int page = 1, int size = 10)
        {
            await jwtService.ApplyTokenAsync(httpClient);
            var response = await httpClient.GetAsync($"/api/Interview?p={page}&size={size}");
            var rawJson = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Raw API response:");
            Console.WriteLine(rawJson);
            if (!response.IsSuccessStatusCode) return new List<InterviewDto>();
            var result = await response.Content.ReadFromJsonAsync<PagedResult<InterviewDto>>();
            Console.WriteLine("Deserialized PagedResult:");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(result));
            if (result?.Items != null)
            {
                foreach (var interview in result.Items)
                {
                    Console.WriteLine($"InterviewId: {interview.Id}, CandidateId: {interview.CandidateId}, CandidateName: {interview.CandidateName}, InterviewerId: {interview.InterviewerId}, InterviewerName: {interview.InterviewerName}");
                }
            }
            return result?.Items ?? new List<InterviewDto>();
        }

        public async Task<InterviewDto> GetByIdAsync(Guid id)
        {
            await jwtService.ApplyTokenAsync(httpClient);
            return await httpClient.GetFromJsonAsync<InterviewDto>($"/api/Interview/{id}");
        }

        public async Task<bool> AddAsync(AddInterviewRequestDto dto, IBrowserFile recordingFile)
        {
            await jwtService.ApplyTokenAsync(httpClient);
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(dto.Job ?? string.Empty), nameof(dto.Job));
            form.Add(new StringContent(dto.CandidateId.ToString()), nameof(dto.CandidateId));
            form.Add(new StringContent(dto.InterviewerId.ToString()), nameof(dto.InterviewerId));
            form.Add(new StringContent(dto.HrId.ToString()), nameof(dto.HrId));
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
            var response = await httpClient.PostAsync("/api/Interview", form);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateInterviewRequestDto dto, IBrowserFile recordingFile)
        {
            await jwtService.ApplyTokenAsync(httpClient);
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(dto.Job ?? string.Empty), "Job");
            content.Add(new StringContent(dto.CandidateId.ToString()), "CandidateId");
            content.Add(new StringContent(dto.InterviewerId.ToString()), "InterviewerId");
            content.Add(new StringContent(dto.HrId.ToString()), "HrId");
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
            var response = await httpClient.PutAsync($"/api/Interview/{id}", content);
            return response.IsSuccessStatusCode;
        }
                          
        public async Task<bool> DeleteAsync(Guid id)
        {
            await jwtService.ApplyTokenAsync(httpClient);
            var response = await httpClient.DeleteAsync($"/api/Interview/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<InterviewDto>> SearchAsync(string query, int page = 1, int size = 10)
        {
            await jwtService.ApplyTokenAsync(httpClient);
            var response = await httpClient.GetAsync($"/api/Interview/search?query={Uri.EscapeDataString(query)}&p={page}&size={size}");
            if (!response.IsSuccessStatusCode) return new List<InterviewDto>();
            var result = await response.Content.ReadFromJsonAsync<PagedResult<InterviewDto>>();
            return result?.Items ?? new List<InterviewDto>();
        }
    }
}

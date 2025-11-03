using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using HRSystem.UI.DTOs;

namespace HRSystem.UI.Services
{
    public class InterviewService
    {
        private readonly HttpClient httpClient;
        private readonly JwtService jwtService;

        public InterviewService(HttpClient httpClient, JwtService jwtService)
        {
            this.httpClient = httpClient;
            this.jwtService = jwtService;
        }

        public async Task<List<InterviewDto>> GetAllAsync()
        {
            await jwtService.ApplyJwtAsync(httpClient);
            return await httpClient.GetFromJsonAsync<List<InterviewDto>>("/api/Interview");
        }

        public async Task<InterviewDto> GetByIdAsync(Guid id)
        {
            await jwtService.ApplyJwtAsync(httpClient);
            return await httpClient.GetFromJsonAsync<InterviewDto>($"/api/Interview/{id}");
        }

        public async Task<bool> AddAsync(AddInterviewRequestDto dto)
        {
            await jwtService.ApplyJwtAsync(httpClient);
            var response = await httpClient.PostAsJsonAsync("/api/Interview", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateInterviewRequestDto dto)
        {
            await jwtService.ApplyJwtAsync(httpClient);
            var response = await httpClient.PutAsJsonAsync($"/api/Interview/{id}", dto);
            return response.IsSuccessStatusCode;
        }
                          
        public async Task<bool> DeleteAsync(Guid id)
        {
            await jwtService.ApplyJwtAsync(httpClient);
            var response = await httpClient.DeleteAsync($"/api/Interview/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}

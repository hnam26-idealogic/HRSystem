using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using HRSystem.UI.DTOs;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HRSystem.UI.Services
{
    public class UserService
    {
        private readonly HttpClient httpClient;
        private readonly JwtService jwtService;

        public UserService(HttpClient httpClient, JwtService jwtService)
        {
            this.httpClient = httpClient;
            this.jwtService = jwtService;
        }

        private async Task ApplyJwtAsync()
        {
            var token = await jwtService.GetTokenAsync();
            httpClient.DefaultRequestHeaders.Authorization = token != null
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            await ApplyJwtAsync();
            return await httpClient.GetFromJsonAsync<List<UserDto>>("/api/User");
        }

        public async Task<UserDto> GetByIdAsync(Guid id)
        {
            await ApplyJwtAsync();
            return await httpClient.GetFromJsonAsync<UserDto>($"/api/User/{id}");
        }

        public async Task<bool> AddAsync(AddUserRequestDto dto)
        {
            await ApplyJwtAsync();
            var response = await httpClient.PostAsJsonAsync("/api/User", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateUserRequestDto dto)
        {
            await ApplyJwtAsync();
            var response = await httpClient.PutAsJsonAsync($"/api/User/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            await ApplyJwtAsync();
            var response = await httpClient.DeleteAsync($"/api/User/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}

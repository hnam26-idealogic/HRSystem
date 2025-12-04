using HRSystem.UI.DTOs;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HRSystem.UI.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient httpClient;
        private readonly ITokenService tokenService;

        public UserService(HttpClient httpClient, ITokenService tokenService)
        {
            this.httpClient = httpClient;
            this.tokenService = tokenService;
        }

        public async Task<List<UserDto>> GetAllAsync(int page = 1, int size = 10)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            var response = await httpClient.GetAsync($"/api/Users?p={page}&size={size}");
            if (!response.IsSuccessStatusCode) return new List<UserDto>();
            var result = await response.Content.ReadFromJsonAsync<PagedResult<UserDto>>();
            return result?.Items ?? new List<UserDto>();
        }

        public async Task<UserDto> GetByIdAsync(Guid id)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            return await httpClient.GetFromJsonAsync<UserDto>($"/api/Users/{id}");
        }

        public async Task<bool> AddAsync(AddUserRequestDto dto)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            var response = await httpClient.PostAsJsonAsync("/api/Users", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateUserRequestDto dto)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            var response = await httpClient.PutAsJsonAsync($"/api/Users/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            await tokenService.ApplyTokenAsync(httpClient);
            var response = await httpClient.DeleteAsync($"/api/Users/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}

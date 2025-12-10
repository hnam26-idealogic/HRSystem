using HRSystem.UI.DTOs;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace HRSystem.UI.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient httpClient;
        private readonly ITokenService tokenService;
        private readonly ILogger<UserService> _logger;

        public UserService(HttpClient httpClient, ITokenService tokenService, ILogger<UserService> logger)
        {
            this.httpClient = httpClient;
            this.tokenService = tokenService;
            _logger = logger;
        }

        public async Task<List<UserDto>> GetAllAsync(int page = 1, int size = 10)
        {
            try
            {
                _logger.LogInformation("Fetching users from API. Page: {Page}, Size: {Size}", page, size);
                await tokenService.ApplyTokenAsync(httpClient);
                var response = await httpClient.GetAsync($"/api/Users?p={page}&size={size}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch users. Status: {StatusCode}", response.StatusCode);
                    return new List<UserDto>();
                }
                var result = await response.Content.ReadFromJsonAsync<PagedResult<UserDto>>();
                _logger.LogInformation("Successfully retrieved {Count} users", result?.Items?.Count ?? 0);
                return result?.Items ?? new List<UserDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users. Page: {Page}, Size: {Size}", page, size);
                throw;
            }
        }

        public async Task<UserDto> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching user from API: {UserId}", id);
                await tokenService.ApplyTokenAsync(httpClient);
                var user = await httpClient.GetFromJsonAsync<UserDto>($"/api/Users/{id}");
                _logger.LogInformation("Successfully retrieved user: {UserId}", id);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user: {UserId}", id);
                throw;
            }
        }

        public async Task<bool> AddAsync(AddUserRequestDto dto)
        {
            try
            {
                _logger.LogInformation("Creating user. Email: {Email}", dto.Email);
                await tokenService.ApplyTokenAsync(httpClient);
                var response = await httpClient.PostAsJsonAsync("/api/Users", dto);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully created user: {Email}", dto.Email);
                }
                else
                {
                    _logger.LogWarning("Failed to create user. Email: {Email}, Status: {StatusCode}", dto.Email, response.StatusCode);
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Email}", dto.Email);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateUserRequestDto dto)
        {
            try
            {
                _logger.LogInformation("Updating user: {UserId}", id);
                await tokenService.ApplyTokenAsync(httpClient);
                var response = await httpClient.PutAsJsonAsync($"/api/Users/{id}", dto);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated user: {UserId}", id);
                }
                else
                {
                    _logger.LogWarning("Failed to update user: {UserId}, Status: {StatusCode}", id, response.StatusCode);
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting user: {UserId}", id);
                await tokenService.ApplyTokenAsync(httpClient);
                var response = await httpClient.DeleteAsync($"/api/Users/{id}");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully deleted user: {UserId}", id);
                }
                else
                {
                    _logger.LogWarning("Failed to delete user: {UserId}, Status: {StatusCode}", id, response.StatusCode);
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                throw;
            }
        }
    }
}

using HRSystem.UI.DTOs;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace HRSystem.UI.Services
{
    public class CandidateService : ICandidateService
    {
        private readonly HttpClient httpClient;
        private readonly ITokenService tokenService;
        private readonly ILogger<CandidateService> _logger;

        public CandidateService(HttpClient httpClient, ITokenService tokenService, ILogger<CandidateService> logger)
        {
            this.httpClient = httpClient;
            this.tokenService = tokenService;
            _logger = logger;
        }

        public async Task<List<CandidateDto>> GetAllAsync(int page = 1, int size = 10)
        {
            try
            {
                _logger.LogInformation("Fetching candidates from API. Page: {Page}, Size: {Size}", page, size);
                await tokenService.ApplyTokenAsync(httpClient);
                var response = await httpClient.GetAsync($"/api/Candidates?p={page}&size={size}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch candidates. Status: {StatusCode}", response.StatusCode);
                    return new List<CandidateDto>();
                }
                var result = await response.Content.ReadFromJsonAsync<PagedResult<CandidateDto>>();
                _logger.LogInformation("Successfully retrieved {Count} candidates", result?.Items?.Count ?? 0);
                return result?.Items ?? new List<CandidateDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching candidates. Page: {Page}, Size: {Size}", page, size);
                throw;
            }
        }

        public async Task<CandidateDto> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching candidate from API: {CandidateId}", id);
                await tokenService.ApplyTokenAsync(httpClient);
                var candidate = await httpClient.GetFromJsonAsync<CandidateDto>($"/api/Candidates/{id}");
                _logger.LogInformation("Successfully retrieved candidate: {CandidateId}", id);
                return candidate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching candidate: {CandidateId}", id);
                throw;
            }
        }

        public async Task<bool> AddAsync(AddCandidateRequestDto dto, IBrowserFile resumeFile)
        {
            try
            {
                _logger.LogInformation("Creating candidate. Email: {Email}, HasResume: {HasResume}", dto.Email, resumeFile != null);
                await tokenService.ApplyTokenAsync(httpClient);
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(dto.Fullname), "Fullname");
                content.Add(new StringContent(dto.Email), "Email");
                content.Add(new StringContent(dto.Phone), "Phone");
                if (resumeFile != null)
                {
                    _logger.LogDebug("Adding resume file: {FileName}, Size: {Size} bytes", resumeFile.Name, resumeFile.Size);
                    var stream = resumeFile.OpenReadStream();
                    content.Add(new StreamContent(stream), "Resume", resumeFile.Name);
                }

                var response = await httpClient.PostAsync("/api/Candidates", content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully created candidate: {Email}", dto.Email);
                }
                else
                {
                    _logger.LogWarning("Failed to create candidate. Email: {Email}, Status: {StatusCode}", dto.Email, response.StatusCode);
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating candidate: {Email}", dto.Email);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateCandidateRequestDto dto, IBrowserFile resumeFile)
        {
            try
            {
                _logger.LogInformation("Updating candidate: {CandidateId}, HasNewResume: {HasResume}", id, resumeFile != null);
                await tokenService.ApplyTokenAsync(httpClient);
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(dto.Fullname), "Fullname");
                content.Add(new StringContent(dto.Phone), "Phone");
                if (resumeFile != null)
                {
                    _logger.LogDebug("Adding new resume file: {FileName}", resumeFile.Name);
                    var stream = resumeFile.OpenReadStream();
                    content.Add(new StreamContent(stream), "Resume", resumeFile.Name);
                }

                var response = await httpClient.PutAsync($"/api/Candidates/{id}", content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated candidate: {CandidateId}", id);
                }
                else
                {
                    _logger.LogWarning("Failed to update candidate: {CandidateId}, Status: {StatusCode}", id, response.StatusCode);
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating candidate: {CandidateId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting candidate: {CandidateId}", id);
                await tokenService.ApplyTokenAsync(httpClient);
                var response = await httpClient.DeleteAsync($"/api/Candidates/{id}");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully deleted candidate: {CandidateId}", id);
                }
                else
                {
                    _logger.LogWarning("Failed to delete candidate: {CandidateId}, Status: {StatusCode}", id, response.StatusCode);
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting candidate: {CandidateId}", id);
                throw;
            }
        }

        public async Task<List<CandidateDto>> SearchAsync(string query, int page = 1, int size = 10)
        {
            try
            {
                _logger.LogInformation("Searching candidates. Query: '{Query}', Page: {Page}, Size: {Size}", query, page, size);
                await tokenService.ApplyTokenAsync(httpClient);
                var response = await httpClient.GetAsync($"/api/Candidates/search?query={Uri.EscapeDataString(query)}&p={page}&size={size}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to search candidates. Status: {StatusCode}", response.StatusCode);
                    return new List<CandidateDto>();
                }
                var result = await response.Content.ReadFromJsonAsync<PagedResult<CandidateDto>>();
                _logger.LogInformation("Search completed. Found {Count} candidates", result?.Items?.Count ?? 0);
                return result?.Items ?? new List<CandidateDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching candidates. Query: '{Query}'", query);
                throw;
            }
        }

        public async Task<string> GetResumeUrlAsync(Guid candidateId)
        {
            try
            {
                _logger.LogInformation("Fetching resume URL for candidate: {CandidateId}", candidateId);
                await tokenService.ApplyTokenAsync(httpClient);
                var response = await httpClient.GetAsync($"/api/Candidates/{candidateId}/resume-url");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get resume URL. CandidateId: {CandidateId}, Status: {StatusCode}", candidateId, response.StatusCode);
                    return null;
                }

                var url = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Successfully retrieved resume URL for candidate: {CandidateId}", candidateId);
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching resume URL for candidate: {CandidateId}", candidateId);
                throw;
            }
        }
    }
}

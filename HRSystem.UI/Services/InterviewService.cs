using HRSystem.UI.DTOs;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using HRSystem.UI.DTOs;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

namespace HRSystem.UI.Services
{
    public class InterviewService : IInterviewService
    {
        private readonly HttpClient httpClient;
        private readonly ITokenService tokenService;
        private readonly ILogger<InterviewService> _logger;

        public InterviewService(HttpClient httpClient, ITokenService tokenService, ILogger<InterviewService> logger)
        {
            this.httpClient = httpClient;
            this.tokenService = tokenService;
            _logger = logger;
        }

        public async Task<List<InterviewDto>> GetAllAsync(int page = 1, int size = 10)
        {
            try
            {
                _logger.LogInformation("Fetching interviews from API. Page: {Page}, Size: {Size}", page, size);
                await tokenService.ApplyTokenAsync(httpClient);
                var response = await httpClient.GetAsync($"api/Interviews?p={page}&size={size}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch interviews. Status: {StatusCode}", response.StatusCode);
                    return new List<InterviewDto>();
                }
                var result = await response.Content.ReadFromJsonAsync<PagedResult<InterviewDto>>();
                _logger.LogInformation("Successfully retrieved {Count} interviews", result?.Items?.Count ?? 0);
                return result?.Items ?? new List<InterviewDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching interviews. Page: {Page}, Size: {Size}", page, size);
                throw;
            }
        }

        public async Task<InterviewDto> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching interview from API: {InterviewId}", id);
                await tokenService.ApplyTokenAsync(httpClient);
                var interviewDto = await httpClient.GetFromJsonAsync<InterviewDto>($"api/Interviews/{id}");
                _logger.LogInformation("Successfully retrieved interview: {InterviewId}", id);
                return interviewDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching interview: {InterviewId}", id);
                throw;
            }
        }

        public async Task<bool> AddAsync(AddInterviewRequestDto dto, IBrowserFile recordingFile)
        {
            try
            {
                _logger.LogInformation("Creating interview. Candidate: {CandidateId}, Job: {Job}, HasRecording: {HasRecording}",
                    dto.CandidateId, dto.Job, recordingFile != null);
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
                    _logger.LogDebug("Adding recording file: {FileName}, Size: {Size} bytes", recordingFile.Name, recordingFile.Size);
                    var stream = recordingFile.OpenReadStream();
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(recordingFile.ContentType);
                    form.Add(fileContent, "Recording", recordingFile.Name);
                }
                var response = await httpClient.PostAsync("api/Interviews", form);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully created interview for candidate: {CandidateId}", dto.CandidateId);
                }
                else
                {
                    _logger.LogWarning("Failed to create interview. CandidateId: {CandidateId}, Status: {StatusCode}",
                        dto.CandidateId, response.StatusCode);
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating interview for candidate: {CandidateId}", dto.CandidateId);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateInterviewRequestDto dto, IBrowserFile recordingFile)
        {
            try
            {
                _logger.LogInformation("Updating interview: {InterviewId}, HasNewRecording: {HasRecording}", id, recordingFile != null);
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
                    _logger.LogDebug("Adding new recording file: {FileName}", recordingFile.Name);
                    var stream = recordingFile.OpenReadStream();
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(recordingFile.ContentType);
                    content.Add(fileContent, "Recording", recordingFile.Name);
                }
                var response = await httpClient.PutAsync($"api/Interviews/{id}", content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated interview: {InterviewId}", id);
                }
                else
                {
                    _logger.LogWarning("Failed to update interview: {InterviewId}, Status: {StatusCode}", id, response.StatusCode);
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating interview: {InterviewId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting interview: {InterviewId}", id);
                await tokenService.ApplyTokenAsync(httpClient);
                var response = await httpClient.DeleteAsync($"api/Interviews/{id}");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully deleted interview: {InterviewId}", id);
                }
                else
                {
                    _logger.LogWarning("Failed to delete interview: {InterviewId}, Status: {StatusCode}", id, response.StatusCode);
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting interview: {InterviewId}", id);
                throw;
            }
        }

        public async Task<List<InterviewDto>> SearchAsync(string query, int page = 1, int size = 10)
        {
            try
            {
                _logger.LogInformation("Searching interviews. Query: '{Query}', Page: {Page}, Size: {Size}", query, page, size);
                await tokenService.ApplyTokenAsync(httpClient);
                var response = await httpClient.GetAsync($"api/Interviews/search?query={Uri.EscapeDataString(query)}&p={page}&size={size}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to search interviews. Status: {StatusCode}", response.StatusCode);
                    return new List<InterviewDto>();
                }
                var result = await response.Content.ReadFromJsonAsync<PagedResult<InterviewDto>>();
                _logger.LogInformation("Search completed. Found {Count} interviews", result?.Items?.Count ?? 0);
                return result?.Items ?? new List<InterviewDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching interviews. Query: '{Query}'", query);
                throw;
            }
        }
    }
}

using AutoMapper;
using HRSystem.API.Models.Domain;
using HRSystem.API.Models.DTO;
using HRSystem.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HRSystem.API.CustomActionFilters;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace HRSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewsController : ControllerBase
    {
        private readonly ILogger<InterviewsController> _logger;
        private readonly IInterviewRepository interviewRepository;
        private readonly ICandidateRepository candidateRepository;
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IRecordingStorageService recordingStorageService;
        private readonly List<Candidate> candidates;
        private readonly List<User> users;

        public InterviewsController(
            ILogger<InterviewsController> logger,
            IInterviewRepository interviewRepository,
            ICandidateRepository candidateRepository,
            IUserRepository userRepository,
            IMapper mapper,
            IRecordingStorageService recordingStorageService)
        {
            _logger = logger;
            this.interviewRepository = interviewRepository;
            this.candidateRepository = candidateRepository;
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.recordingStorageService = recordingStorageService;

            _logger.LogInformation("Initializing InterviewsController and loading reference data");
            // Fetch all candidates and users once for name mapping
            candidates = candidateRepository.GetAllAsync().Result.Items.ToList();
            users = userRepository.GetAllAsync().Result.Items.ToList();
            _logger.LogDebug("Loaded {CandidateCount} candidates and {UserCount} users for reference", candidates.Count, users.Count);
        }

        [HttpGet]
        [Authorize (Roles = "HR, Interviewer")]
        public async Task<IActionResult> GetAll([FromQuery] int p = 1, [FromQuery] int size = 10)
        {
            try
            {
                _logger.LogInformation("Retrieving all interviews. Page: {Page}, Size: {Size}", p, size);
                var (pagedInterviews, totalCount) = await interviewRepository.GetAllAsync(p, size);
                var interviewDtos = mapper.Map<List<InterviewDto>>(pagedInterviews);
                foreach (var dto in interviewDtos)
                {
                    dto.CandidateName = candidates.FirstOrDefault(c => c.Id == dto.CandidateId)?.Fullname ?? "";
                    dto.InterviewerName = users.FirstOrDefault(u => u.Email == dto.InterviewerEmail)?.Fullname ?? "";
                    dto.HrName = users.FirstOrDefault(u => u.Email == dto.HrEmail)?.Fullname ?? "";
                }
                _logger.LogInformation("Successfully retrieved {Count} interviews out of {TotalCount}", interviewDtos.Count, totalCount);
                return Ok(new
                {
                    TotalCount = totalCount,
                    PageNumber = p,
                    PageSize = size,
                    Items = interviewDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving interviews. Page: {Page}, Size: {Size}", p, size);
                return StatusCode(500, "An error occurred while retrieving interviews");
            }
        }

        [HttpGet("{id:Guid}")]
        [Authorize (Roles = "HR, Interviewer")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                _logger.LogInformation("Retrieving interview: {InterviewId}", id);
                var interview = await interviewRepository.GetByIdAsync(id);
                if (interview == null)
                {
                    _logger.LogWarning("Interview not found: {InterviewId}", id);
                    return NotFound();
                }
                var dto = mapper.Map<InterviewDto>(interview);
                dto.CandidateName = candidates.FirstOrDefault(c => c.Id == interview.CandidateId)?.Fullname ?? "";
                dto.InterviewerName = users.FirstOrDefault(u => u.Email == interview.InterviewerEmail)?.Fullname ?? "";
                dto.HrName = users.FirstOrDefault(u => u.Email == interview.HrEmail)?.Fullname ?? "";
                _logger.LogInformation("Successfully retrieved interview: {InterviewId}, Candidate: {CandidateId}", id, interview.CandidateId);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving interview: {InterviewId}", id);
                return StatusCode(500, "An error occurred while retrieving the interview");
            }
        }

        [HttpPost]
        [ValidateModel]
        [Authorize (Roles = "HR")]
        public async Task<IActionResult> Add([FromForm] AddInterviewRequestDto addInterviewRequestDto)
        {
            try
            {
                _logger.LogInformation("Creating new interview. Candidate: {CandidateId}, Job: {Job}, HasRecording: {HasRecording}",
                    addInterviewRequestDto.CandidateId, addInterviewRequestDto.Job, addInterviewRequestDto.Recording != null);
                
                var interviewEntity = mapper.Map<Interview>(addInterviewRequestDto);

                if (addInterviewRequestDto.Recording != null)
                {
                    _logger.LogInformation("Uploading recording file: {FileName}, Size: {Size} bytes",
                        addInterviewRequestDto.Recording.FileName, addInterviewRequestDto.Recording.Length);
                    var (recordingPath, _) = await recordingStorageService.UploadAsync(addInterviewRequestDto.Recording);
                    interviewEntity.Recording = recordingPath;
                    _logger.LogInformation("Recording uploaded successfully: {RecordingPath}", recordingPath);
                }

                var createdInterview = await interviewRepository.AddAsync(interviewEntity);
                var createdInterviewDto = mapper.Map<InterviewDto>(createdInterview);
                _logger.LogInformation("Interview created successfully: {InterviewId}, Candidate: {CandidateId}",
                    createdInterviewDto.Id, createdInterviewDto.CandidateId);
                return CreatedAtAction(nameof(GetById), new { id = createdInterviewDto.Id }, createdInterviewDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating interview. Candidate: {CandidateId}, Job: {Job}",
                    addInterviewRequestDto.CandidateId, addInterviewRequestDto.Job);
                return StatusCode(500, "An error occurred while creating the interview");
            }
        }

        [HttpPut("{id:Guid}")]
        [Authorize (Roles = "HR, Interviewer")]
        [ValidateModel]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateInterviewRequestDto updateInterviewRequestDto)
        {
            try
            {
                _logger.LogInformation("Updating interview: {InterviewId}, HasRecording: {HasRecording}",
                    id, updateInterviewRequestDto.Recording != null);
                
                var interviewEntity = mapper.Map<Interview>(updateInterviewRequestDto);

                if (updateInterviewRequestDto.Recording != null)
                {
                    _logger.LogInformation("Uploading new recording for interview: {InterviewId}, FileName: {FileName}",
                        id, updateInterviewRequestDto.Recording.FileName);
                    var (recordingPath, _) = await recordingStorageService.UploadAsync(updateInterviewRequestDto.Recording);
                    interviewEntity.Recording = recordingPath;
                }

                var updatedInterview = await interviewRepository.UpdateAsync(id, interviewEntity);
                var updatedInterviewDto = mapper.Map<InterviewDto>(updatedInterview);
                _logger.LogInformation("Interview updated successfully: {InterviewId}", id);
                return Ok(updatedInterviewDto);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Interview not found for update: {InterviewId}", id);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating interview: {InterviewId}", id);
                return StatusCode(500, "An error occurred while updating the interview");
            }
        }

        [HttpDelete("{id:Guid}")]
        [Authorize (Roles = "HR")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting interview: {InterviewId}", id);
                var deleted = await interviewRepository.DeleteAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("Interview not found for deletion: {InterviewId}", id);
                    return NotFound();
                }
                _logger.LogInformation("Interview deleted successfully: {InterviewId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting interview: {InterviewId}", id);
                return StatusCode(500, "An error occurred while deleting the interview");
            }
        }

        [HttpGet("search")]
        [Authorize (Roles = "HR, Interviewer")]
        public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int p = 1, [FromQuery] int size = 10)
        {
            try
            {
                _logger.LogInformation("Searching interviews. Query: '{Query}', Page: {Page}, Size: {Size}", query, p, size);
                var (interviews, totalCount) = await interviewRepository.SearchAsync(query, p, size);
                var interviewDtos = mapper.Map<List<InterviewDto>>(interviews);
                
                // Map names from Graph API users
                foreach (var dto in interviewDtos)
                {
                    dto.CandidateName = candidates.FirstOrDefault(c => c.Id == dto.CandidateId)?.Fullname ?? "";
                    dto.InterviewerName = users.FirstOrDefault(u => u.Email == dto.InterviewerEmail)?.Fullname ?? "";
                    dto.HrName = users.FirstOrDefault(u => u.Email == dto.HrEmail)?.Fullname ?? "";
                }
                
                // Additionally filter by user names if they match the query (client-side filtering)
                if (!string.IsNullOrWhiteSpace(query))
                {
                    var normalizedQuery = query.Trim().ToLower();
                    interviewDtos = interviewDtos.Where(dto =>
                        dto.CandidateName.ToLower().Contains(normalizedQuery) ||
                        dto.InterviewerName.ToLower().Contains(normalizedQuery) ||
                        dto.HrName.ToLower().Contains(normalizedQuery) ||
                        dto.Job.ToLower().Contains(normalizedQuery) ||
                        dto.Status.ToLower().Contains(normalizedQuery) ||
                        dto.InterviewerEmail.ToLower().Contains(normalizedQuery) ||
                        dto.HrEmail.ToLower().Contains(normalizedQuery)
                    ).ToList();
                    
                    totalCount = interviewDtos.Count;
                }
                
                _logger.LogInformation("Search completed. Query: '{Query}', Found: {Count} interviews", query, totalCount);
                return Ok(new
                {
                    TotalCount = totalCount,
                    PageNumber = p,
                    PageSize = size,
                    Items = interviewDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching interviews. Query: '{Query}'", query);
                return StatusCode(500, "An error occurred while searching interviews");
            }
        }
    }
}

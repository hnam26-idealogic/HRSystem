using AutoMapper;
using HRSystem.API.CustomActionFilters;
using HRSystem.API.Models.Domain;
using HRSystem.API.Models.DTO;
using HRSystem.API.Repositories;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Microsoft.Extensions.Logging;

namespace HRSystem.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CandidatesController : ControllerBase
    {
        private readonly ILogger<CandidatesController> _logger;
        static readonly string[] scopeRequiredByApi = new string[] { "access_as_user" };

        private readonly ICandidateRepository _candidateRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMapper _mapper;

        public CandidatesController(ICandidateRepository candidateRepository, IFileStorageService fileStorageService, IMapper mapper, ILogger<CandidatesController> logger)
        {
            _candidateRepository = candidateRepository;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "HR,Interviewer")]
        public async Task<IActionResult> GetAll([FromQuery] int p = 1, [FromQuery] int size = 10)
        {
            _logger.LogInformation("Getting all candidates, page: {Page}, size: {Size}", p, size);
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

            var (pagedCandidates, totalCount) = await _candidateRepository.GetAllAsync(p, size);
            var candidateDtos = _mapper.Map<List<CandidateDto>>(pagedCandidates);
            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = p,
                PageSize = size,
                Items = candidateDtos
            });
        }

        [HttpGet("{id:Guid}")]
        [Authorize(Roles = "HR, Interviewer")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            _logger.LogInformation("Getting candidate by ID: {Id}", id);
            var candidate = await _candidateRepository.GetByIdAsync(id);
            if (candidate == null) 
            {
                _logger.LogWarning("Candidate not found with ID: {Id}", id);
                return NotFound();
            }
            return Ok(_mapper.Map<CandidateDto>(candidate));
        }

        [HttpGet("{id:Guid}/resume-url")]
        [Authorize(Roles = "HR, Interviewer")]
        public async Task<IActionResult> GetResumeUrl([FromRoute] Guid id)
        {
            _logger.LogInformation("Getting resume URL for candidate ID: {Id}", id);
            var candidate = await _candidateRepository.GetByIdAsync(id);
            if (candidate == null || string.IsNullOrEmpty(candidate.ResumePath))
            {
                _logger.LogWarning("Candidate or resume not found for ID: {Id}", id);
                return NotFound();
            }

            var sasUrl = _fileStorageService.GetBlobSasUrl(candidate.ResumePath, 10); // 10 min expiry
            _logger.LogInformation("Generated resume SAS URL for candidate ID: {Id}", id);
            return Ok(sasUrl);
        }

        [HttpPost]
        [ValidateModel]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Add([FromForm] AddCandidateRequestDto addCandidateRequestDto)
        {
            _logger.LogInformation("Adding a new candidate with email: {Email}", addCandidateRequestDto.Email);
            // Check if email exists
            var existingCandidate = await _candidateRepository.GetByEmailAsync(addCandidateRequestDto.Email);
            if (existingCandidate != null)
            {
                _logger.LogWarning("Attempt to add a candidate with an existing email: {Email}", addCandidateRequestDto.Email);
                ModelState.AddModelError("Email", "A candidate with this email already exists.");
                return ValidationProblem(ModelState);
            }

            // Resume validation is handled by [Required] attribute

            var candidateEntity = _mapper.Map<Candidate>(addCandidateRequestDto);

            var (resumeBlobName, _) = await _fileStorageService.UploadAsync(addCandidateRequestDto.Resume!);
            candidateEntity.ResumePath = resumeBlobName;

            var createdCandidate = await _candidateRepository.AddAsync(candidateEntity);
            var createdCandidateDto = _mapper.Map<CandidateDto>(createdCandidate);
            _logger.LogInformation("Added new candidate with ID: {Id}", createdCandidateDto.Id);
            return CreatedAtAction(nameof(GetById), new { id = createdCandidateDto.Id }, createdCandidateDto);
        }

        [HttpPut("{id:Guid}")]
        [ValidateModel]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromForm] UpdateCandidateRequestDto updateCandidateRequestDto)
        {
            _logger.LogInformation("Updating candidate ID: {Id}", id);
            // Optionally require resume on update
            if (updateCandidateRequestDto.Resume == null)
            {
                _logger.LogWarning("Attempt to update candidate ID: {Id} without a resume file.", id);
                ModelState.AddModelError("Resume", "Resume file is required for update.");
                return ValidationProblem(ModelState);
            }

            var candidateEntity = _mapper.Map<Candidate>(updateCandidateRequestDto);

            var (resumePath, _) = await _fileStorageService.UploadAsync(updateCandidateRequestDto.Resume!);
            candidateEntity.ResumePath = resumePath;

            var updatedCandidate = await _candidateRepository.UpdateAsync(id, candidateEntity);
            var updatedCandidateDto = _mapper.Map<CandidateDto>(updatedCandidate);
            _logger.LogInformation("Updated candidate ID: {Id}", id);
            return Ok(updatedCandidateDto);
        }

        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            _logger.LogInformation("Deleting candidate ID: {Id}", id);
            var deleted = await _candidateRepository.DeleteAsync(id);
            if (!deleted) 
            {
                _logger.LogWarning("Candidate not found for deletion with ID: {Id}", id);
                return NotFound();
            }
            _logger.LogInformation("Deleted candidate ID: {Id}", id);
            return NoContent();
        }

        [HttpGet("search")]
        [Authorize(Roles = "HR, Interviewer")]
        public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int p = 1, [FromQuery] int size = 10)
        {
            _logger.LogInformation("Searching candidates with query: {Query}, page: {Page}, size: {Size}", query, p, size);
            var (candidates, totalCount) = await _candidateRepository.SearchAsync(query, p, size);
            var candidateDtos = _mapper.Map<List<CandidateDto>>(candidates);
            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = p,
                PageSize = size,
                Items = candidateDtos
            });
        }
    }
}

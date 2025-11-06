using AutoMapper;
using HRSystem.API.CustomActionFilters;
using HRSystem.API.Models.Domain;
using HRSystem.API.Models.DTO;
using HRSystem.API.Repositories;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private readonly ICandidateRepository candidateRepository;
        private readonly IFileStorageService fileStorageService;
        private readonly IMapper mapper;

        public CandidateController(ICandidateRepository candidateRepository, IFileStorageService fileStorageService, IMapper mapper)
        {
            this.candidateRepository = candidateRepository;
            this.fileStorageService = fileStorageService;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int p = 1, [FromQuery] int size = 10)
        {
            var (pagedCandidates, totalCount) = await candidateRepository.GetAllAsync(p, size);
            var candidateDtos = mapper.Map<List<CandidateDto>>(pagedCandidates);
            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = p,
                PageSize = size,
                Items = candidateDtos
            });
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var candidate = await candidateRepository.GetByIdAsync(id);
            if (candidate == null) return NotFound();
            return Ok(mapper.Map<CandidateDto>(candidate));
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Add([FromForm] AddCandidateRequestDto addCandidateRequestDto)
        {
            // Check if email exists
            var existingCandidate = await candidateRepository.GetByEmailAsync(addCandidateRequestDto.Email);
            if (existingCandidate != null)
            {
                ModelState.AddModelError("Email", "A candidate with this email already exists.");
                return ValidationProblem(ModelState);
            }

            // Resume validation is handled by [Required] attribute

            var candidateEntity = mapper.Map<Candidate>(addCandidateRequestDto);

            var (resumePath, file) = await fileStorageService.UploadAsync(addCandidateRequestDto.Resume!);
            candidateEntity.ResumePath = resumePath;

            var createdCandidate = await candidateRepository.AddAsync(candidateEntity);
            var createdCandidateDto = mapper.Map<CandidateDto>(createdCandidate);
            return CreatedAtAction(nameof(GetById), new { id = createdCandidateDto.Id }, createdCandidateDto);
        }

        [HttpPut("{id:Guid}")]
        [ValidateModel]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromForm] UpdateCandidateRequestDto updateCandidateRequestDto)
        {
            // Optionally require resume on update
            if (updateCandidateRequestDto.Resume == null)
            {
                ModelState.AddModelError("Resume", "Resume file is required for update.");
                return ValidationProblem(ModelState);
            }

            var candidateEntity = mapper.Map<Candidate>(updateCandidateRequestDto);

            var (resumePath, _) = await fileStorageService.UploadAsync(updateCandidateRequestDto.Resume!);
            candidateEntity.ResumePath = resumePath;

            var updatedCandidate = await candidateRepository.UpdateAsync(id, candidateEntity);
            var updatedCandidateDto = mapper.Map<CandidateDto>(updatedCandidate);
            return Ok(updatedCandidateDto);
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var deleted = await candidateRepository.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}

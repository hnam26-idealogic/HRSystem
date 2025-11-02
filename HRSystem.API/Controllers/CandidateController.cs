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
        public async Task<IActionResult> GetAll()
        {
            var candidates = await candidateRepository.GetAllAsync();
            return Ok(mapper.Map<List<CandidateDto>>(candidates));
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
            var candidateEntity = mapper.Map<Candidate>(addCandidateRequestDto);

            if (addCandidateRequestDto.Resume != null)
            {
                var (resumePath, _) = await fileStorageService.UploadAsync(addCandidateRequestDto.Resume);
                candidateEntity.ResumePath = resumePath;
            }

            var createdCandidate = await candidateRepository.AddAsync(candidateEntity);
            var createdCandidateDto = mapper.Map<CandidateDto>(createdCandidate);
            return CreatedAtAction(nameof(GetById), new { id = createdCandidateDto.Id }, createdCandidateDto);
        }

        [HttpPut("{id:Guid}")]
        [ValidateModel]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromForm] UpdateCandidateRequestDto updateCandidateRequestDto)
        {
            var candidateEntity = mapper.Map<Candidate>(updateCandidateRequestDto);

            if (updateCandidateRequestDto.Resume != null)
            {
                var (resumePath, _) = await fileStorageService.UploadAsync(updateCandidateRequestDto.Resume);
                candidateEntity.ResumePath = resumePath;
            }

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

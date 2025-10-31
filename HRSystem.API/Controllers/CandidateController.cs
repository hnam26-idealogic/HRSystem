using AutoMapper;
using HRSystem.API.Models.Domain;
using HRSystem.API.Models.DTO;
using HRSystem.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private readonly ICandidateRepository candidateRepository;
        private readonly IMapper mapper;

        public CandidateController(ICandidateRepository candidateRepository, IMapper mapper)
        {
            this.candidateRepository = candidateRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var candidates = await candidateRepository.GetAllAsync();
            return Ok(mapper.Map<List<CandidateDto>>(candidates));
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var candidate = await candidateRepository.GetByIdAsync(id);
            if (candidate == null) return NotFound();
            return Ok(mapper.Map<CandidateDto>(candidate));
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddCandidateRequestDto addCandidateRequestDto)
        {
            var candidateEntity = mapper.Map<Candidate>(addCandidateRequestDto);
            var createdCandidate = await candidateRepository.AddAsync(candidateEntity);
            var createdCandidateDto = mapper.Map<CandidateDto>(createdCandidate);
            return CreatedAtAction(nameof(GetById), new { id = createdCandidateDto.Id }, createdCandidateDto);
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCandidateRequestDto updateCandidateRequestDto)
        {
            var candidateEntity = mapper.Map<Candidate>(updateCandidateRequestDto);
            var updatedCandidate = await candidateRepository.UpdateAsync(id, candidateEntity);
            var updatedCandidateDto = mapper.Map<CandidateDto>(updatedCandidate);
            return Ok(updatedCandidateDto);
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await candidateRepository.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}

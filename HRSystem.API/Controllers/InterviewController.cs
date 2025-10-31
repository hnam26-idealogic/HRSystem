using AutoMapper;
using HRSystem.API.Models.Domain;
using HRSystem.API.Models.DTO;
using HRSystem.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HRSystem.API.CustomActionFilters;

namespace HRSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewController : ControllerBase
    {
        private readonly IInterviewRepository interviewRepository;
        private readonly IMapper mapper;

        public InterviewController(IInterviewRepository interviewRepository, IMapper mapper)
        {
            this.interviewRepository = interviewRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var interviews = await interviewRepository.GetAllAsync();
            return Ok(mapper.Map<List<InterviewDto>>(interviews));
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var interview = await interviewRepository.GetByIdAsync(id);
            if (interview == null) return NotFound();
            return Ok(mapper.Map<InterviewDto>(interview));
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Add([FromBody] AddInterviewRequestDto addInterviewRequestDto)
        {
            var interviewEntity = mapper.Map<Interview>(addInterviewRequestDto);
            var createdInterview = await interviewRepository.AddAsync(interviewEntity);
            var createdInterviewDto = mapper.Map<InterviewDto>(createdInterview);
            return CreatedAtAction(nameof(GetById), new { id = createdInterviewDto.Id }, createdInterviewDto);
        }

        [HttpPut("{id:Guid}")]
        [ValidateModel]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInterviewRequestDto updateInterviewRequestDto)
        {
            var interviewEntity = mapper.Map<Interview>(updateInterviewRequestDto);
            var updatedInterview = await interviewRepository.UpdateAsync(id, interviewEntity);
            var updatedInterviewDto = mapper.Map<InterviewDto>(updatedInterview);
            return Ok(updatedInterviewDto);
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await interviewRepository.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}

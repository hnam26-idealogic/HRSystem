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
    private readonly ICandidateRepository candidateRepository;
    private readonly IUserRepository userRepository;
    private readonly IMapper mapper;

    private List<Models.Domain.Candidate> candidates;
    private List<Models.Domain.User> users;

        public InterviewController(
            IInterviewRepository interviewRepository,
            ICandidateRepository candidateRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            this.interviewRepository = interviewRepository;
            this.candidateRepository = candidateRepository;
            this.userRepository = userRepository;
            this.mapper = mapper;

            // Fetch all candidates and users once for name mapping
            candidates = candidateRepository.GetAllAsync().Result.ToList();
            users = userRepository.GetAllAsync().Result.ToList();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var interviews = await interviewRepository.GetAllAsync();
            var interviewDtos = interviews.Select(i =>
            {
                var dto = mapper.Map<InterviewDto>(i);
                dto.CandidateName = candidates.FirstOrDefault(c => c.Id == i.CandidateId)?.Fullname ?? "";
                dto.InterviewerName = users.FirstOrDefault(u => u.Id == i.InterviewerId)?.Fullname ?? "";
                dto.HRName = users.FirstOrDefault(u => u.Id == i.HrId)?.Fullname ?? "";
                return dto;
            }).ToList();
            return Ok(interviewDtos);
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var interview = await interviewRepository.GetByIdAsync(id);
            if (interview == null) return NotFound();
            var dto = mapper.Map<InterviewDto>(interview);
            dto.CandidateName = candidates.FirstOrDefault(c => c.Id == interview.CandidateId)?.Fullname ?? "";
            dto.InterviewerName = users.FirstOrDefault(u => u.Id == interview.InterviewerId)?.Fullname ?? "";
            dto.HRName = users.FirstOrDefault(u => u.Id == interview.HrId)?.Fullname ?? "";
            return Ok(dto);
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

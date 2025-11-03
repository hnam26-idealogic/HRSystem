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
            candidates = candidateRepository.GetAllAsync().Result.Items.ToList();
            users = userRepository.GetAllAsync().Result.Items.ToList();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int p = 1, [FromQuery] int size = 10)
        {
            var (pagedInterviews, totalCount) = await interviewRepository.GetAllAsync(p, size);
            var interviewDtos = mapper.Map<List<InterviewDto>>(pagedInterviews);
            foreach (var dto in interviewDtos)
            {
                dto.CandidateName = candidates.FirstOrDefault(c => c.Id == dto.CandidateId)?.Fullname ?? "";
                dto.InterviewerName = users.FirstOrDefault(u => u.Id == dto.InterviewerId)?.Fullname ?? "";
                dto.HRName = users.FirstOrDefault(u => u.Id == dto.HrId)?.Fullname ?? "";
            }
            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = p,
                PageSize = size,
                Items = interviewDtos
            });
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

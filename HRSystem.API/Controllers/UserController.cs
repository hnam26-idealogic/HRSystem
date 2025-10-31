using AutoMapper;
using HRSystem.API.Models.Domain;
using HRSystem.API.Models.DTO;
using HRSystem.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        // private readonly ILogger<UserController> logger;

        public UserController(IUserRepository userRepository, IMapper mapper)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            // this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // logger.LogInformation("Fetching all users");

            var usersDomainModel = await userRepository.GetAllAsync();

            return Ok(mapper.Map<List<UserDto>>(usersDomainModel));

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await userRepository.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(mapper.Map<UserDto>(user));
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddUserDto addUserDto)
        {
            var user = mapper.Map<User>(addUserDto);
            var created = await userRepository.AddAsync(user, addUserDto.Password);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, mapper.Map<UserDto>(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserDto userDto)
        {
            var user = mapper.Map<User>(userDto);
            var updated = await userRepository.UpdateAsync(id, user);
            return Ok(mapper.Map<UserDto>(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await userRepository.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}

using AutoMapper;
using HRSystem.API.CustomActionFilters;
using HRSystem.API.Models.Domain;
using HRSystem.API.Models.DTO;
using HRSystem.API.Repositories;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize (Roles = "HR")]
        public async Task<IActionResult> GetAll([FromQuery] int p = 1, [FromQuery] int size = 10)
        {
            var (pagedUsers, totalCount) = await userRepository.GetAllAsync(p, size);
            var userDtos = mapper.Map<List<UserDto>>(pagedUsers);
            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = p,
                PageSize = size,
                Items = userDtos
            });
        }

        [HttpGet("{id:Guid}")]
        [Authorize (Roles = "HR")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var userEntity = await userRepository.GetByIdAsync(id);
            if (userEntity == null) return NotFound();
            var userDto = mapper.Map<UserDto>(userEntity);
            return Ok(userDto);
        }

        // [HttpPost]
        // [ValidateModel]
        // public async Task<IActionResult> Add([FromBody] AddUserRequestDto addUserRequestDto)
        // {
        //     var userEntity = mapper.Map<User>(addUserRequestDto);
        //     var createdUser = await userRepository.AddAsync(userEntity, addUserRequestDto.Password);
        //     var createdUserDto = mapper.Map<UserDto>(createdUser);
        //     return CreatedAtAction(nameof(GetById), new { id = createdUserDto.Id }, createdUserDto);
        // }

        [HttpPut("{id:Guid}")]
        [ValidateModel]
        [Authorize (Roles = "HR")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateUserRequestDto updateUserRequestDto)
        {
            var userEntity = mapper.Map<User>(updateUserRequestDto);
            var updatedUser = await userRepository.UpdateAsync(id, userEntity);
            var updatedUserDto = mapper.Map<UpdateUserRequestDto>(updatedUser);
            return Ok(updatedUserDto);
        }

        [HttpDelete("{id:Guid}")]
        [Authorize (Roles = "HR")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var deletedUser = await userRepository.DeleteAsync(id);
            if (!deletedUser) return NotFound();
            return NoContent();
        }
    }
}

using AutoMapper;
using HRSystem.API.CustomActionFilters;
using HRSystem.API.Models.Domain;
using HRSystem.API.Models.DTO;
using HRSystem.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        public UsersController(ILogger<UsersController> logger, IUserRepository userRepository, IMapper mapper)
        {
            _logger = logger;
            this.userRepository = userRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        [Authorize (Roles = "HR")]
        public async Task<IActionResult> GetAll([FromQuery] int p = 1, [FromQuery] int size = 10)
        {
            try
            {
                _logger.LogInformation("Retrieving all users. Page: {Page}, Size: {Size}", p, size);
                var (pagedUsers, totalCount) = await userRepository.GetAllAsync(p, size);
                var userDtos = mapper.Map<List<UserDto>>(pagedUsers);
                _logger.LogInformation("Successfully retrieved {Count} users out of {TotalCount}", userDtos.Count, totalCount);
                return Ok(new
                {
                    TotalCount = totalCount,
                    PageNumber = p,
                    PageSize = size,
                    Items = userDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users. Page: {Page}, Size: {Size}", p, size);
                return StatusCode(500, "An error occurred while retrieving users");
            }
        }

        [HttpGet("{id:Guid}")]
        [Authorize (Roles = "HR")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            try
            {
                _logger.LogInformation("Retrieving user: {UserId}", id);
                var userEntity = await userRepository.GetByIdAsync(id);
                if (userEntity == null)
                {
                    _logger.LogWarning("User not found: {UserId}", id);
                    return NotFound();
                }
                var userDto = mapper.Map<UserDto>(userEntity);
                _logger.LogInformation("Successfully retrieved user: {UserId}, Email: {Email}", id, userDto.Email);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user: {UserId}", id);
                return StatusCode(500, "An error occurred while retrieving the user");
            }
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
            _logger.LogWarning("User update endpoint called but not implemented. UserId: {UserId}", id);
            return StatusCode(StatusCodes.Status501NotImplemented);
            //var userEntity = mapper.Map<User>(updateUserRequestDto);
            //var updatedUser = await userRepository.UpdateAsync(id, userEntity);
            //var updatedUserDto = mapper.Map<UpdateUserRequestDto>(updatedUser);
            //return Ok(updatedUserDto);
        }

        [HttpDelete("{id:Guid}")]
        [Authorize (Roles = "HR")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting user: {UserId}", id);
                var deletedUser = await userRepository.DeleteAsync(id);
                if (!deletedUser)
                {
                    _logger.LogWarning("User not found for deletion: {UserId}", id);
                    return NotFound();
                }
                _logger.LogInformation("User deleted successfully: {UserId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                return StatusCode(500, "An error occurred while deleting the user");
            }
        }
    }
}

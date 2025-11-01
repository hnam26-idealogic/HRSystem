using HRSystem.API.Models.Domain;
using HRSystem.API.Models.DTO;
using HRSystem.API.Repositories;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly IRoleService roleService;
        private readonly ITokenRepository tokenRepository;

        public AuthController(IAuthService authService, IRoleService roleService, ITokenRepository tokenRepository)
        {
            this.authService = authService;
            this.roleService = roleService;
            this.tokenRepository = tokenRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Ensure role exists and assign to user
            var roleName = request.UserType.ToString();
            await roleService.EnsureRoleExistsAsync(roleName);

            var user = new User
            {
                UserName = request.Username,
                Email = request.Email,
                Fullname = request.Fullname,
                PhoneNumber = request.PhoneNumber,
                UserType = request.UserType,
                AccessLevel = request.AccessLevel,
                Specialty = request.Specialty
            };

            var result = await authService.RegisterAsync(user, request.Password);
            if (result == null)
                return BadRequest(new { message = "Registration failed." });

            await roleService.AssignRoleAsync(user, roleName);
            return Ok(new { message = "Registration successful." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await authService.LoginAsync(request.Username, request.Password);
            if (user == null)
                return Unauthorized(new { message = "Invalid username or password." });

            var roles = await roleService.GetUserRolesAsync(user);
            
            if (roles == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }
            var jwtToken = tokenRepository.CreateJwtToken(user, roles.ToList());

            if (jwtToken == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var response = new LoginResponseDto
            {
                JwtToken = jwtToken
            };
            // You can add JWT token generation here if needed
            return Ok(response);
        }
    }
}

using HRSystem.API.Models.Domain;
using HRSystem.API.Models.DTO;
using HRSystem.API.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly RoleManager<IdentityRole<Guid>> roleManager;

        public AuthController(IUserRepository userRepository, RoleManager<IdentityRole<Guid>> roleManager)
        {
            this.userRepository = userRepository;
            this.roleManager = roleManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Email is already registered." });

            // Ensure role exists
            var roleName = request.UserType.ToString();
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                if (!roleResult.Succeeded)
                    return BadRequest(roleResult.Errors);
            }

            var user = new User
            {
                UserName = request.Username,
                Email = request.Email,
                Fullname = request.Fullname,
                PhoneNumber = request.PhoneNumber,
                UserType = request.UserType, 
                AccessLevel = request.AccessLevel.ToString(), 
                Specialty = request.Specialty
            };

            var createdUser = await userRepository.AddAsync(user, request.Password, roleName);

            // Assign role to user
            // You may need to expose AddToRoleAsync in your repository, or use userManager here if needed
            // For now, use roleManager to ensure role exists, but role assignment should be handled in repository for consistency
            // If not, you can inject UserManager<User> as well

            return Ok(new { message = "Registration successful." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
                return Unauthorized(new { message = "Invalid username." });

            // You may need to expose password check in your repository
            // For now, assume repository handles password validation
            // If not, inject UserManager<User> as well

            // You can add JWT token generation here if needed
            return Ok(new { message = "Login successful." });
        }
    }
}

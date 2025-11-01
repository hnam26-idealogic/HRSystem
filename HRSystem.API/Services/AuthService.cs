using HRSystem.API.Models.Domain;
using HRSystem.API.Repositories;
using Microsoft.AspNetCore.Identity;

namespace HRSystem.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository userRepository;
        private readonly UserManager<User> userManager;

        public AuthService(IUserRepository userRepository, UserManager<User> userManager)
        {
            this.userRepository = userRepository;
            this.userManager = userManager;
        }

        public async Task<User> RegisterAsync(User user, string password)
        {
            // Calls the repository to add the user and assign the role
            return await userRepository.AddAsync(user, password);
        }

        public async Task<User?> LoginAsync(string usernameOrEmail, string password)
        {
            // Find user by username or email
            User? user = usernameOrEmail.Contains("@")
                ? await userRepository.GetByEmailAsync(usernameOrEmail)
                : await userRepository.GetByUsernameAsync(usernameOrEmail);

            if (user == null)
                return null;

            // Use UserManager directly or expose a password check in the repo
            // Example if you expose CheckPasswordAsync in repo:
            var isValid = await userManager.CheckPasswordAsync(user, password);
            return isValid ? user : null;
        }
    }
}
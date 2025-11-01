using HRSystem.API.Models.Domain;

public interface IAuthService
{
    Task<User?> LoginAsync(string usernameOrEmail, string password);
    Task<User> RegisterAsync(User user, string password);
}
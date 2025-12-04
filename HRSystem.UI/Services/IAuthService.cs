using HRSystem.UI.DTOs;

namespace HRSystem.UI.Services
{
    public interface IAuthService
    {
        Task<Dictionary<string, List<string>>> GetAllClaimsAsync();
        Task<string?> GetCurrentUserEmailAsync();
        Task<string?> GetCurrentUserFamilyNameAsync();
        Task<string?> GetCurrentUserGivenNameAsync();
        Task<Guid?> GetCurrentUserIdAsync();
        Task<string?> GetCurrentUserNameAsync();
        Task<UserDto?> GetCurrentUserProfileAsync();
        Task<List<string>> GetCurrentUserRolesAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<bool> IsInAnyRoleAsync(params string[] roles);
        Task<bool> IsInRoleAsync(string role);
    }
}
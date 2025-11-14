
namespace HRSystem.UI.Services
{
    public interface ITokenService
    {
        Task ApplyTokenAsync(HttpClient httpClient);
        Task<string> GetTokenAsync();
        Task LogoutAsync();
        Task RemoveTokenAsync();
        Task SetTokenAsync(string token);
    }
}
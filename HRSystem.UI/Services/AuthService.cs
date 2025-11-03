using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HRSystem.UI.DTOs;

namespace HRSystem.UI.Services
{
    public class AuthService
    {
        private readonly HttpClient httpClient;
        private readonly JwtService jwtService;

        public AuthService(HttpClient httpClient, JwtService jwtService)
        {
            this.httpClient = httpClient;
            this.jwtService = jwtService;
        }

        public async Task<bool> LoginAsync(LoginRequestDto loginDto)
        {
            var response = await httpClient.PostAsJsonAsync("/api/Auth/login", loginDto);
            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (loginResponse?.JwtToken != null)
                {
                    await jwtService.SetTokenAsync(loginResponse.JwtToken);
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto registerDto)
        {
            var response = await httpClient.PostAsJsonAsync("/api/Auth/register", registerDto);
            return response.IsSuccessStatusCode;
        }
    }
}

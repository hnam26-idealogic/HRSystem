using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HRSystem.UI.DTOs;

namespace HRSystem.UI.Services
{
    public class AuthService
    {
        private readonly HttpClient httpClient;

        public AuthService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<bool> LoginAsync(LoginRequestDto loginDto)
        {
            var response = await httpClient.PostAsJsonAsync("/api/Auth/login", loginDto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto registerDto)
        {
            var response = await httpClient.PostAsJsonAsync("/api/Auth/register", registerDto);
            return response.IsSuccessStatusCode;
        }
    }
}

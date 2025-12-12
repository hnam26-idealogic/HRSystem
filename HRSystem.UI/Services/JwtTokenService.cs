using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HRSystem.UI.Services
{
    public class JwtTokenService : ITokenService
    {
        private readonly IJSRuntime _js;

        public JwtTokenService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task SetTokenAsync(string token)
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "jwtToken", token);
        }

        public async Task<string> GetTokenAsync()
        {
            return await _js.InvokeAsync<string>("localStorage.getItem", "jwtToken");
        }

        public async Task RemoveTokenAsync()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "jwtToken");
        }

        public async Task ApplyTokenAsync(HttpClient httpClient)
        {
            var token = await GetTokenAsync();
            httpClient.DefaultRequestHeaders.Authorization = token != null
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
        }

        public async Task LogoutAsync()
        {
            await RemoveTokenAsync();
        }
    }
}
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HRSystem.UI.Services
{
    public class JwtService
    {
        private readonly IJSRuntime js;

        public JwtService(IJSRuntime js)
        {
            this.js = js;
        }

        public async Task SetTokenAsync(string token)
        {
            await js.InvokeVoidAsync("localStorage.setItem", "jwtToken", token);
        }

        public async Task<string> GetTokenAsync()
        {
            return await js.InvokeAsync<string>("localStorage.getItem", "jwtToken");
        }

        public async Task RemoveTokenAsync()
        {
            await js.InvokeVoidAsync("localStorage.removeItem", "jwtToken");
        }

        public async Task ApplyJwtAsync(HttpClient httpClient)
        {
            var token = await GetTokenAsync();
            httpClient.DefaultRequestHeaders.Authorization = token != null
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
        }
    }
}
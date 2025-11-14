using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HRSystem.UI.Services
{
    public class EntraIdTokenService : ITokenService
    {
        private readonly IAccessTokenProvider tokenProvider;

        public EntraIdTokenService(IAccessTokenProvider tokenProvider)
        {
            this.tokenProvider = tokenProvider;
        }

        public async Task ApplyTokenAsync(HttpClient httpClient)
        {
            var tokenResult = await tokenProvider.RequestAccessToken();
            if (tokenResult.TryGetToken(out var token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token.Value);
            }
            else
            {
                httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public Task<string> GetTokenAsync() => Task.FromResult<string>(null);
        public Task SetTokenAsync(string token) => Task.CompletedTask;
        public Task RemoveTokenAsync() => Task.CompletedTask;
        public Task LogoutAsync() => Task.CompletedTask;
    }
}

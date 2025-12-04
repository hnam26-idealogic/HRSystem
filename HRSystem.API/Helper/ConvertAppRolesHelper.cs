using Microsoft.Graph;

namespace HRSystem.API.Helper
{
    public class ConvertAppRolesHelper
    {
        private readonly GraphServiceClient _graphServiceClient;

        public ConvertAppRolesHelper(GraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
        }

        public async Task<Dictionary<Guid, string>> LoadAppRoleMapAsync(string? clientId)
        {
            // service principal for your application
            var sp = await _graphServiceClient.ServicePrincipals
                .Request()
                .Filter($"appId eq '{clientId}'")
                .GetAsync();

            var principal = sp.CurrentPage.First();

            // build: AppRoleId → DisplayName
            return principal.AppRoles
                .Where(r => r.Id.HasValue)
                .ToDictionary(r => r.Id!.Value, r => r.DisplayName);
        }

    }
}

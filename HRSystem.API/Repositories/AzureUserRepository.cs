using Azure.Identity;
using HRSystem.API.Helper;
using Microsoft.Identity.Web;
using HRSystem.API.Models.Domain;
using Microsoft.Graph;

namespace HRSystem.API.Repositories
{
    public class AzureUserRepository : IUserRepository
    {
        private readonly Microsoft.Graph.GraphServiceClient _graphServiceClient;
        private readonly IConfiguration _configuration;
        private readonly ConvertAppRolesHelper convertAppRolesHelper;

        public AzureUserRepository(Microsoft.Graph.GraphServiceClient graphServiceClient, IConfiguration configuration, ConvertAppRolesHelper convertAppRolesHelper)
        {
            this._graphServiceClient = graphServiceClient;
            this._configuration = configuration;
            this.convertAppRolesHelper = convertAppRolesHelper;
        }

        public Task<Models.Domain.User> AddAsync(Models.Domain.User user, string password)
        {
            throw new NotSupportedException();
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            throw new NotSupportedException();
        }

        public async Task<(IEnumerable<Models.Domain.User> Items, int TotalCount)> GetAllAsync(int page = 1, int size = 10)
        {
            var appRoleMap = await convertAppRolesHelper.LoadAppRoleMapAsync(_configuration["AzureAd:ClientId"]);

            var usersPage = await _graphServiceClient.Users
                .Request()
                .Top(999)
                .GetAsync();

            var list = new List<Models.Domain.User>();

            foreach (var u in usersPage.CurrentPage)
            {
                var assignments = await _graphServiceClient.Users[u.Id]
                    .AppRoleAssignments
                    .Request()
                    .GetAsync();

                var roleNames = assignments.CurrentPage
                    .Where(a => appRoleMap.ContainsKey(a.AppRoleId.Value))
                    .Select(a => appRoleMap[a.AppRoleId.Value])
                    .ToList();

                list.Add(new Models.Domain.User
                {
                    Id = Guid.Parse(u.Id),
                    UserPrincipalName = u.UserPrincipalName,
                    Email = u.Mail,
                    Fullname = u.DisplayName,
                    AppRoles = roleNames
                });
            }

            // Only include users with AppRoles assigned
            var filtered = list.Where(x => x.AppRoles != null && x.AppRoles.Count > 0);
            var sorted = filtered.OrderBy(x => x.Fullname);
            var pageItems = sorted.Skip((page - 1) * size).Take(size);

            return (pageItems, filtered.Count());

        }


        public async Task<Models.Domain.User?> GetByEmailAsync(string email)
        {

            var appRoleMap = await convertAppRolesHelper.LoadAppRoleMapAsync(_configuration["AzureAd:ClientId"]);

            var u = await _graphServiceClient.Users[email]
                .Request()
                .GetAsync();

            if (u == null)
                return null;

            var assignments = await _graphServiceClient.Users[u.Id]
                .AppRoleAssignments
                .Request()
                .GetAsync();

            var roleNames = assignments.CurrentPage
                .Where(a => a.AppRoleId.HasValue && appRoleMap.ContainsKey(a.AppRoleId.Value))
                .Select(a => appRoleMap[a.AppRoleId.Value])
                .ToList();

            return new Models.Domain.User
            {
                Id = Guid.Parse(u.Id),
                UserPrincipalName = u.UserPrincipalName,
                Email = u.Mail,
                Fullname = u.DisplayName,
                AppRoles = roleNames
            };
        }


        public async Task<Models.Domain.User?> GetByIdAsync(Guid id)
        {

            var appRoleMap = await convertAppRolesHelper.LoadAppRoleMapAsync(_configuration["AzureAd:ClientId"]);

            var u = await _graphServiceClient.Users[id.ToString()]
                .Request()
                .GetAsync();

            if (u == null)
                return null;

            var assignments = await _graphServiceClient.Users[u.Id]
                .AppRoleAssignments
                .Request()
                .GetAsync();

            var roleNames = assignments.CurrentPage
                .Where(a => a.AppRoleId.HasValue && appRoleMap.ContainsKey(a.AppRoleId.Value))
                .Select(a => appRoleMap[a.AppRoleId.Value])
                .ToList();

            return new Models.Domain.User
            {
                Id = Guid.Parse(u.Id),
                UserPrincipalName = u.UserPrincipalName,
                Email = u.Mail,
                Fullname = u.DisplayName,
                AppRoles = roleNames
            };
        }


        public Task<Models.Domain.User?> GetByUsernameAsync(string username)
        {
            return GetByEmailAsync(username);
        }

        public Task<Models.Domain.User> UpdateAsync(Guid id, Models.Domain.User user)
        {
            throw new NotSupportedException();
        }
    }
}

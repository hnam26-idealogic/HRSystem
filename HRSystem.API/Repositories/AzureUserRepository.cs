using Azure.Identity;
using HRSystem.API.Helper;
using Microsoft.Identity.Web;
using HRSystem.API.Models.Domain;
using Microsoft.Graph;
using Microsoft.Extensions.Logging;

namespace HRSystem.API.Repositories
{
    public class AzureUserRepository : IUserRepository
    {
        private readonly Microsoft.Graph.GraphServiceClient _graphServiceClient;
        private readonly IConfiguration _configuration;
        private readonly ConvertAppRolesHelper convertAppRolesHelper;
        private readonly ILogger<AzureUserRepository> _logger;

        public AzureUserRepository(
            Microsoft.Graph.GraphServiceClient graphServiceClient,
            IConfiguration configuration,
            ConvertAppRolesHelper convertAppRolesHelper,
            ILogger<AzureUserRepository> logger)
        {
            this._graphServiceClient = graphServiceClient;
            this._configuration = configuration;
            this.convertAppRolesHelper = convertAppRolesHelper;
            _logger = logger;
        }

        public Task<Models.Domain.User> AddAsync(Models.Domain.User user, string password)
        {
            _logger.LogWarning("AddAsync called on AzureUserRepository (not supported)");
            throw new NotSupportedException();
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogWarning("DeleteAsync called on AzureUserRepository (not supported). UserId: {UserId}", id);
            throw new NotSupportedException();
        }

        public async Task<(IEnumerable<Models.Domain.User> Items, int TotalCount)> GetAllAsync(int page = 1, int size = 10)
        {
            try
            {
                _logger.LogInformation("Retrieving users from Microsoft Graph API. Page: {Page}, Size: {Size}", page, size);
                var appRoleMap = await convertAppRolesHelper.LoadAppRoleMapAsync(_configuration["AzureAd:ClientId"]);

                var usersPage = await _graphServiceClient.Users
                    .Request()
                    .Top(999)
                    .GetAsync();

                _logger.LogDebug("Retrieved {Count} users from Graph API", usersPage.CurrentPage.Count);

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

                _logger.LogInformation("Retrieved {Count} users with app roles from Graph API. TotalCount: {TotalCount}",
                    pageItems.Count(), filtered.Count());

                return (pageItems, filtered.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users from Microsoft Graph API");
                throw;
            }
        }


        public async Task<Models.Domain.User?> GetByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation("Retrieving user from Microsoft Graph API by email: {Email}", email);
                var appRoleMap = await convertAppRolesHelper.LoadAppRoleMapAsync(_configuration["AzureAd:ClientId"]);

                var u = await _graphServiceClient.Users[email]
                    .Request()
                    .GetAsync();

                if (u == null)
                {
                    _logger.LogWarning("User not found in Graph API: {Email}", email);
                    return null;
                }

                var assignments = await _graphServiceClient.Users[u.Id]
                    .AppRoleAssignments
                    .Request()
                    .GetAsync();

                var roleNames = assignments.CurrentPage
                    .Where(a => a.AppRoleId.HasValue && appRoleMap.ContainsKey(a.AppRoleId.Value))
                    .Select(a => appRoleMap[a.AppRoleId.Value])
                    .ToList();

                _logger.LogInformation("User retrieved from Graph API: {Email}, Roles: {Roles}", email, string.Join(", ", roleNames));

                return new Models.Domain.User
                {
                    Id = Guid.Parse(u.Id),
                    UserPrincipalName = u.UserPrincipalName,
                    Email = u.Mail,
                    Fullname = u.DisplayName,
                    AppRoles = roleNames
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email from Graph API: {Email}", email);
                throw;
            }
        }


        public async Task<Models.Domain.User?> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Retrieving user from Microsoft Graph API by ID: {UserId}", id);
                var appRoleMap = await convertAppRolesHelper.LoadAppRoleMapAsync(_configuration["AzureAd:ClientId"]);

                var u = await _graphServiceClient.Users[id.ToString()]
                    .Request()
                    .GetAsync();

                if (u == null)
                {
                    _logger.LogWarning("User not found in Graph API: {UserId}", id);
                    return null;
                }

                var assignments = await _graphServiceClient.Users[u.Id]
                    .AppRoleAssignments
                    .Request()
                    .GetAsync();

                var roleNames = assignments.CurrentPage
                    .Where(a => a.AppRoleId.HasValue && appRoleMap.ContainsKey(a.AppRoleId.Value))
                    .Select(a => appRoleMap[a.AppRoleId.Value])
                    .ToList();

                _logger.LogInformation("User retrieved from Graph API: {UserId}, Email: {Email}", id, u.Mail);

                return new Models.Domain.User
                {
                    Id = Guid.Parse(u.Id),
                    UserPrincipalName = u.UserPrincipalName,
                    Email = u.Mail,
                    Fullname = u.DisplayName,
                    AppRoles = roleNames
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID from Graph API: {UserId}", id);
                throw;
            }
        }


        public Task<Models.Domain.User?> GetByUsernameAsync(string username)
        {
            return GetByEmailAsync(username);
        }

        public Task<Models.Domain.User> UpdateAsync(Guid id, Models.Domain.User user)
        {
            _logger.LogWarning("UpdateAsync called on AzureUserRepository (not supported). UserId: {UserId}", id);
            throw new NotSupportedException();
        }
    }
}

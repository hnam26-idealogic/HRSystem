using HRSystem.API.Models.Domain;

namespace HRSystem.API.Repositories
{
    public class AzureUserRepository : IUserRepository
    {
        private readonly Microsoft.Graph.GraphServiceClient _graphServiceClient;

        public AzureUserRepository(Microsoft.Graph.GraphServiceClient graphServiceClient)
        {
            this._graphServiceClient = graphServiceClient;


        }

        public Task<User> AddAsync(User user, string password)
        {
            throw new NotSupportedException();
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            throw new NotSupportedException();
        }

        public async Task<(IEnumerable<User> Items, int TotalCount)> GetAllAsync(int page = 1, int size = 10)
        {
            var users = await _graphServiceClient.Users.Request().Top(5000).GetAsync();

            var result = users.CurrentPage.Select(user => new User
            {
                Id = Guid.Parse(user.Id!),
                UserName = user.UserPrincipalName!,
                Email = user.Mail!,
                Fullname = user.DisplayName!,
            });

            return (result.OrderBy(user => user.Fullname).Skip((page - 1)*size).Take(size), result.Count());
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var user = await _graphServiceClient.Users[email].Request().GetAsync();
            if (user == null)
            {
                return null;
            }

            return new User
            {
                Id = Guid.Parse(user.Id!),
                UserName = user.UserPrincipalName!,
                Email = user.Mail!,
                Fullname = user.DisplayName!,
            };
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            var user = await _graphServiceClient.Users[id.ToString()].Request().GetAsync();
            if (user == null)
            {
                return null;
            }

            return new User
            {
                Id = Guid.Parse(user.Id!),
                UserName = user.UserPrincipalName!,
                Email = user.Mail!,
                Fullname = user.DisplayName!,
            };
        }

        public Task<User?> GetByUsernameAsync(string username)
        {
            return GetByEmailAsync(username);
        }

        public Task<User> UpdateAsync(Guid id, User user)
        {
            throw new NotSupportedException();
        }
    }
}

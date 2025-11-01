using HRSystem.API.Models.Domain;

namespace HRSystem.API.Repositories
{
    public interface ITokenRepository
    {
        string CreateJwtToken(User user, List<string> roles);
    }
}

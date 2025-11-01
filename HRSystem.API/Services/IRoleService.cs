using HRSystem.API.Models.Domain;
using System.Threading.Tasks;

namespace HRSystem.API.Services
{
	public interface IRoleService
	{
		Task EnsureRoleExistsAsync(string roleName);
		Task AssignRoleAsync(User user, string roleName);
		Task<IList<string>> GetUserRolesAsync(User user);
	}
}

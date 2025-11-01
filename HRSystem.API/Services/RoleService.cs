using HRSystem.API.Models.Domain;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Add this using

public class RoleService : IRoleService
{
    private readonly UserManager<User> userManager;
    private readonly RoleManager<IdentityRole<Guid>> roleManager; // Add RoleManager

    public RoleService(UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager) // Update constructor
    {
        this.userManager = userManager;
        this.roleManager = roleManager;
    }

    public async Task EnsureRoleExistsAsync(string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName)) // Use roleManager instead
        {
            var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName)); // Use roleManager instead
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task AssignRoleAsync(User user, string roleName)
    {
        var result = await userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
            throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task<IList<string>> GetUserRolesAsync(User user)
    {
        return await userManager.GetRolesAsync(user);
    }
}
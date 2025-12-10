using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using HRSystem.UI.DTOs;
using Microsoft.Extensions.Logging;

namespace HRSystem.UI.Services;

public class AuthService : IAuthService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AuthenticationStateProvider authenticationStateProvider, ILogger<AuthService> logger)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _logger = logger;
    }

    private async Task<ClaimsPrincipal?> GetCurrentUserAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return authState.User?.Identity?.IsAuthenticated == true ? authState.User : null;
    }

    public async Task<Guid?> GetCurrentUserIdAsync()
    {
        try
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                _logger.LogDebug("No authenticated user found");
                return null;
            }

            // Try different claim types for user ID
            var userIdClaim = user.FindFirst("oid")                      // Azure AD Object ID (most reliable)
                           ?? user.FindFirst(ClaimTypes.NameIdentifier)  // Standard claim
                           ?? user.FindFirst("sub")                      // OIDC standard
                           ?? user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogDebug("Retrieved current user ID: {UserId}", userId);
                return userId;
            }

            _logger.LogWarning("Could not extract user ID from claims");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user ID");
            throw;
        }
    }

    public async Task<string?> GetCurrentUserEmailAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return null;

        return user.FindFirst("preferred_username")?.Value  // Most common in Entra ID
            ?? user.FindFirst(ClaimTypes.Email)?.Value
            ?? user.FindFirst("email")?.Value
            ?? user.FindFirst("upn")?.Value;                // User Principal Name
    }

    public async Task<string?> GetCurrentUserNameAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return null;

        return user.FindFirst("name")?.Value
            ?? user.FindFirst(ClaimTypes.Name)?.Value
            ?? user.FindFirst(ClaimTypes.GivenName)?.Value
            ?? user.Identity?.Name;
    }

    public async Task<string?> GetCurrentUserGivenNameAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return null;

        return user.FindFirst(ClaimTypes.GivenName)?.Value
            ?? user.FindFirst("given_name")?.Value;
    }

    public async Task<string?> GetCurrentUserFamilyNameAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return null;

        return user.FindFirst(ClaimTypes.Surname)?.Value
            ?? user.FindFirst("family_name")?.Value;
    }

    public async Task<List<string>> GetCurrentUserRolesAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return new List<string>();

        return user.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
    }

    public async Task<bool> IsInRoleAsync(string role)
    {
        var user = await GetCurrentUserAsync();
        return user?.IsInRole(role) ?? false;
    }

    public async Task<bool> IsInAnyRoleAsync(params string[] roles)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return false;

        foreach (var role in roles)
        {
            if (user.IsInRole(role))
                return true;
        }
        return false;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var user = await GetCurrentUserAsync();
            var isAuthenticated = user != null;
            _logger.LogDebug("User authentication status: {IsAuthenticated}", isAuthenticated);
            return isAuthenticated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication status");
            throw;
        }
    }


    public async Task<UserDto?> GetCurrentUserProfileAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return null;

        return new UserDto
        {
            Id = await GetCurrentUserIdAsync() ?? Guid.Empty,
            Email = await GetCurrentUserEmailAsync(),
            Fullname = await GetCurrentUserNameAsync(),
            GivenName = await GetCurrentUserGivenNameAsync(),
            FamilyName = await GetCurrentUserFamilyNameAsync(),
            Roles = await GetCurrentUserRolesAsync()
        };
    }

    public async Task<Dictionary<string, List<string>>> GetAllClaimsAsync()
    {
        var user = await GetCurrentUserAsync();
        var claimsDict = new Dictionary<string, List<string>>();

        if (user != null)
        {
            foreach (var claim in user.Claims)
            {
                if (!claimsDict.ContainsKey(claim.Type))
                {
                    claimsDict[claim.Type] = new List<string>();
                }
                claimsDict[claim.Type].Add(claim.Value);
            }
        }

        return claimsDict;
    }
}


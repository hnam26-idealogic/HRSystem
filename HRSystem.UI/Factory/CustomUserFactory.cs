using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using System.Security.Claims;
using System.Text.Json;

namespace HRSystem.UI.Factory;

public class CustomUserFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
{
    private readonly IAccessTokenProviderAccessor _accessor;

    public CustomUserFactory(IAccessTokenProviderAccessor accessor)
        : base(accessor)
    {
        _accessor = accessor;
    }

    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(
        RemoteUserAccount account,
        RemoteAuthenticationUserOptions options)
    {
        var user = await base.CreateUserAsync(account, options);

        if (user.Identity is ClaimsIdentity identity && identity.IsAuthenticated)
        {
            Console.WriteLine("=== Creating User - ID Token Claims ===");
            foreach (var claim in identity.Claims)
            {
                Console.WriteLine($"  {claim.Type}: {claim.Value}");
            }

            // Get roles from access token
            try
            {
                var tokenResult = await _accessor.TokenProvider.RequestAccessToken();

                if (tokenResult.TryGetToken(out var accessToken))
                {
                    Console.WriteLine("=== Access Token Retrieved ===");

                    // Parse JWT access token
                    var claims = ParseClaimsFromJwt(accessToken.Value);

                    // Find and add role claims
                    var roleClaims = claims.Where(c =>
                        c.Type == "roles" ||
                        c.Type == "role" ||
                        c.Type == ClaimTypes.Role).ToList();

                    if (roleClaims.Any())
                    {
                        Console.WriteLine("=== Roles Found in Access Token ===");
                        foreach (var roleClaim in roleClaims)
                        {
                            // Add as standard Role claim type
                            var roleValue = roleClaim.Value;
                            identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                            identity.AddClaim(new Claim("roles", roleValue));
                            Console.WriteLine($"  ✅ Added Role: {roleValue}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("  ⚠️ No roles found in access token");
                    }
                }
                else
                {
                    Console.WriteLine("  ❌ Failed to retrieve access token");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ Error retrieving access token: {ex.Message}");
            }

            // Also check AdditionalProperties for roles
            if (account.AdditionalProperties.TryGetValue("roles", out var rolesObj))
            {
                Console.WriteLine("=== Roles from AdditionalProperties ===");
                if (rolesObj is JsonElement rolesElement)
                {
                    if (rolesElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var role in rolesElement.EnumerateArray())
                        {
                            var roleValue = role.GetString();
                            if (!string.IsNullOrEmpty(roleValue))
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                                identity.AddClaim(new Claim("roles", roleValue));
                                Console.WriteLine($"  ✅ Added Role from properties: {roleValue}");
                            }
                        }
                    }
                    else if (rolesElement.ValueKind == JsonValueKind.String)
                    {
                        var roleValue = rolesElement.GetString();
                        if (!string.IsNullOrEmpty(roleValue))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                            identity.AddClaim(new Claim("roles", roleValue));
                            Console.WriteLine($"  ✅ Added Role from properties: {roleValue}");
                        }
                    }
                }
            }

            Console.WriteLine("=== Final User Claims (Including Roles) ===");
            foreach (var claim in identity.Claims)
            {
                if (claim.Type == ClaimTypes.Role || claim.Type == "roles")
                {
                    Console.WriteLine($"  🎯 {claim.Type}: {claim.Value}");
                }
            }
            Console.WriteLine("==================");
        }

        return user;
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();

        try
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);

            if (keyValuePairs != null)
            {
                foreach (var kvp in keyValuePairs)
                {
                    if (kvp.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in kvp.Value.EnumerateArray())
                        {
                            var value = item.ValueKind == JsonValueKind.String
                                ? item.GetString()
                                : item.ToString();

                            if (!string.IsNullOrEmpty(value))
                            {
                                claims.Add(new Claim(kvp.Key, value));
                            }
                        }
                    }
                    else if (kvp.Value.ValueKind == JsonValueKind.String)
                    {
                        var value = kvp.Value.GetString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            claims.Add(new Claim(kvp.Key, value));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing JWT: {ex.Message}");
        }

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
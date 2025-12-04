using HRSystem.UI;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HRSystem.UI.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using HRSystem.UI.Factory;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient with authentication
var apiBaseUrl = builder.Configuration["Api:BaseUrl"];

builder.Services.AddHttpClient("AuthenticatedAPI", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl!);
})
.AddHttpMessageHandler(sp =>
{
    var handler = sp.GetRequiredService<AuthorizationMessageHandler>();
    handler.ConfigureHandler(
        authorizedUrls: new[] { apiBaseUrl! },
        scopes: new[] { builder.Configuration["Api:Scopes:0"]! });
    return handler;
});

// Register default HttpClient for services
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("AuthenticatedAPI"));


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, EntraIdTokenService>();
builder.Services.AddScoped<ICandidateService, CandidateService>();
builder.Services.AddScoped<IInterviewService, InterviewService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 104857600; }); // 100MB

// Configure MSAL authentication
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);

    // Add API scope
    var scopes = builder.Configuration.GetSection("Api:Scopes").Get<string[]>();
    if (scopes != null)
    {
        foreach (var scope in scopes)
        {
            options.ProviderOptions.DefaultAccessTokenScopes.Add(scope);
        }
    }
    //options.ProviderOptions.DefaultAccessTokenScopes.Add("User.Read.All");

    options.UserOptions.RoleClaim = "roles";
})
    .AddAccountClaimsPrincipalFactory<CustomUserFactory>(); 

// Register custom user factory
builder.Services.AddScoped<CustomUserFactory>();

await builder.Build().RunAsync();

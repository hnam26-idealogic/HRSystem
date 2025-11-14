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

//// Set HttpClient to use the API server address
//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://hrsystem-api-1-ajcghmadc8cwd5hb.southeastasia-01.azurewebsites.net") });
////builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7106") });


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


builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ITokenService, EntraIdTokenService>();
builder.Services.AddScoped<CandidateService>();
builder.Services.AddScoped<InterviewService>();
builder.Services.AddScoped<UserService>();

builder.Services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 104857600; }); // 100MB

// Configure MSAL authentication
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);

    var scopes = builder.Configuration.GetSection("Api:Scopes").Get<string[]>();
    if (scopes != null)
    {
        foreach (var scope in scopes)
        {
            options.ProviderOptions.DefaultAccessTokenScopes.Add(scope);
        }
    }

    options.UserOptions.RoleClaim = "roles";
})
    .AddAccountClaimsPrincipalFactory<CustomUserFactory>(); // <-- Add this line

// Register custom user factory
builder.Services.AddScoped<CustomUserFactory>();

await builder.Build().RunAsync();

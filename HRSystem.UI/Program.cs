using HRSystem.UI;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HRSystem.UI.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Set HttpClient to use the API server address
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7106/") });

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CandidateService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<InterviewService>();
builder.Services.AddScoped<UserService>();

builder.Services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 104857600; }); // 100MB

await builder.Build().RunAsync();

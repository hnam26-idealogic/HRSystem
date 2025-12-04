using HRSystem.API.Data;
using HRSystem.API.Mappings;
using HRSystem.API.Repositories;
using HRSystem.API.Services;
using HRSystem.API.Helper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using HRSystem.API.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddHttpContextAccessor();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Use Azure SQL if available, otherwise fallback to local SQL Server
var azureSqlConnection = builder.Configuration.GetConnectionString("AzureSqlConnection");
var localSqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<HRSystemDBContext>(options =>
    options.UseSqlServer(!string.IsNullOrEmpty(azureSqlConnection) ? azureSqlConnection : localSqlConnection));

builder.Services.AddScoped<IUserRepository, AzureUserRepository>();
builder.Services.AddScoped<ICandidateRepository, SQLCandidateRepository>();
builder.Services.AddScoped<IInterviewRepository, SQLInterviewRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IFileStorageService, AzureBlobFileStorageService>();
builder.Services.AddScoped<IRecordingStorageService, LocalRecordingStorageService>();

builder.Services.AddScoped<ConvertAppRolesHelper>();


builder.Services.AddAutoMapper(cfg => { }, typeof(AutoMapperProfiles));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
     .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
     .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
     .EnableTokenAcquisitionToCallDownstreamApi()
     .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
     .AddInMemoryTokenCaches();

builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options => options.Events = new RejectSessionCookieWhenAccountNotInCacheEvents());


// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireHRRole", policy =>
        policy.RequireRole("HR"));

    options.AddPolicy("RequireInterviewerRole", policy =>
        policy.RequireRole("Interviewer"));

    options.AddPolicy("RequireHROrInterviewer", policy =>
        policy.RequireRole("HR", "Interviewer"));
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy
              .WithOrigins(
                            "https://hrsystem-azfkepg9eeadf7bx.southeastasia-01.azurewebsites.net",
                            "https://localhost:7208"
                        )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Resumes")),
    RequestPath = "/Resumes"
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Recordings")),
    RequestPath = "/Recordings"
});

app.MapControllers();

// app.UseMiddleware<HRSystem.API.Middleware.ErrorHandlingMiddleware>();

app.Run();

using HRSystem.API.Data;
using HRSystem.API.Mappings;
using HRSystem.API.Models.Domain;
using HRSystem.API.Repositories;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

//builder.Services.AddScoped<IUserRepository, SQLUserRepository>();
builder.Services.AddScoped<ICandidateRepository, SQLCandidateRepository>();
builder.Services.AddScoped<IInterviewRepository, SQLInterviewRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
//builder.Services.AddScoped<IAuthService, AuthService>();
//builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IFileStorageService, AzureBlobFileStorageService>();
builder.Services.AddScoped<IRecordingStorageService, LocalRecordingStorageService>();

builder.Services.AddAutoMapper(cfg => { }, typeof(AutoMapperProfiles));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddIdentity<User, IdentityRole<Guid>>()
//    .AddEntityFrameworkStores<HRSystemDBContext>()
//    .AddDefaultTokenProviders();

//builder.Services.Configure<IdentityOptions>(options =>
//{
//    options.Password.RequireDigit = true;
//    options.Password.RequireLowercase = true;
//    options.Password.RequireNonAlphanumeric = false;
//    options.Password.RequireUppercase = true;
//    options.Password.RequiredLength = 6;
//    options.Password.RequiredUniqueChars = 1;
//});

// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// })
// .AddJwtBearer(options =>
// {
//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidateLifetime = true,
//         ValidateIssuerSigningKey = true,
//         ValidIssuer = builder.Configuration["Jwt:Issuer"],
//         ValidAudience = builder.Configuration["Jwt:Audience"],
//         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//     };
// });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(jwtOptions =>
    {
        builder.Configuration.Bind("AzureAd", jwtOptions);

        // IMPORTANT: Configure audience validation
        var clientId = builder.Configuration["AzureAd:ClientId"];

        jwtOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            // Accept both formats of audience
            ValidAudiences = new[]
            {
                clientId!,                           // Just the client ID
                $"api://{clientId}",                 // With api:// prefix
            },

            // Log validation details
            LogValidationExceptions = true
        };

        jwtOptions.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"❌ Authentication failed: {context.Exception.Message}");
                if (context.Exception is SecurityTokenInvalidAudienceException)
                {
                    Console.WriteLine($"   Expected audience: {string.Join(", ", jwtOptions.TokenValidationParameters.ValidAudiences)}");
                    Console.WriteLine($"   Token audience: {context.Exception.Data["InvalidAudience"]}");
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("✅ Token validated successfully");
                var claims = context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}");
                Console.WriteLine($"   Claims: {string.Join(", ", claims ?? Array.Empty<string>())}");
                return Task.CompletedTask;
            }
        };
    },
    msIdentityOptions =>
    {
        builder.Configuration.Bind("AzureAd", msIdentityOptions);
    });


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
        //policy.WithOrigins("https://localhost:7208") // Blazor UI origin
        policy.WithOrigins("https://hrsystem-azfkepg9eeadf7bx.southeastasia-01.azurewebsites.net/") // Azure UI 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
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

app.Run();

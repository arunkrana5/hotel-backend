using System.Text;
using HotelOS.Api.Hubs;
using HotelOS.Api.Services;
using HotelOS.Application.Common.Interfaces;
using HotelOS.Infrastructure;
using HotelOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers & SignalR
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IRealtimeNotificationSender, RealtimeNotificationSender>();

// 2. Add Infrastructure Services
builder.Services.AddInfrastructureServices(builder.Configuration);

// 3. Add Authentication & JWT Bearer
string secretKey = builder.Configuration["Jwt:SecretKey"] ?? "HotelOS_Super_Secure_Enterprise_Secret_Key_2026_DotNet10_LTS_!#*";
string issuer = builder.Configuration["Jwt:Issuer"] ?? "HotelOS.Api";
string audience = builder.Configuration["Jwt:Audience"] ?? "HotelOS.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Internal dev
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// 4. Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientApp", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// 5. Automatic Database Seeding & Schema Setup
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    if (args.Contains("--reset-db"))
    {
        Console.WriteLine("[HotelOS] --reset-db flag detected. Purging schemas, running EF migrations, and re-seeding baseline data...");
        await seeder.ResetAndReSeedAsync();
        Console.WriteLine("[HotelOS] Database successfully reset and re-seeded. Ready for use.");
        return;
    }
    if (args.Contains("--seed-only"))
    {
        Console.WriteLine("[HotelOS] --seed-only flag detected. Running database seeder...");
        await seeder.SeedAsync();
        Console.WriteLine("[HotelOS] Seeding finished successfully.");
        return;
    }
    await seeder.SeedAsync();
}

// 6. Middleware Pipeline
app.UseCors("AllowClientApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<HotelOS.Api.Hubs.NotificationHub>("/hubs/notifications");

// Forward guest portal links to frontend UI
app.MapGet("/guest/q/{token}", (string token, IConfiguration config) =>
{
    var clientBase = config["GuestPortal:BaseUrl"]
        ?? (!string.IsNullOrWhiteSpace(config["App:ClientUrl"]) ? $"{config["App:ClientUrl"]!.TrimEnd('/')}/guest/q" : null)
        ?? "https://hotal-dho2.onrender.com/guest/q";
    return Results.Redirect($"{clientBase.TrimEnd('/')}/{token}", permanent: false);
});

app.MapGet("/guest/order/{orderNumber}", (string orderNumber, IConfiguration config) =>
{
    var clientUrl = config["App:ClientUrl"] ?? "https://hotal-dho2.onrender.com";
    return Results.Redirect($"{clientUrl.TrimEnd('/')}/guest/order/{orderNumber}", permanent: false);
});

app.Run();

public partial class Program { }

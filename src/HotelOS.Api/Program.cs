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
        policy.WithOrigins(
            builder.Configuration["App:ClientUrl"] ?? "http://localhost:3000",
            "http://localhost:5173"
        )
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
    await seeder.SeedAsync();
}

// 6. Middleware Pipeline
app.UseCors("AllowClientApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<HotelOS.Api.Hubs.NotificationHub>("/hubs/notifications");

app.Run();

public partial class Program { }

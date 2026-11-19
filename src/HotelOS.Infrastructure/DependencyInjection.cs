using HotelOS.Application.Common.Interfaces;
using HotelOS.Infrastructure.Persistence;
using HotelOS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HotelOS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=127.0.0.1;Port=5432;Database=hotelos_db;Username=postgres;Password=postgres";

        services.AddDbContext<HotelOSDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(HotelOSDbContext).Assembly.FullName);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "platform");
            });
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMasterService, MasterService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<IFormService, FormService>();
        services.AddScoped<IPmsService, PmsService>();
        services.AddScoped<IHousekeepingService, HousekeepingService>();
        services.AddScoped<IRestaurantService, RestaurantService>();
        services.AddScoped<IBanquetService, BanquetService>();
        services.AddScoped<IOperationsService, OperationsService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IFinanceService, FinanceService>();
        services.AddScoped<IHrService, HrService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<ISecurityService, SecurityService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDocumentNumberingService, DocumentNumberingService>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}

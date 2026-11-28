using HotelOS.Domain.Audit.Entities;
using HotelOS.Domain.Banquet.Entities;
using HotelOS.Domain.Configuration.Entities;
using HotelOS.Domain.Finance.Entities;
using HotelOS.Domain.Housekeeping.Entities;
using HotelOS.Domain.Hr.Entities;
using HotelOS.Domain.Identity.Entities;
using HotelOS.Domain.Inventory.Entities;
using HotelOS.Domain.Maintenance.Entities;
using HotelOS.Domain.Masters.Entities;
using HotelOS.Domain.Operations.Entities;
using HotelOS.Domain.Pms.Entities;
using HotelOS.Domain.Purchase.Entities;
using HotelOS.Domain.Restaurant.Entities;
using HotelOS.Domain.Security.Entities;
using HotelOS.Domain.Workflow.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelOS.Infrastructure.Persistence;

public class HotelOSDbContext : DbContext
{
    public HotelOSDbContext(DbContextOptions<HotelOSDbContext> options) : base(options)
    {
    }

    // Identity schema
    public DbSet<User> Users => Set<User>();
    public DbSet<UserCredential> UserCredentials => Set<UserCredential>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();
    public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();
    public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();
    public DbSet<UserPropertyAssignment> UserPropertyAssignments => Set<UserPropertyAssignment>();
    public DbSet<AppNotification> Notifications => Set<AppNotification>();

    // Masters schema
    public DbSet<MasterCategory> MasterCategories => Set<MasterCategory>();
    public DbSet<MasterDefinition> MasterDefinitions => Set<MasterDefinition>();
    public DbSet<MasterValue> MasterValues => Set<MasterValue>();

    // Configuration schema
    public DbSet<NavigationMenu> NavigationMenus => Set<NavigationMenu>();
    public DbSet<DynamicFormDefinition> DynamicFormDefinitions => Set<DynamicFormDefinition>();
    public DbSet<DynamicFormField> DynamicFormFields => Set<DynamicFormField>();
    public DbSet<DocumentNumberConfiguration> DocumentNumberConfigurations => Set<DocumentNumberConfiguration>();
    public DbSet<DocumentNumberAudit> DocumentNumberAudits => Set<DocumentNumberAudit>();

    // Workflow schema
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowHistory> WorkflowHistories => Set<WorkflowHistory>();

    // Audit schema
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // PMS schema
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Folio> Folios => Set<Folio>();
    public DbSet<FolioItem> FolioItems => Set<FolioItem>();

    // Housekeeping schema
    public DbSet<HousekeepingTask> HousekeepingTasks => Set<HousekeepingTask>();
    public DbSet<LinenItem> LinenItems => Set<LinenItem>();
    public DbSet<LaundryOrder> LaundryOrders => Set<LaundryOrder>();
    public DbSet<LaundryOrderItem> LaundryOrderItems => Set<LaundryOrderItem>();

    // Restaurant schema
    public DbSet<DiningTable> DiningTables => Set<DiningTable>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<RestaurantOrder> RestaurantOrders => Set<RestaurantOrder>();
    public DbSet<RestaurantOrderItem> RestaurantOrderItems => Set<RestaurantOrderItem>();
    public DbSet<RoomQrCode> RoomQrCodes => Set<RoomQrCode>();
    public DbSet<GuestOrderingSession> GuestOrderingSessions => Set<GuestOrderingSession>();

    // Banquet schema
    public DbSet<BanquetVenue> BanquetVenues => Set<BanquetVenue>();
    public DbSet<BanquetBooking> BanquetBookings => Set<BanquetBooking>();

    // Operations schema
    public DbSet<GuestServiceRequest> GuestServiceRequests => Set<GuestServiceRequest>();

    // Purchase schema
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<PurchaseRequisition> PurchaseRequisitions => Set<PurchaseRequisition>();
    public DbSet<PurchaseRequisitionItem> PurchaseRequisitionItems => Set<PurchaseRequisitionItem>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();

    // Inventory schema
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

    // Finance schema
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalLine> JournalLines => Set<JournalLine>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    // HR schema
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    // Maintenance schema
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    // Security schema
    public DbSet<VisitorLog> VisitorLogs => Set<VisitorLog>();
    public DbSet<IncidentReport> IncidentReports => Set<IncidentReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelOSDbContext).Assembly);
    }
}

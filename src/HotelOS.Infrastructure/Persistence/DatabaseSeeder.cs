using HotelOS.Application.Common.Interfaces;
using HotelOS.Domain.Configuration.Entities;
using HotelOS.Domain.Identity.Entities;
using HotelOS.Domain.Identity.Enums;
using HotelOS.Domain.Masters.Entities;
using HotelOS.Domain.Pms.Entities;
using HotelOS.Domain.Workflow.Entities;
using HotelOS.Domain.Banquet.Entities;
using HotelOS.Domain.Housekeeping.Entities;
using HotelOS.Domain.Operations.Entities;
using HotelOS.Domain.Restaurant.Entities;
using HotelOS.Domain.Finance.Entities;
using HotelOS.Domain.Hr.Entities;
using HotelOS.Domain.Inventory.Entities;
using HotelOS.Domain.Maintenance.Entities;
using HotelOS.Domain.Purchase.Entities;
using HotelOS.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelOS.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly HotelOSDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<DatabaseSeeder> _logger;

    private record SubmenuSeedItem(string Code, string Title, string Path, int Order, string? ReqPerm = null);
    private record MenuSeedItem(string Code, string Title, string Path, string Icon, string Module, int Order, string? ReqPerm, SubmenuSeedItem[] Submenus);

    public DatabaseSeeder(
        HotelOSDbContext context,
        IPasswordService passwordService,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _logger = logger;
    }

    public async Task ResetAndReSeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("HotelOS Database Reset: Purging all application schemas...");

        const string dropSql = @"
            DROP SCHEMA IF EXISTS identity CASCADE;
            DROP SCHEMA IF EXISTS masters CASCADE;
            DROP SCHEMA IF EXISTS configuration CASCADE;
            DROP SCHEMA IF EXISTS workflow CASCADE;
            DROP SCHEMA IF EXISTS audit CASCADE;
            DROP SCHEMA IF EXISTS pms CASCADE;
            DROP SCHEMA IF EXISTS housekeeping CASCADE;
            DROP SCHEMA IF EXISTS restaurant CASCADE;
            DROP SCHEMA IF EXISTS banquet CASCADE;
            DROP SCHEMA IF EXISTS operations CASCADE;
            DROP SCHEMA IF EXISTS purchase CASCADE;
            DROP SCHEMA IF EXISTS inventory CASCADE;
            DROP SCHEMA IF EXISTS finance CASCADE;
            DROP SCHEMA IF EXISTS hr CASCADE;
            DROP SCHEMA IF EXISTS maintenance CASCADE;
            DROP SCHEMA IF EXISTS security CASCADE;
            DROP SCHEMA IF EXISTS platform CASCADE;
            DROP SCHEMA IF EXISTS public CASCADE;
            CREATE SCHEMA public;
        ";

        await _context.Database.ExecuteSqlRawAsync(dropSql, cancellationToken);
        _logger.LogInformation("All schemas dropped. Running fresh EF Core migrations...");

        await _context.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("Migrations applied. Seeding fresh enterprise baseline data...");

        await SeedAsync(cancellationToken);
        _logger.LogInformation("HotelOS Database reset and baseline seed completed successfully.");
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.MigrateAsync(cancellationToken);

        // 1. Roles & Permissions & SuperAdmin
        await SeedIdentityAsync(cancellationToken);

        // 2. Master Categories, Definitions & Values
        await SeedMastersAsync(cancellationToken);

        // 3. Database-Driven Navigation Menus
        await SeedNavigationMenusAsync(cancellationToken);

        // 4. Dynamic Forms & Workflows
        await SeedDynamicEnginesAsync(cancellationToken);

        // 5. PMS Core (Rooms, Sample Guests, Reservations & Folios)
        await SeedPmsAsync(cancellationToken);

        // 6. Operations (Housekeeping, Restaurant, Banquet, Laundry, Guest Requests)
        await SeedOperationsAsync(cancellationToken);

        // 7. Enterprise ERP (Purchase, Inventory, Finance, HR, Maintenance, Security)
        await SeedErpAsync(cancellationToken);

        // 8. Document Numbering Configurations
        await SeedDocumentNumberingAsync(cancellationToken);

        _logger.LogInformation("Core dynamic engines, PMS, Operations, ERP and Document Numbering seed data initialized successfully.");
    }

    private async Task SeedIdentityAsync(CancellationToken cancellationToken)
    {
        var superAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "SUPERADMIN", cancellationToken);
        if (superAdminRole == null)
        {
            superAdminRole = new Role
            {
                Name = "SuperAdmin",
                NormalizedName = "SUPERADMIN",
                Description = "System Super Administrator with unrestricted access",
                IsSystemRole = true,
                CreatedBy = "SYSTEM"
            };
            _context.Roles.Add(superAdminRole);
        }

        var itAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "IT_ADMINISTRATOR", cancellationToken);
        if (itAdminRole == null)
        {
            itAdminRole = new Role
            {
                Name = "IT Administrator",
                NormalizedName = "IT_ADMINISTRATOR",
                Description = "IT Department Administrator managing configurations, users, and masters",
                IsSystemRole = true,
                CreatedBy = "SYSTEM"
            };
            _context.Roles.Add(itAdminRole);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var permissions = new (string Code, string Module, string Action, string Description)[]
        {
            ("it.dashboard.view", "IT", "View", "View IT Dashboard"),
            ("it.users.view", "IT", "View", "View Users"),
            ("it.users.create", "IT", "Create", "Create Users"),
            ("it.users.edit", "IT", "Edit", "Edit Users"),
            ("it.users.delete", "IT", "Delete", "Delete Users"),
            ("it.roles.manage", "IT", "Manage", "Manage Roles and Permissions"),
            ("it.masters.view", "IT", "View", "View Masters"),
            ("it.masters.manage", "IT", "Manage", "Manage Masters and Dropdowns"),
            ("it.workflows.manage", "IT", "Manage", "Manage Workflows"),
            ("it.forms.manage", "IT", "Manage", "Manage Dynamic Forms"),
            ("it.security.manage", "IT", "Manage", "Manage Security Settings and Audits"),
            ("frontoffice.dashboard.view", "FrontOffice", "View", "View Front Office Dashboard"),
            ("frontoffice.checkin", "FrontOffice", "Create", "Perform Guest Check-In"),
            ("frontoffice.checkout", "FrontOffice", "Manage", "Perform Guest Checkout"),
            ("reservation.view", "Reservation", "View", "View Reservations"),
            ("reservation.create", "Reservation", "Create", "Create Reservations")
        };

        foreach (var p in permissions)
        {
            if (!await _context.Permissions.AnyAsync(x => x.Code == p.Code, cancellationToken))
            {
                var permEntity = new Permission
                {
                    Code = p.Code,
                    Module = p.Module,
                    Action = p.Action,
                    Description = p.Description,
                    CreatedBy = "SYSTEM"
                };
                _context.Permissions.Add(permEntity);
                await _context.SaveChangesAsync(cancellationToken);

                _context.RolePermissions.Add(new RolePermission { RoleId = superAdminRole.Id, PermissionId = permEntity.Id, GrantedBy = "SYSTEM" });
                _context.RolePermissions.Add(new RolePermission { RoleId = itAdminRole.Id, PermissionId = permEntity.Id, GrantedBy = "SYSTEM" });
            }
        }
        await _context.SaveChangesAsync(cancellationToken);

        var adminUser = await _context.Users
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u => u.NormalizedUsername == "ADMIN", cancellationToken);

        if (adminUser == null)
        {
            adminUser = new User
            {
                Username = "admin",
                NormalizedUsername = "ADMIN",
                Email = "admin@hotelos.internal",
                NormalizedEmail = "ADMIN@HOTELOS.INTERNAL",
                FirstName = "System",
                LastName = "Administrator",
                Status = AccountStatus.Active,
                EmailConfirmed = true,
                MustChangePassword = false,
                CreatedBy = "SYSTEM"
            };

            string passwordHash = _passwordService.HashPassword("Admin@HotelOS2026!");

            adminUser.Credential = new UserCredential
            {
                PasswordHash = passwordHash,
                SecurityStamp = Guid.NewGuid().ToString("N"),
                PasswordUpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync(cancellationToken);

            _context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = superAdminRole.Id, AssignedBy = "SYSTEM" });
            _context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = itAdminRole.Id, AssignedBy = "SYSTEM" });
            _context.UserPropertyAssignments.Add(new UserPropertyAssignment { UserId = adminUser.Id, PropertyId = 1, IsDefault = true, AssignedBy = "SYSTEM" });
            _context.PasswordHistories.Add(new PasswordHistory { UserId = adminUser.Id, PasswordHash = passwordHash });

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedMastersAsync(CancellationToken cancellationToken)
    {
        var categoryMap = new Dictionary<string, MasterCategory>();

        var categories = new (string Code, string Name, string Desc)[]
        {
            ("ORGANIZATION", "Organization & Personnel", "Organizational structures, departments, and employee roles"),
            ("ROOMS", "Rooms & Housekeeping", "Room classifications, statuses, amenities, and inventory settings"),
            ("RESERVATIONS", "Reservations & Bookings", "Booking channels, guest rates, and reservation lifecycles"),
            ("BILLING", "Finance & Billing", "Payment methods, tax slabs, and currency definitions"),
            ("OPERATIONS", "Operations & Maintenance", "Operational priorities, task statuses, and maintenance types")
        };

        foreach (var c in categories)
        {
            var cat = await _context.MasterCategories.FirstOrDefaultAsync(x => x.Code == c.Code, cancellationToken);
            if (cat == null)
            {
                cat = new MasterCategory { Code = c.Code, Name = c.Name, Description = c.Desc, IsSystem = true, CreatedBy = "SYSTEM" };
                _context.MasterCategories.Add(cat);
                await _context.SaveChangesAsync(cancellationToken);
            }
            categoryMap[c.Code] = cat;
        }

        var definitions = new (string Code, string Name, string CatCode, string Desc, (string Code, string Name, string Desc, int Order)[] Values)[]
        {
            ("DEPARTMENT", "Departments", "ORGANIZATION", "Hotel Operational Departments", new[]
            {
                ("FO", "Front Office", "Front desk, concierge, guest relations", 1),
                ("HK", "Housekeeping", "Room cleaning, laundry, public area hygiene", 2),
                ("FB", "Food & Beverage", "Restaurants, bars, banquets, and room service", 3),
                ("KIT", "Kitchen / Culinary", "Executive chefs, hot line, pantry, bakery", 4),
                ("ENG", "Engineering & Maintenance", "HVAC, electrical, plumbing, civil upkeep", 5),
                ("ACC", "Accounts & Finance", "Ledger, billing, revenue audit, night audit", 6),
                ("HR", "Human Resources", "Recruitment, payroll, training, employee relations", 7),
                ("IT", "Information Technology", "PMS, networks, security, dynamic configurations", 8),
                ("SEC", "Security & Surveillance", "Loss prevention, physical security, visitor access", 9),
                ("SPA", "Spa & Wellness", "Therapies, fitness centre, wellness packages", 10)
            }),

            ("ROOM_TYPE", "Room Types", "ROOMS", "Standard, Luxury and Suite categories", new[]
            {
                ("DLX", "Deluxe Room", "Spacious 35sqm room with city skyline views", 1),
                ("EXEC", "Executive King Suite", "50sqm suite with dedicated lounge access", 2),
                ("OCN", "Ocean View Villa", "Private plunge pool and panoramic coastline views", 3),
                ("PRES", "Presidential Grand Suite", "120sqm luxury multi-room suite with butler service", 4),
                ("FAM", "Family Penthouse", "Two-bedroom interconnected family suite", 5),
                ("STD", "Standard Double", "Comfortable compact room for transient stays", 6),
                ("TWIN", "Superior Twin", "Twin bed format for conference and tour groups", 7),
                ("ROYAL", "Royal Garden Chalet", "Private garden pavilion with jacuzzi", 8)
            }),

            ("ROOM_STATUS", "Room Statuses", "ROOMS", "Operational housekeeping states", new[]
            {
                ("CLN", "Clean & Ready", "Housekeeper cleaned and ready for guest allocation", 1),
                ("DRT", "Dirty / Vacant", "Guest departed, requires comprehensive turnover", 2),
                ("OCC", "Occupied", "Currently inhabited by registered guest", 3),
                ("INS", "Inspected & Approved", "Supervisor inspected and certified pristine", 4),
                ("OOD", "Out of Order", "Under major technical repair or renovation", 5),
                ("MNT", "Maintenance Required", "Minor maintenance scheduled", 6),
                ("DND", "Do Not Disturb", "Guest privacy requested, skip housekeeping", 7)
            }),

            ("RESERVATION_STATUS", "Reservation Statuses", "RESERVATIONS", "Booking lifecycle milestones", new[]
            {
                ("CONF", "Confirmed", "Guaranteed booking with deposit or card authorization", 1),
                ("PROV", "Provisional", "Temporary hold without financial guarantee", 2),
                ("CHKI", "Checked In", "Guest arrived, key card issued, folio opened", 3),
                ("CHKO", "Checked Out", "Settled folio, room vacated, key cards returned", 4),
                ("CANC", "Cancelled", "Booking cancelled according to policy", 5),
                ("NOSH", "No Show", "Guest did not arrive on scheduled check-in date", 6)
            }),

            ("PAYMENT_MODE", "Payment Modes", "BILLING", "Supported transaction settlement types", new[]
            {
                ("CC", "Credit Card", "Visa, Mastercard, Amex via EMV/NFC chip terminal", 1),
                ("CSH", "Cash Settlement", "Direct physical currency with cashier receipt", 2),
                ("CORP", "Direct Corporate Bill", "Invoiced against contracted corporate line", 3),
                ("UPI", "Digital Wallet / UPI", "Instant QR payment / Apple Pay / Google Pay", 4),
                ("WIRE", "Bank Wire Transfer", "Direct ACH/SWIFT advance bank settlement", 5)
            }),

            ("TASK_PRIORITY", "Task Priorities", "OPERATIONS", "Priority escalation levels", new[]
            {
                ("LOW", "Low Priority", "Routine task within 48 hours SLA", 1),
                ("MED", "Medium Priority", "Standard operations within 8 hours SLA", 2),
                ("HIGH", "High Priority", "Urgent operational request within 2 hours SLA", 3),
                ("CRIT", "Critical Escalation", "Immediate life-safety or VIP escalation (30 mins)", 4)
            }),

            ("BOOKING_SOURCE", "Booking Sources", "RESERVATIONS", "Reservation acquisition channels", new[]
            {
                ("DIRECT", "Direct Hotel Website", "Online engine reservation", 1),
                ("WALKIN", "Front Desk Walk-In", "Direct physical guest arrival", 2),
                ("CORP", "Corporate Contract", "Contracted company negotiated booking", 3),
                ("PHONE", "Telephone Reservation", "Direct concierge reservation", 4),
                ("AGENT", "Travel Agent (Consortium)", "Licensed IATA travel agency", 5),
                ("OTA", "Online Travel Agency (OTA)", "Global travel distributor channel", 6)
            })
        };

        foreach (var d in definitions)
        {
            var def = await _context.MasterDefinitions.FirstOrDefaultAsync(x => x.Code == d.Code, cancellationToken);
            if (def == null)
            {
                def = new MasterDefinition
                {
                    Code = d.Code,
                    Name = d.Name,
                    CategoryId = categoryMap[d.CatCode].Id,
                    Description = d.Desc,
                    CreatedBy = "SYSTEM"
                };
                _context.MasterDefinitions.Add(def);
                await _context.SaveChangesAsync(cancellationToken);
            }

            foreach (var v in d.Values)
            {
                if (!await _context.MasterValues.AnyAsync(x => x.MasterDefinitionId == def.Id && x.Code == v.Code, cancellationToken))
                {
                    _context.MasterValues.Add(new MasterValue
                    {
                        MasterDefinitionId = def.Id,
                        Code = v.Code,
                        Name = v.Name,
                        Description = v.Desc,
                        DisplayOrder = v.Order,
                        IsActive = true,
                        CreatedBy = "SYSTEM"
                    });
                }
            }
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedNavigationMenusAsync(CancellationToken cancellationToken)
    {
        var existingReports = await _context.NavigationMenus.FirstOrDefaultAsync(m => m.Code == "REPORTS", cancellationToken);
        if (existingReports == null && await _context.NavigationMenus.AnyAsync(cancellationToken))
        {
            var reportsMenu = new NavigationMenu
            {
                Code = "REPORTS",
                Title = "Reports & Analytics",
                Path = "/reports",
                Icon = "BarChart3",
                Module = "Reports",
                DisplayOrder = 15,
                IsActive = true,
                CreatedBy = "SYSTEM"
            };
            _context.NavigationMenus.Add(reportsMenu);
            await _context.SaveChangesAsync(cancellationToken);

            _context.NavigationMenus.AddRange(
                new NavigationMenu { ParentId = reportsMenu.Id, Code = "REP_CENTER", Title = "Reports Center", Path = "/reports/center", Module = "Reports", DisplayOrder = 1, IsActive = true, CreatedBy = "SYSTEM" },
                new NavigationMenu { ParentId = reportsMenu.Id, Code = "REP_NIGHT_AUDIT", Title = "Night Audit Ledger", Path = "/reports/night-audit", Module = "Reports", DisplayOrder = 2, IsActive = true, CreatedBy = "SYSTEM" },
                new NavigationMenu { ParentId = reportsMenu.Id, Code = "REP_OCCUPANCY", Title = "Occupancy Forecast", Path = "/reports/forecast", Module = "Reports", DisplayOrder = 3, IsActive = true, CreatedBy = "SYSTEM" },
                new NavigationMenu { ParentId = reportsMenu.Id, Code = "REP_TRIAL_BAL", Title = "Revenue Trial Balance", Path = "/reports/trial-balance", Module = "Reports", DisplayOrder = 4, IsActive = true, CreatedBy = "SYSTEM" }
            );
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        if (await _context.NavigationMenus.AnyAsync(cancellationToken))
        {
            var itParent = await _context.NavigationMenus.FirstOrDefaultAsync(m => m.Code == "IT" && m.ParentId == null, cancellationToken);
            if (itParent != null && !await _context.NavigationMenus.AnyAsync(m => m.Code == "IT_NUMBERING", cancellationToken))
            {
                _context.NavigationMenus.Add(new NavigationMenu
                {
                    ParentId = itParent.Id,
                    Code = "IT_NUMBERING",
                    Title = "Document Numbering",
                    Path = "/it/numbering",
                    Module = "IT",
                    DisplayOrder = 12,
                    RequiredPermission = "it.configuration.manage",
                    CreatedBy = "SYSTEM"
                });
                await _context.SaveChangesAsync(cancellationToken);
            }
            return;
        }

        var menus = new MenuSeedItem[]
        {
            new("DASHBOARD", "Command Center", "/dashboard", "LayoutDashboard", "Platform", 1, null, Array.Empty<SubmenuSeedItem>()),

            new("FRONTOFFICE", "Front Office", "/front-office", "Bed", "FrontOffice", 2, "frontoffice.dashboard.view", new SubmenuSeedItem[]
            {
                new("FO_ARRIVALS", "Arrivals & Check-In", "/front-office/arrivals", 1, "frontoffice.checkin"),
                new("FO_DEPARTURES", "Departures & Checkout", "/front-office/departures", 2, "frontoffice.checkout"),
                new("FO_ROOMS", "Room Allocation Grid", "/front-office/room-grid", 3, "frontoffice.dashboard.view"),
                new("FO_FOLIO", "Guest Folios & Billing", "/front-office/folios", 4, "frontoffice.checkout")
            }),

            new("RESERVATIONS", "Reservations", "/reservations", "CalendarCheck", "Reservation", 3, "reservation.view", new SubmenuSeedItem[]
            {
                new("RES_ALL", "All Reservations", "/reservations/list", 1, "reservation.view"),
                new("RES_NEW", "Create New Booking", "/reservations/new", 2, "reservation.create"),
                new("RES_CAL", "Availability Matrix", "/reservations/matrix", 3, "reservation.view")
            }),

            new("GUESTS", "Guest Profiles & CRM", "/guests", "Users", "Guest", 4, null, new SubmenuSeedItem[]
            {
                new("GST_ALL", "Guest Directory", "/guests/list", 1),
                new("GST_VIP", "VIP & Loyalty Members", "/guests/loyalty", 2)
            }),

            new("HOUSEKEEPING", "Housekeeping", "/housekeeping", "Sparkles", "Housekeeping", 5, null, new SubmenuSeedItem[]
            {
                new("HK_BOARD", "Room Status Board", "/housekeeping/board", 1),
                new("HK_TASKS", "Task Assignments", "/housekeeping/tasks", 2),
                new("HK_LINEN", "Linen & Laundry Log", "/housekeeping/linen", 3)
            }),

            new("RESTAURANT", "Restaurant & POS", "/restaurant", "UtensilsCrossed", "Restaurant", 6, null, new SubmenuSeedItem[]
            {
                new("POS_ORDER", "Live POS Terminal", "/restaurant/pos", 1),
                new("POS_TABLES", "Table Management", "/restaurant/tables", 2),
                new("POS_ITEMS", "Menu Items & Pricing", "/restaurant/items", 3)
            }),

            new("KITCHEN", "Kitchen (KDS)", "/kitchen", "Flame", "Kitchen", 7, null, new SubmenuSeedItem[]
            {
                new("KDS_ACTIVE", "Active Order Queue", "/kitchen/queue", 1)
            }),

            new("BANQUET", "Banquets & Events", "/banquet", "Wine", "Banquet", 8, null, new SubmenuSeedItem[]
            {
                new("BNQ_EVENTS", "Event Bookings", "/banquet/events", 1),
                new("BNQ_HALLS", "Halls & Function Spaces", "/banquet/spaces", 2)
            }),

            new("PURCHASE", "Purchase & Vendor", "/purchase", "ShoppingBag", "Purchase", 9, null, new SubmenuSeedItem[]
            {
                new("PUR_REQ", "Purchase Requisitions", "/purchase/requisitions", 1),
                new("PUR_ORDERS", "Purchase Orders", "/purchase/orders", 2),
                new("PUR_VENDORS", "Vendor Directory", "/purchase/vendors", 3)
            }),

            new("INVENTORY", "Inventory & Stores", "/inventory", "Boxes", "Inventory", 10, null, new SubmenuSeedItem[]
            {
                new("INV_ITEMS", "Stock Items Master", "/inventory/items", 1),
                new("INV_STORES", "Central Stores Ledger", "/inventory/stores", 2)
            }),

            new("FINANCE", "Finance & Accounts", "/finance", "Receipt", "Finance", 11, null, new SubmenuSeedItem[]
            {
                new("FIN_LEDGER", "General Ledger", "/finance/ledger", 1),
                new("FIN_INVOICES", "Invoices & Settlement", "/finance/invoices", 2)
            }),

            new("HR", "Human Resources", "/hr", "Briefcase", "HR", 12, null, new SubmenuSeedItem[]
            {
                new("HR_STAFF", "Staff Directory", "/hr/staff", 1),
                new("HR_ATTEND", "Attendance & Shifts", "/hr/attendance", 2)
            }),

            new("MAINTENANCE", "Engineering & Maintenance", "/maintenance", "Wrench", "Maintenance", 13, null, new SubmenuSeedItem[]
            {
                new("MNT_ORDERS", "Work Orders", "/maintenance/orders", 1),
                new("MNT_ASSETS", "Asset Register", "/maintenance/assets", 2)
            }),

            new("SECURITY", "Security & Visitors", "/security", "ShieldCheck", "Security", 14, null, new SubmenuSeedItem[]
            {
                new("SEC_LOG", "Visitor Access Log", "/security/visitors", 1),
                new("SEC_INCIDENTS", "Incident Reports", "/security/incidents", 2)
            }),

            new("REPORTS", "Reports & Analytics", "/reports", "BarChart3", "Reports", 15, null, new SubmenuSeedItem[]
            {
                new("REP_CENTER", "Reports Center", "/reports/center", 1),
                new("REP_NIGHT_AUDIT", "Night Audit Ledger", "/reports/night-audit", 2),
                new("REP_OCCUPANCY", "Occupancy Forecast", "/reports/forecast", 3),
                new("REP_TRIAL_BAL", "Revenue Trial Balance", "/reports/trial-balance", 4)
            }),

            new("IT", "IT Department", "/it", "Cpu", "IT", 16, "it.dashboard.view", new SubmenuSeedItem[]
            {
                new("IT_DASHBOARD", "IT Dashboard", "/it/dashboard", 1, "it.dashboard.view"),
                new("IT_USERS", "User Management", "/it/users", 2, "it.users.view"),
                new("IT_ROLES", "Role & Permissions", "/it/roles", 3, "it.roles.manage"),
                new("IT_MASTERS", "Master Management", "/it/masters", 4, "it.masters.manage"),
                new("IT_DROPDOWNS", "Dynamic Dropdowns", "/it/dropdowns", 5, "it.masters.manage"),
                new("IT_MENUS", "Dynamic Menu Config", "/it/menus", 6, "it.configuration.manage"),
                new("IT_WORKFLOWS", "Workflow Management", "/it/workflows", 7, "it.workflows.manage"),
                new("IT_FORMS", "Dynamic Forms Engine", "/it/forms", 8, "it.forms.manage"),
                new("IT_SECURITY", "Security Settings", "/it/security", 9, "it.security.manage"),
                new("IT_AUDIT", "Audit Logs", "/it/audit", 10, "it.security.manage"),
                new("IT_HEALTH", "System Health", "/it/health", 11, "it.dashboard.view"),
                new("IT_NUMBERING", "Document Numbering", "/it/numbering", 12, "it.configuration.manage")
            })
        };

        foreach (var m in menus)
        {
            var parentMenu = new NavigationMenu
            {
                Code = m.Code,
                Title = m.Title,
                Path = m.Path,
                Icon = m.Icon,
                Module = m.Module,
                DisplayOrder = m.Order,
                RequiredPermission = m.ReqPerm,
                CreatedBy = "SYSTEM"
            };

            _context.NavigationMenus.Add(parentMenu);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var sub in m.Submenus)
            {
                _context.NavigationMenus.Add(new NavigationMenu
                {
                    ParentId = parentMenu.Id,
                    Code = sub.Code,
                    Title = sub.Title,
                    Path = sub.Path,
                    Module = m.Module,
                    DisplayOrder = sub.Order,
                    RequiredPermission = sub.ReqPerm,
                    CreatedBy = "SYSTEM"
                });
            }
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedDynamicEnginesAsync(CancellationToken cancellationToken)
    {
        // 1. Dynamic Form: Reservation Form
        if (!await _context.DynamicFormDefinitions.AnyAsync(f => f.Code == "RESERVATION_FORM", cancellationToken))
        {
            var resForm = new DynamicFormDefinition
            {
                Code = "RESERVATION_FORM",
                Title = "Guest Room Reservation",
                Description = "Dynamic form for registering and modifying guest room bookings",
                Module = "Reservation",
                EntityType = "Reservation",
                Version = 1,
                IsPublished = true,
                CreatedBy = "SYSTEM"
            };
            _context.DynamicFormDefinitions.Add(resForm);
            await _context.SaveChangesAsync(cancellationToken);

            var fields = new (string Key, string Label, string Type, bool Mandatory, string? MasterDef, int Order)[]
            {
                ("GuestName", "Primary Guest Full Name", "text", true, null, 1),
                ("GuestEmail", "Contact Work / Personal Email", "text", true, null, 2),
                ("GuestPhone", "Mobile Phone Number", "text", true, null, 3),
                ("RoomType", "Room Classification", "select", true, "ROOM_TYPE", 4),
                ("CheckInDate", "Check-In Arrival Date", "date", true, null, 5),
                ("CheckOutDate", "Check-Out Departure Date", "date", true, null, 6),
                ("BookingSource", "Channel / Booking Source", "select", true, "BOOKING_SOURCE", 7),
                ("AdultsCount", "Number of Adults", "number", true, null, 8),
                ("ChildrenCount", "Number of Children", "number", false, null, 9),
                ("SpecialRequests", "Guest Preferences & Remarks", "textarea", false, null, 10)
            };

            foreach (var f in fields)
            {
                _context.DynamicFormFields.Add(new DynamicFormField
                {
                    FormDefinitionId = resForm.Id,
                    FieldKey = f.Key,
                    FieldLabel = f.Label,
                    FieldType = f.Type,
                    IsMandatory = f.Mandatory,
                    MasterDefinitionCode = f.MasterDef,
                    DisplayOrder = f.Order,
                    CreatedBy = "SYSTEM"
                });
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 2. Dynamic Workflow: Purchase Requisition Workflow
        if (!await _context.WorkflowDefinitions.AnyAsync(w => w.Code == "PURCHASE_REQUISITION", cancellationToken))
        {
            var poWorkflow = new WorkflowDefinition
            {
                Code = "PURCHASE_REQUISITION",
                Title = "Departmental Purchase Requisition Approval",
                Module = "Purchase",
                Version = 1,
                IsPublished = true,
                Description = "Sequential multi-level approval for goods, equipment and procurement requisitions",
                CreatedBy = "SYSTEM"
            };
            _context.WorkflowDefinitions.Add(poWorkflow);
            await _context.SaveChangesAsync(cancellationToken);

            var step1 = new WorkflowStep { WorkflowDefinitionId = poWorkflow.Id, StepOrder = 1, StepName = "Draft Submitted", StepType = "Start", CreatedBy = "SYSTEM" };
            var step2 = new WorkflowStep { WorkflowDefinitionId = poWorkflow.Id, StepOrder = 2, StepName = "Department Manager Approval", StepType = "Approval", AssignedRole = "DepartmentManager", SlaMinutes = 720, CreatedBy = "SYSTEM" };
            var step3 = new WorkflowStep { WorkflowDefinitionId = poWorkflow.Id, StepOrder = 3, StepName = "Purchase Manager Review", StepType = "Approval", AssignedRole = "PurchaseManager", SlaMinutes = 1440, CreatedBy = "SYSTEM" };
            var step4 = new WorkflowStep { WorkflowDefinitionId = poWorkflow.Id, StepOrder = 4, StepName = "Finance General Manager Approval", StepType = "Approval", AssignedRole = "FinanceManager", SlaMinutes = 2880, CreatedBy = "SYSTEM" };
            var step5 = new WorkflowStep { WorkflowDefinitionId = poWorkflow.Id, StepOrder = 5, StepName = "Purchase Order Dispatched", StepType = "End", CreatedBy = "SYSTEM" };

            _context.WorkflowSteps.AddRange(step1, step2, step3, step4, step5);
            await _context.SaveChangesAsync(cancellationToken);

            _context.WorkflowTransitions.AddRange(
                new WorkflowTransition { WorkflowDefinitionId = poWorkflow.Id, FromStepId = step1.Id, ToStepId = step2.Id, ActionName = "Submit", CreatedBy = "SYSTEM" },
                new WorkflowTransition { WorkflowDefinitionId = poWorkflow.Id, FromStepId = step2.Id, ToStepId = step3.Id, ActionName = "Approve", CreatedBy = "SYSTEM" },
                new WorkflowTransition { WorkflowDefinitionId = poWorkflow.Id, FromStepId = step2.Id, ToStepId = step1.Id, ActionName = "Reject", CreatedBy = "SYSTEM" },
                new WorkflowTransition { WorkflowDefinitionId = poWorkflow.Id, FromStepId = step3.Id, ToStepId = step4.Id, ActionName = "Approve", CreatedBy = "SYSTEM" },
                new WorkflowTransition { WorkflowDefinitionId = poWorkflow.Id, FromStepId = step3.Id, ToStepId = step2.Id, ActionName = "Reject", CreatedBy = "SYSTEM" },
                new WorkflowTransition { WorkflowDefinitionId = poWorkflow.Id, FromStepId = step4.Id, ToStepId = step5.Id, ActionName = "Approve", CreatedBy = "SYSTEM" },
                new WorkflowTransition { WorkflowDefinitionId = poWorkflow.Id, FromStepId = step4.Id, ToStepId = step3.Id, ActionName = "Reject", CreatedBy = "SYSTEM" }
            );
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedPmsAsync(CancellationToken cancellationToken)
    {
        if (await _context.Rooms.AnyAsync(cancellationToken))
            return;

        // 1. Seed Rooms across 3 floors
        var rooms = new Room[]
        {
            // Floor 1
            new() { PropertyId = 1, RoomNumber = "101", FloorNumber = 1, RoomTypeCode = "DLX", RoomStatusCode = "CLN", MaxOccupancy = 2, BaseRatePerNight = 180m, CreatedBy = "SYSTEM" },
            new() { PropertyId = 1, RoomNumber = "102", FloorNumber = 1, RoomTypeCode = "DLX", RoomStatusCode = "CLN", MaxOccupancy = 2, BaseRatePerNight = 180m, CreatedBy = "SYSTEM" },
            new() { PropertyId = 1, RoomNumber = "103", FloorNumber = 1, RoomTypeCode = "DLX", RoomStatusCode = "DRT", MaxOccupancy = 2, BaseRatePerNight = 180m, CreatedBy = "SYSTEM" },
            new() { PropertyId = 1, RoomNumber = "104", FloorNumber = 1, RoomTypeCode = "STD", RoomStatusCode = "OCC", MaxOccupancy = 2, BaseRatePerNight = 120m, KeyCardUid = "NFC-104-981", CreatedBy = "SYSTEM" },
            new() { PropertyId = 1, RoomNumber = "105", FloorNumber = 1, RoomTypeCode = "STD", RoomStatusCode = "INS", MaxOccupancy = 2, BaseRatePerNight = 120m, CreatedBy = "SYSTEM" },
            new() { PropertyId = 1, RoomNumber = "106", FloorNumber = 1, RoomTypeCode = "STD", RoomStatusCode = "DND", MaxOccupancy = 2, BaseRatePerNight = 120m, CreatedBy = "SYSTEM" },

            // Floor 2
            new() { PropertyId = 1, RoomNumber = "201", FloorNumber = 2, RoomTypeCode = "EXEC", RoomStatusCode = "CLN", MaxOccupancy = 3, BaseRatePerNight = 280m, CreatedBy = "SYSTEM" },
            new() { PropertyId = 1, RoomNumber = "202", FloorNumber = 2, RoomTypeCode = "EXEC", RoomStatusCode = "OCC", MaxOccupancy = 3, BaseRatePerNight = 280m, KeyCardUid = "NFC-202-451", CreatedBy = "SYSTEM" },
            new() { PropertyId = 1, RoomNumber = "203", FloorNumber = 2, RoomTypeCode = "EXEC", RoomStatusCode = "MNT", MaxOccupancy = 3, BaseRatePerNight = 280m, Notes = "AC unit filter servicing", CreatedBy = "SYSTEM" },
            new() { PropertyId = 1, RoomNumber = "204", FloorNumber = 2, RoomTypeCode = "TWIN", RoomStatusCode = "CLN", MaxOccupancy = 2, BaseRatePerNight = 150m, CreatedBy = "SYSTEM" },
            new() { PropertyId = 1, RoomNumber = "205", FloorNumber = 2, RoomTypeCode = "TWIN", RoomStatusCode = "DRT", MaxOccupancy = 2, BaseRatePerNight = 150m, CreatedBy = "SYSTEM" },

            // Floor 3
            new() { PropertyId = 1, RoomNumber = "301", FloorNumber = 3, RoomTypeCode = "OCN", RoomStatusCode = "CLN", MaxOccupancy = 3, BaseRatePerNight = 450m, CreatedBy = "SYSTEM" },
            new() { PropertyId = 1, RoomNumber = "302", FloorNumber = 3, RoomTypeCode = "OCN", RoomStatusCode = "OCC", MaxOccupancy = 3, BaseRatePerNight = 450m, KeyCardUid = "NFC-302-772", CreatedBy = "SYSTEM" },
            new() { PropertyId = 1, RoomNumber = "303", FloorNumber = 3, RoomTypeCode = "PRES", RoomStatusCode = "INS", MaxOccupancy = 4, BaseRatePerNight = 850m, CreatedBy = "SYSTEM" },
            new() { PropertyId = 1, RoomNumber = "304", FloorNumber = 3, RoomTypeCode = "FAM", RoomStatusCode = "CLN", MaxOccupancy = 5, BaseRatePerNight = 350m, CreatedBy = "SYSTEM" },
            new() { PropertyId = 1, RoomNumber = "305", FloorNumber = 3, RoomTypeCode = "ROYAL", RoomStatusCode = "OOD", MaxOccupancy = 4, BaseRatePerNight = 950m, Notes = "Bathroom marble polishing", CreatedBy = "SYSTEM" },
        };

        _context.Rooms.AddRange(rooms);
        await _context.SaveChangesAsync(cancellationToken);

        // 2. Seed Sample Guests
        var guest1 = new Guest
        {
            PropertyId = 1,
            FirstName = "Alexander",
            LastName = "Wright",
            Email = "alexander.wright@embassy.gov.uk",
            Phone = "+44 7911 123456",
            IdDocumentType = "Passport",
            IdDocumentNumber = "GB98213401",
            Nationality = "British",
            VipStatus = "Platinum",
            TotalStays = 6,
            LifetimeSpend = 14200m,
            SpecialPreferences = "High floor, extra feather pillows, sparkling water on arrival",
            City = "London",
            Country = "United Kingdom",
            CreatedBy = "SYSTEM"
        };

        var guest2 = new Guest
        {
            PropertyId = 1,
            FirstName = "Elena",
            LastName = "Rostova",
            Email = "elena.rostova@techcorp.de",
            Phone = "+49 151 2345678",
            IdDocumentType = "Passport",
            IdDocumentNumber = "D8830192A",
            Nationality = "German",
            VipStatus = "Gold",
            TotalStays = 4,
            LifetimeSpend = 6800m,
            SpecialPreferences = "Quiet room away from elevators, soy milk in minibar",
            City = "Munich",
            Country = "Germany",
            CreatedBy = "SYSTEM"
        };

        var guest3 = new Guest
        {
            PropertyId = 1,
            FirstName = "Michael",
            LastName = "Chang",
            Email = "mchang@pacificventure.com",
            Phone = "+1 415 555 0192",
            IdDocumentType = "DriverLicense",
            IdDocumentNumber = "CA-D991048",
            Nationality = "American",
            VipStatus = "Silver",
            TotalStays = 2,
            LifetimeSpend = 2400m,
            SpecialPreferences = "Late checkout requested when possible",
            City = "San Francisco",
            Country = "United States",
            CreatedBy = "SYSTEM"
        };

        var guest4 = new Guest
        {
            PropertyId = 1,
            FirstName = "Sarah",
            LastName = "Jenkins",
            Email = "sarah.jenkins@consulting.ca",
            Phone = "+1 604 555 7821",
            IdDocumentType = "Passport",
            IdDocumentNumber = "CAN-7718290",
            Nationality = "Canadian",
            VipStatus = "Standard",
            TotalStays = 1,
            LifetimeSpend = 950m,
            SpecialPreferences = "Non-smoking room guaranteed",
            City = "Vancouver",
            Country = "Canada",
            CreatedBy = "SYSTEM"
        };

        _context.Guests.AddRange(guest1, guest2, guest3, guest4);
        await _context.SaveChangesAsync(cancellationToken);

        // 3. Seed Sample Reservations & Folios
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Reservation 1: Arriving today (Ready for Check-In)
        var res1 = new Reservation
        {
            PropertyId = 1,
            ConfirmationNumber = "RES-2026-00101",
            GuestId = guest1.Id,
            RoomTypeCode = "DLX",
            CheckInDate = today,
            CheckOutDate = today.AddDays(3),
            AdultsCount = 2,
            ChildrenCount = 0,
            ReservationStatus = "CONF",
            BookingSource = "DIRECT",
            RatePerNight = 180m,
            TotalEstimatedAmount = 540m,
            PaidAmount = 180m,
            SpecialRequests = "Anniversary setup with fruit basket",
            CreatedBy = "SYSTEM"
        };

        // Reservation 2: Checked In (In-house in Room 104)
        var room104 = rooms.First(r => r.RoomNumber == "104");
        var res2 = new Reservation
        {
            PropertyId = 1,
            ConfirmationNumber = "RES-2026-00088",
            GuestId = guest2.Id,
            RoomTypeCode = "STD",
            AssignedRoomId = room104.Id,
            CheckInDate = today.AddDays(-1),
            CheckOutDate = today.AddDays(2),
            AdultsCount = 1,
            ChildrenCount = 0,
            ReservationStatus = "CHKI",
            BookingSource = "CORP",
            RatePerNight = 120m,
            TotalEstimatedAmount = 360m,
            PaidAmount = 120m,
            ActualCheckInAt = DateTimeOffset.UtcNow.AddHours(-20),
            CheckedInBy = "admin",
            CreatedBy = "SYSTEM"
        };

        // Reservation 3: Checked Out (Departed today from Room 103)
        var room103 = rooms.First(r => r.RoomNumber == "103");
        var res3 = new Reservation
        {
            PropertyId = 1,
            ConfirmationNumber = "RES-2026-00045",
            GuestId = guest3.Id,
            RoomTypeCode = "DLX",
            AssignedRoomId = room103.Id,
            CheckInDate = today.AddDays(-2),
            CheckOutDate = today,
            AdultsCount = 1,
            ChildrenCount = 0,
            ReservationStatus = "CHKO",
            BookingSource = "OTA",
            RatePerNight = 180m,
            TotalEstimatedAmount = 360m,
            PaidAmount = 360m,
            ActualCheckInAt = DateTimeOffset.UtcNow.AddDays(-2),
            ActualCheckOutAt = DateTimeOffset.UtcNow.AddHours(-2),
            CheckedInBy = "admin",
            CheckedOutBy = "admin",
            CreatedBy = "SYSTEM"
        };

        _context.Reservations.AddRange(res1, res2, res3);
        await _context.SaveChangesAsync(cancellationToken);

        // Folio for Reservation 2 (Active in-house)
        var folio2 = new Folio
        {
            PropertyId = 1,
            ReservationId = res2.Id,
            GuestId = guest2.Id,
            FolioNumber = "FOL-2026-00088",
            Status = "Open",
            TotalCharges = 410m,
            TotalPayments = 120m,
            BalanceAmount = 290m,
            CreatedBy = "SYSTEM"
        };
        _context.Folios.Add(folio2);
        await _context.SaveChangesAsync(cancellationToken);

        _context.FolioItems.AddRange(
            new FolioItem { FolioId = folio2.Id, ItemType = "CHARGE", DepartmentCode = "FO", Description = "Room Accommodation (104) - 3 nights @ $120.00", UnitPrice = 120m, Quantity = 3, Amount = 360m, PostedBy = "admin", PostedAt = DateTimeOffset.UtcNow.AddHours(-20) },
            new FolioItem { FolioId = folio2.Id, ItemType = "CHARGE", DepartmentCode = "FB", Description = "Room Service - Executive Breakfast & Espresso", UnitPrice = 50m, Quantity = 1, Amount = 50m, PostedBy = "POS", PostedAt = DateTimeOffset.UtcNow.AddHours(-3) },
            new FolioItem { FolioId = folio2.Id, ItemType = "PAYMENT", DepartmentCode = "FO", Description = "Advance Deposit via Corporate Credit Card", UnitPrice = 120m, Quantity = 1, Amount = 120m, PaymentModeCode = "CC", TransactionReference = "TXN-99881", PostedBy = "admin", PostedAt = DateTimeOffset.UtcNow.AddHours(-20) }
        );
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedOperationsAsync(CancellationToken cancellationToken)
    {
        // 1. Linen Inventory
        if (!await _context.LinenItems.AnyAsync(cancellationToken))
        {
            _context.LinenItems.AddRange(
                new LinenItem { PropertyId = 1, ItemCode = "LIN-SHT-K", ItemName = "King Fitted Pure Cotton Sheet", Category = "BEDSHEET", ParLevel = 250, CleanStock = 180, InUseStock = 45, InLaundryStock = 20, DiscardedStock = 5, CreatedBy = "SYSTEM" },
                new LinenItem { PropertyId = 1, ItemCode = "LIN-SHT-Q", ItemName = "Queen Fitted Pure Cotton Sheet", Category = "BEDSHEET", ParLevel = 200, CleanStock = 140, InUseStock = 40, InLaundryStock = 15, DiscardedStock = 5, CreatedBy = "SYSTEM" },
                new LinenItem { PropertyId = 1, ItemCode = "LIN-PLW", ItemName = "Hypoallergenic Pillow Case (500TC)", Category = "PILLOW_CASE", ParLevel = 400, CleanStock = 290, InUseStock = 80, InLaundryStock = 25, DiscardedStock = 5, CreatedBy = "SYSTEM" },
                new LinenItem { PropertyId = 1, ItemCode = "LIN-DVT", ItemName = "Plush Down Alternative Duvet Cover", Category = "DUVET_COVER", ParLevel = 180, CleanStock = 120, InUseStock = 42, InLaundryStock = 14, DiscardedStock = 4, CreatedBy = "SYSTEM" },
                new LinenItem { PropertyId = 1, ItemCode = "LIN-TWL-B", ItemName = "Egyptian Cotton Bath Towel (700 GSM)", Category = "BATH_TOWEL", ParLevel = 500, CleanStock = 350, InUseStock = 110, InLaundryStock = 35, DiscardedStock = 5, CreatedBy = "SYSTEM" },
                new LinenItem { PropertyId = 1, ItemCode = "LIN-TWL-H", ItemName = "Turkish Cotton Hand Towel", Category = "HAND_TOWEL", ParLevel = 450, CleanStock = 320, InUseStock = 95, InLaundryStock = 30, DiscardedStock = 5, CreatedBy = "SYSTEM" }
            );
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 2. Housekeeping Tasks
        if (!await _context.HousekeepingTasks.AnyAsync(cancellationToken))
        {
            var dirtyRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomStatusCode == "DRT", cancellationToken)
                ?? await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == "103", cancellationToken);

            var cleanRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomStatusCode == "CLN", cancellationToken)
                ?? await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == "102", cancellationToken);

            var occRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomStatusCode == "OCC", cancellationToken)
                ?? await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == "101", cancellationToken);

            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin", cancellationToken);

            if (dirtyRoom != null)
            {
                _context.HousekeepingTasks.Add(new HousekeepingTask
                {
                    PropertyId = 1,
                    TaskNumber = "HK-20261119-0001",
                    RoomId = dirtyRoom.Id,
                    TaskType = "TURNOVER",
                    Priority = "HIGH",
                    Status = "PENDING",
                    AssignedToUserId = adminUser?.Id,
                    AssignedAt = DateTimeOffset.UtcNow.AddMinutes(-30),
                    Remarks = "Guest departed, complete deep linen turnover and restock minibar",
                    CreatedBy = "SYSTEM"
                });
            }

            if (occRoom != null)
            {
                _context.HousekeepingTasks.Add(new HousekeepingTask
                {
                    PropertyId = 1,
                    TaskNumber = "HK-20261119-0002",
                    RoomId = occRoom.Id,
                    TaskType = "DAILY_CLEAN",
                    Priority = "MEDIUM",
                    Status = "IN_PROGRESS",
                    AssignedToUserId = adminUser?.Id,
                    AssignedAt = DateTimeOffset.UtcNow.AddHours(-1),
                    StartedAt = DateTimeOffset.UtcNow.AddMinutes(-15),
                    Remarks = "VIP in-house guest stayover service, extra towels requested",
                    CreatedBy = "SYSTEM"
                });
            }

            if (cleanRoom != null)
            {
                _context.HousekeepingTasks.Add(new HousekeepingTask
                {
                    PropertyId = 1,
                    TaskNumber = "HK-20261119-0003",
                    RoomId = cleanRoom.Id,
                    TaskType = "INSPECTION",
                    Priority = "MEDIUM",
                    Status = "COMPLETED",
                    AssignedToUserId = adminUser?.Id,
                    StartedAt = DateTimeOffset.UtcNow.AddHours(-2),
                    CompletedAt = DateTimeOffset.UtcNow.AddHours(-1),
                    Remarks = "Cleaned by attendant, waiting supervisor sign-off",
                    CreatedBy = "SYSTEM"
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        // 3. Dining Tables
        if (!await _context.DiningTables.AnyAsync(cancellationToken))
        {
            _context.DiningTables.AddRange(
                new DiningTable { PropertyId = 1, TableNumber = "T-01", Section = "MAIN_DINING", Capacity = 2, Status = "AVAILABLE", DisplayOrder = 1, CreatedBy = "SYSTEM" },
                new DiningTable { PropertyId = 1, TableNumber = "T-02", Section = "MAIN_DINING", Capacity = 4, Status = "OCCUPIED", DisplayOrder = 2, CreatedBy = "SYSTEM" },
                new DiningTable { PropertyId = 1, TableNumber = "T-03", Section = "MAIN_DINING", Capacity = 4, Status = "AVAILABLE", DisplayOrder = 3, CreatedBy = "SYSTEM" },
                new DiningTable { PropertyId = 1, TableNumber = "T-04", Section = "MAIN_DINING", Capacity = 6, Status = "RESERVED", DisplayOrder = 4, CreatedBy = "SYSTEM" },
                new DiningTable { PropertyId = 1, TableNumber = "TERR-01", Section = "TERRACE", Capacity = 4, Status = "AVAILABLE", DisplayOrder = 5, CreatedBy = "SYSTEM" },
                new DiningTable { PropertyId = 1, TableNumber = "TERR-02", Section = "TERRACE", Capacity = 2, Status = "AVAILABLE", DisplayOrder = 6, CreatedBy = "SYSTEM" },
                new DiningTable { PropertyId = 1, TableNumber = "ROOF-01", Section = "ROOFTOP", Capacity = 4, Status = "OCCUPIED", DisplayOrder = 7, CreatedBy = "SYSTEM" },
                new DiningTable { PropertyId = 1, TableNumber = "ROOF-02", Section = "ROOFTOP", Capacity = 6, Status = "AVAILABLE", DisplayOrder = 8, CreatedBy = "SYSTEM" },
                new DiningTable { PropertyId = 1, TableNumber = "BAR-01", Section = "BAR", Capacity = 2, Status = "AVAILABLE", DisplayOrder = 9, CreatedBy = "SYSTEM" },
                new DiningTable { PropertyId = 1, TableNumber = "PVT-01", Section = "PRIVATE_DINING", Capacity = 12, Status = "AVAILABLE", DisplayOrder = 10, CreatedBy = "SYSTEM" }
            );
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 4. Menu Items
        if (!await _context.MenuItems.AnyAsync(cancellationToken))
        {
            _context.MenuItems.AddRange(
                new MenuItem { PropertyId = 1, ItemCode = "MNU-APP-01", ItemName = "Crispy Calamari Fritti", Category = "APPETIZERS", Description = "Tender calamari with saffron garlic aioli and charred lemon", Price = 18.50m, PreparationTimeMinutes = 12, DietaryType = "NON_VEG", IsAvailable = true, CreatedBy = "SYSTEM" },
                new MenuItem { PropertyId = 1, ItemCode = "MNU-APP-02", ItemName = "Heirloom Tomato & Burrata", Category = "APPETIZERS", Description = "Artisanal pugliese burrata, aged balsamic, micro basil, and focaccia crisps", Price = 16.00m, PreparationTimeMinutes = 10, DietaryType = "VEG", IsAvailable = true, CreatedBy = "SYSTEM" },
                new MenuItem { PropertyId = 1, ItemCode = "MNU-MAIN-01", ItemName = "Pan-Seared Atlantic Halibut", Category = "MAINS", Description = "Wild caught halibut with butter-poached leeks and white wine reduction", Price = 38.00m, PreparationTimeMinutes = 20, DietaryType = "NON_VEG", IsAvailable = true, CreatedBy = "SYSTEM" },
                new MenuItem { PropertyId = 1, ItemCode = "MNU-MAIN-02", ItemName = "Prime Australian Wagyu Burger", Category = "MAINS", Description = "Brioche bun, truffled cheddar, caramelized shallots, and hand-cut fries", Price = 28.50m, PreparationTimeMinutes = 15, DietaryType = "NON_VEG", IsAvailable = true, CreatedBy = "SYSTEM" },
                new MenuItem { PropertyId = 1, ItemCode = "MNU-MAIN-03", ItemName = "Truffle & Wild Mushroom Risotto", Category = "MAINS", Description = "Carnaroli rice, foraged forest mushrooms, black truffle butter, and 24-month parmesan", Price = 26.00m, PreparationTimeMinutes = 18, DietaryType = "VEG", IsAvailable = true, CreatedBy = "SYSTEM" },
                new MenuItem { PropertyId = 1, ItemCode = "MNU-PIZ-01", ItemName = "Artisanal Margherita Bufala", Category = "BREADS", Description = "San Marzano tomatoes, mozzarella di bufala, fresh basil, and extra virgin olive oil", Price = 22.00m, PreparationTimeMinutes = 14, DietaryType = "VEG", IsAvailable = true, CreatedBy = "SYSTEM" },
                new MenuItem { PropertyId = 1, ItemCode = "MNU-DES-01", ItemName = "Classic Espresso Tiramisu", Category = "DESSERTS", Description = "Savoiardi ladyfingers, Illy espresso liqueur, and mascarpone mousse", Price = 12.50m, PreparationTimeMinutes = 8, DietaryType = "VEG", IsAvailable = true, CreatedBy = "SYSTEM" },
                new MenuItem { PropertyId = 1, ItemCode = "MNU-DRK-01", ItemName = "Smoked Rosemary Old Fashioned", Category = "COCKTAILS", Description = "Woodford Reserve bourbon, bitters, torch-smoked organic rosemary sprig", Price = 19.00m, PreparationTimeMinutes = 6, DietaryType = "VEGAN", IsAvailable = true, CreatedBy = "SYSTEM" }
            );
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 5. Active Restaurant & KDS Orders
        if (!await _context.RestaurantOrders.AnyAsync(cancellationToken))
        {
            var table2 = await _context.DiningTables.FirstOrDefaultAsync(t => t.TableNumber == "T-02", cancellationToken);
            var itemBurger = await _context.MenuItems.FirstOrDefaultAsync(m => m.ItemCode == "MNU-MAIN-02", cancellationToken);
            var itemCalamari = await _context.MenuItems.FirstOrDefaultAsync(m => m.ItemCode == "MNU-APP-01", cancellationToken);

            if (table2 != null && itemBurger != null && itemCalamari != null)
            {
                var order = new RestaurantOrder
                {
                    PropertyId = 1,
                    OrderNumber = "POS-20261119-0001",
                    OrderType = "DINE_IN",
                    TableId = table2.Id,
                    Status = "COOKING",
                    SubTotal = 47.00m,
                    TaxAmount = 2.35m,
                    TotalAmount = 49.35m,
                    PaymentStatus = "UNPAID",
                    PlacedAt = DateTimeOffset.UtcNow.AddMinutes(-12),
                    SpecialInstructions = "Burger medium-rare please, calamari extra crisp",
                    CreatedBy = "SYSTEM",
                    Items = new List<RestaurantOrderItem>
                    {
                        new() { MenuItemId = itemCalamari.Id, ItemName = itemCalamari.ItemName, Quantity = 1, UnitPrice = itemCalamari.Price, TotalPrice = itemCalamari.Price, Status = "READY", Notes = "Extra lemon", CreatedBy = "SYSTEM" },
                        new() { MenuItemId = itemBurger.Id, ItemName = itemBurger.ItemName, Quantity = 1, UnitPrice = itemBurger.Price, TotalPrice = itemBurger.Price, Status = "PREPARING", Notes = "Medium rare", CreatedBy = "SYSTEM" }
                    }
                };
                _context.RestaurantOrders.Add(order);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        // 6. Banquet Venues & Bookings
        if (!await _context.BanquetVenues.AnyAsync(cancellationToken))
        {
            var grandBallroom = new BanquetVenue
            {
                PropertyId = 1,
                VenueCode = "VNU-GBR",
                VenueName = "The Imperial Grand Ballroom",
                Location = "Level 2, North Wing",
                MaxCapacity = 600,
                HourlyRate = 500m,
                DayRate = 4500m,
                Status = "AVAILABLE",
                CreatedBy = "SYSTEM"
            };

            var sapphireHall = new BanquetVenue
            {
                PropertyId = 1,
                VenueCode = "VNU-SPH",
                VenueName = "The Sapphire Pavilion",
                Location = "Level 1, Poolside Courtyard",
                MaxCapacity = 180,
                HourlyRate = 250m,
                DayRate = 2200m,
                Status = "AVAILABLE",
                CreatedBy = "SYSTEM"
            };

            var azureBoardroom = new BanquetVenue
            {
                PropertyId = 1,
                VenueCode = "VNU-AZR",
                VenueName = "The Azure Executive Boardroom",
                Location = "Level 14, Penthouse Wing",
                MaxCapacity = 30,
                HourlyRate = 120m,
                DayRate = 950m,
                Status = "AVAILABLE",
                CreatedBy = "SYSTEM"
            };

            _context.BanquetVenues.AddRange(grandBallroom, sapphireHall, azureBoardroom);
            await _context.SaveChangesAsync(cancellationToken);

            var todayUtc = new DateTimeOffset(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0, TimeSpan.Zero);
            _context.BanquetBookings.AddRange(
                new BanquetBooking
                {
                    PropertyId = 1,
                    BookingNumber = "BNQ-20261119-0001",
                    VenueId = grandBallroom.Id,
                    EventName = "Global Technology Leadership Summit 2026",
                    EventType = "CONFERENCE",
                    ClientName = "Apex Capital Innovations Inc",
                    ClientPhone = "+1 415 889 2200",
                    ClientEmail = "events@apexcap.com",
                    StartDateTime = todayUtc.AddDays(3).AddHours(9),
                    EndDateTime = todayUtc.AddDays(3).AddHours(18),
                    ExpectedGuests = 350,
                    Status = "CONFIRMED",
                    TotalAmount = 8500m,
                    AdvancePaid = 4000m,
                    BalanceAmount = 4500m,
                    CateringPackage = "Executive Delegate Full Day Buffet",
                    SetupStyle = "THEATER",
                    SpecialRequests = "Keynote dual projection screens and wireless translation headsets",
                    CreatedBy = "SYSTEM"
                },
                new BanquetBooking
                {
                    PropertyId = 1,
                    BookingNumber = "BNQ-20261119-0002",
                    VenueId = sapphireHall.Id,
                    EventName = "Montgomery-Vanderbilt Gala Wedding Reception",
                    EventType = "WEDDING",
                    ClientName = "Lady Genevieve Montgomery",
                    ClientPhone = "+1 212 990 4433",
                    ClientEmail = "g.montgomery@vanderbilt-estate.org",
                    StartDateTime = todayUtc.AddDays(7).AddHours(17),
                    EndDateTime = todayUtc.AddDays(7).AddHours(23),
                    ExpectedGuests = 150,
                    Status = "CONFIRMED",
                    TotalAmount = 14500m,
                    AdvancePaid = 7500m,
                    BalanceAmount = 7000m,
                    CateringPackage = "Royal 5-Course Plated Degustation",
                    SetupStyle = "ROUND_TABLE",
                    SpecialRequests = "White floral arches, grand piano staging, and champagne tower",
                    CreatedBy = "SYSTEM"
                }
            );
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 7. Guest Service Requests
        if (!await _context.GuestServiceRequests.AnyAsync(cancellationToken))
        {
            var room101 = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == "101", cancellationToken);
            var room104 = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == "104", cancellationToken);
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin", cancellationToken);

            if (room101 != null)
            {
                _context.GuestServiceRequests.Add(new GuestServiceRequest
                {
                    PropertyId = 1,
                    RequestNumber = "REQ-20261119-0001",
                    RoomId = room101.Id,
                    RequestType = "EXTRA_TOWELS",
                    Department = "HOUSEKEEPING",
                    Priority = "MEDIUM",
                    Status = "IN_PROGRESS",
                    Description = "Guest requested two additional luxury bath sheets and lavender aromatherapy spray",
                    AssignedToUserId = adminUser?.Id,
                    RequestedAt = DateTimeOffset.UtcNow.AddMinutes(-25),
                    CreatedBy = "SYSTEM"
                });
            }

            if (room104 != null)
            {
                _context.GuestServiceRequests.Add(new GuestServiceRequest
                {
                    PropertyId = 1,
                    RequestNumber = "REQ-20261119-0002",
                    RoomId = room104.Id,
                    RequestType = "CONCIERGE",
                    Department = "FRONT_OFFICE",
                    Priority = "HIGH",
                    Status = "RESOLVED",
                    Description = "Airport transfer requested for 6:00 AM flight departure tomorrow",
                    AssignedToUserId = adminUser?.Id,
                    RequestedAt = DateTimeOffset.UtcNow.AddHours(-3),
                    ResolvedAt = DateTimeOffset.UtcNow.AddHours(-1),
                    ResolutionNotes = "Reserved Mercedes S-Class Chauffeur with driver John Davis (+1 555 9011)",
                    CreatedBy = "SYSTEM"
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedErpAsync(CancellationToken cancellationToken)
    {
        // 1. Vendors
        if (!await _context.Vendors.AnyAsync(cancellationToken))
        {
            var vendors = new List<Vendor>
            {
                new()
                {
                    PropertyId = 1,
                    VendorCode = "VND-001",
                    VendorName = "Grand Linen & Textiles Corp",
                    ContactPerson = "Sarah Jenkins",
                    Email = "orders@grandlinen.com",
                    Phone = "+1 312 555 0192",
                    Address = "400 Industrial Pkwy, Chicago, IL",
                    TaxNumber = "US-9921441",
                    PaymentTermsDays = 30,
                    Status = "ACTIVE",
                    CreatedBy = "SYSTEM"
                },
                new()
                {
                    PropertyId = 1,
                    VendorCode = "VND-002",
                    VendorName = "Pacific Seafood & Gourmet Produce",
                    ContactPerson = "Chef Marco Rossi",
                    Email = "sales@pacificproduce.com",
                    Phone = "+1 415 555 8821",
                    Address = "Pier 45 Fishermans Wharf, San Francisco, CA",
                    TaxNumber = "US-8812330",
                    PaymentTermsDays = 15,
                    Status = "ACTIVE",
                    CreatedBy = "SYSTEM"
                },
                new()
                {
                    PropertyId = 1,
                    VendorCode = "VND-003",
                    VendorName = "Apex Hotel Amenities & Toiletries",
                    ContactPerson = "David Sterling",
                    Email = "contact@apexamenities.com",
                    Phone = "+1 212 555 9934",
                    Address = "789 5th Ave, New York, NY",
                    TaxNumber = "US-7744112",
                    PaymentTermsDays = 30,
                    Status = "ACTIVE",
                    CreatedBy = "SYSTEM"
                },
                new()
                {
                    PropertyId = 1,
                    VendorCode = "VND-004",
                    VendorName = "Otis Mechanical & Elevator Services",
                    ContactPerson = "Tech Support",
                    Email = "service@otis-maint.com",
                    Phone = "+1 800 555 4321",
                    Address = "100 Engineering Blvd, Dallas, TX",
                    TaxNumber = "US-6633221",
                    PaymentTermsDays = 45,
                    Status = "ACTIVE",
                    CreatedBy = "SYSTEM"
                }
            };
            _context.Vendors.AddRange(vendors);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 2. Inventory Items
        if (!await _context.InventoryItems.AnyAsync(cancellationToken))
        {
            var items = new List<InventoryItem>
            {
                new()
                {
                    PropertyId = 1,
                    ItemCode = "INV-LNN-001",
                    ItemName = "Egyptian Cotton Bed Sheet 600TC King",
                    Category = "LINEN",
                    UnitOfMeasure = "PCS",
                    UnitCost = 35.00m,
                    CurrentStock = 240,
                    ReorderLevel = 60,
                    MinimumStock = 40,
                    MaximumStock = 500,
                    LocationBin = "BIN-LNN-A01",
                    CreatedBy = "SYSTEM"
                },
                new()
                {
                    PropertyId = 1,
                    ItemCode = "INV-FNB-001",
                    ItemName = "A5 Japanese Wagyu Ribeye Cut 10oz",
                    Category = "FOOD_BEVERAGE",
                    UnitOfMeasure = "KG",
                    UnitCost = 95.00m,
                    CurrentStock = 18,
                    ReorderLevel = 15,
                    MinimumStock = 10,
                    MaximumStock = 60,
                    LocationBin = "FREEZER-01",
                    CreatedBy = "SYSTEM"
                },
                new()
                {
                    PropertyId = 1,
                    ItemCode = "INV-AMN-001",
                    ItemName = "Verbena Botanical Luxury Shampoo 50ml",
                    Category = "AMENITY",
                    UnitOfMeasure = "BOTTLE",
                    UnitCost = 1.85m,
                    CurrentStock = 1200,
                    ReorderLevel = 400,
                    MinimumStock = 250,
                    MaximumStock = 3000,
                    LocationBin = "BIN-AMN-B12",
                    CreatedBy = "SYSTEM"
                },
                new()
                {
                    PropertyId = 1,
                    ItemCode = "INV-MNT-001",
                    ItemName = "HEPA H13 Air Purification Filter Cartridge",
                    Category = "MAINTENANCE",
                    UnitOfMeasure = "UNIT",
                    UnitCost = 28.50m,
                    CurrentStock = 45,
                    ReorderLevel = 20,
                    MinimumStock = 15,
                    MaximumStock = 120,
                    LocationBin = "BIN-ENG-C05",
                    CreatedBy = "SYSTEM"
                }
            };
            _context.InventoryItems.AddRange(items);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 3. Finance Chart of Accounts
        if (!await _context.Accounts.AnyAsync(cancellationToken))
        {
            var accounts = new List<Account>
            {
                new() { PropertyId = 1, AccountCode = "1010", AccountName = "Operating Cash on Hand", AccountType = "ASSET", Balance = 25000m, Description = "Front Desk Cash Float and Vault Petty Cash", CreatedBy = "SYSTEM" },
                new() { PropertyId = 1, AccountCode = "1020", AccountName = "Commercial Operating Bank Account", AccountType = "ASSET", Balance = 485000m, Description = "JPMorgan Chase Primary Operations Account", CreatedBy = "SYSTEM" },
                new() { PropertyId = 1, AccountCode = "1100", AccountName = "Accounts Receivable (City Ledger)", AccountType = "ASSET", Balance = 14500m, Description = "Corporate and Travel Agency Pending Settlements", CreatedBy = "SYSTEM" },
                new() { PropertyId = 1, AccountCode = "1200", AccountName = "Inventory Asset - Operating Supplies", AccountType = "ASSET", Balance = 38200m, Description = "Central Store and Linen Stock Valuation", CreatedBy = "SYSTEM" },
                new() { PropertyId = 1, AccountCode = "2010", AccountName = "Accounts Payable - Trade Vendors", AccountType = "LIABILITY", Balance = 18900m, Description = "Outstanding Vendor Invoices and Procurement Liabilities", CreatedBy = "SYSTEM" },
                new() { PropertyId = 1, AccountCode = "3010", AccountName = "Owner Equity & Retained Capital", AccountType = "EQUITY", Balance = 500000m, Description = "Shareholder Capital and Prior Year Retained Earnings", CreatedBy = "SYSTEM" },
                new() { PropertyId = 1, AccountCode = "4010", AccountName = "Guest Room Revenue", AccountType = "REVENUE", Balance = 125000m, Description = "Nightly Room Charges and Tariff Revenues", CreatedBy = "SYSTEM" },
                new() { PropertyId = 1, AccountCode = "4020", AccountName = "Food & Beverage Revenue", AccountType = "REVENUE", Balance = 48000m, Description = "Restaurant Dining, Room Service, and Bar Revenues", CreatedBy = "SYSTEM" },
                new() { PropertyId = 1, AccountCode = "5010", AccountName = "Cost of Goods Sold - Food & Beverage", AccountType = "EXPENSE", Balance = 19200m, Description = "Ingredient Procurement and Culinary Consumption Costs", CreatedBy = "SYSTEM" }
            };
            _context.Accounts.AddRange(accounts);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 4. Employees & Attendance
        if (!await _context.Employees.AnyAsync(cancellationToken))
        {
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin", cancellationToken);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var employees = new List<Employee>
            {
                new()
                {
                    PropertyId = 1,
                    EmployeeCode = "EMP-001",
                    UserId = adminUser?.Id,
                    FirstName = "Eleanor",
                    LastName = "Vance",
                    DepartmentCode = "ADMIN",
                    Designation = "General Manager",
                    DateOfJoining = new DateOnly(2023, 1, 15),
                    EmploymentType = "FULL_TIME",
                    BasicSalary = 9500m,
                    Status = "ACTIVE",
                    Email = "eleanor.vance@hotelos.com",
                    Phone = "+1 555 1010",
                    EmergencyContact = "+1 555 9901",
                    CreatedBy = "SYSTEM"
                },
                new()
                {
                    PropertyId = 1,
                    EmployeeCode = "EMP-002",
                    FirstName = "Marcus",
                    LastName = "Chen",
                    DepartmentCode = "FO",
                    Designation = "Front Desk Lead",
                    DateOfJoining = new DateOnly(2024, 3, 1),
                    EmploymentType = "FULL_TIME",
                    BasicSalary = 4200m,
                    Status = "ACTIVE",
                    Email = "marcus.chen@hotelos.com",
                    Phone = "+1 555 1011",
                    CreatedBy = "SYSTEM"
                },
                new()
                {
                    PropertyId = 1,
                    EmployeeCode = "EMP-003",
                    FirstName = "Sofia",
                    LastName = "Ramirez",
                    DepartmentCode = "HK",
                    Designation = "Executive Housekeeper",
                    DateOfJoining = new DateOnly(2023, 6, 20),
                    EmploymentType = "FULL_TIME",
                    BasicSalary = 4500m,
                    Status = "ACTIVE",
                    Email = "sofia.ramirez@hotelos.com",
                    Phone = "+1 555 1012",
                    CreatedBy = "SYSTEM"
                },
                new()
                {
                    PropertyId = 1,
                    EmployeeCode = "EMP-004",
                    FirstName = "Jean-Luc",
                    LastName = "Moreau",
                    DepartmentCode = "FB",
                    Designation = "Executive Chef",
                    DateOfJoining = new DateOnly(2023, 11, 10),
                    EmploymentType = "FULL_TIME",
                    BasicSalary = 6800m,
                    Status = "ACTIVE",
                    Email = "chef.moreau@hotelos.com",
                    Phone = "+1 555 1013",
                    CreatedBy = "SYSTEM"
                },
                new()
                {
                    PropertyId = 1,
                    EmployeeCode = "EMP-005",
                    FirstName = "Derrick",
                    LastName = "Kowalski",
                    DepartmentCode = "ENG",
                    Designation = "Chief Engineer",
                    DateOfJoining = new DateOnly(2024, 1, 8),
                    EmploymentType = "FULL_TIME",
                    BasicSalary = 5200m,
                    Status = "ACTIVE",
                    Email = "derrick.k@hotelos.com",
                    Phone = "+1 555 1014",
                    CreatedBy = "SYSTEM"
                },
                new()
                {
                    PropertyId = 1,
                    EmployeeCode = "EMP-006",
                    FirstName = "Tariq",
                    LastName = "Mansoor",
                    DepartmentCode = "SEC",
                    Designation = "Head of Security",
                    DateOfJoining = new DateOnly(2024, 2, 14),
                    EmploymentType = "FULL_TIME",
                    BasicSalary = 4600m,
                    Status = "ACTIVE",
                    Email = "tariq.m@hotelos.com",
                    Phone = "+1 555 1015",
                    CreatedBy = "SYSTEM"
                }
            };

            _context.Employees.AddRange(employees);
            await _context.SaveChangesAsync(cancellationToken);

            // Seed today's attendance for employees
            var empList = await _context.Employees.Where(e => e.PropertyId == 1).ToListAsync(cancellationToken);
            foreach (var emp in empList)
            {
                _context.AttendanceRecords.Add(new AttendanceRecord
                {
                    PropertyId = 1,
                    EmployeeId = emp.Id,
                    Date = today,
                    CheckInTime = new TimeOnly(8, 0),
                    Shift = "MORNING",
                    Status = "PRESENT",
                    OvertimeHours = 0,
                    Remarks = "Standard scheduled morning shift",
                    CreatedBy = "SYSTEM"
                });
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 5. Maintenance Assets & Work Orders
        if (!await _context.Assets.AnyAsync(cancellationToken))
        {
            var assets = new List<Asset>
            {
                new()
                {
                    PropertyId = 1,
                    AssetCode = "AST-HVAC-01",
                    AssetName = "Trane Centrifugal Water Chiller 400T",
                    Category = "HVAC",
                    Location = "Basement Mechanical Plant B-02",
                    PurchaseDate = new DateOnly(2023, 4, 10),
                    PurchaseCost = 85000m,
                    WarrantyExpiryDate = new DateOnly(2028, 4, 10),
                    Status = "OPERATIONAL",
                    SerialNumber = "TR-CHL-99812",
                    Manufacturer = "Trane Commercial Systems",
                    CreatedBy = "SYSTEM"
                },
                new()
                {
                    PropertyId = 1,
                    AssetCode = "AST-ELV-01",
                    AssetName = "Otis SkyRise High-Speed Elevator - North Tower",
                    Category = "ELEVATOR",
                    Location = "North Wing Central Shaft",
                    PurchaseDate = new DateOnly(2023, 2, 20),
                    PurchaseCost = 120000m,
                    WarrantyExpiryDate = new DateOnly(2026, 2, 20),
                    Status = "OPERATIONAL",
                    SerialNumber = "OTIS-SR-4412",
                    Manufacturer = "Otis Elevator Company",
                    CreatedBy = "SYSTEM"
                },
                new()
                {
                    PropertyId = 1,
                    AssetCode = "AST-GEN-01",
                    AssetName = "Caterpillar 500kVA Diesel Standby Generator",
                    Category = "ELECTRICAL",
                    Location = "Service Yard Generator Enclosure",
                    PurchaseDate = new DateOnly(2023, 1, 10),
                    PurchaseCost = 65000m,
                    WarrantyExpiryDate = new DateOnly(2027, 1, 10),
                    Status = "OPERATIONAL",
                    SerialNumber = "CAT-500-GEN-891",
                    Manufacturer = "Caterpillar Energy",
                    CreatedBy = "SYSTEM"
                }
            };
            _context.Assets.AddRange(assets);
            await _context.SaveChangesAsync(cancellationToken);

            var firstAsset = await _context.Assets.FirstOrDefaultAsync(cancellationToken);
            var room102 = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == "102", cancellationToken);
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin", cancellationToken);

            _context.WorkOrders.Add(new WorkOrder
            {
                PropertyId = 1,
                WorkOrderNumber = "WO-20261119-001",
                AssetId = firstAsset?.Id,
                RoomId = room102?.Id,
                DepartmentCode = "ENG",
                IssueDescription = "Thermostat cooling sensor calibration check requested",
                Priority = "MEDIUM",
                Status = "IN_PROGRESS",
                AssignedToUserId = adminUser?.Id,
                RequestedAt = DateTimeOffset.UtcNow.AddHours(-4),
                TotalCost = 0,
                CreatedBy = "SYSTEM"
            });
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 6. Security Visitors & Incidents
        if (!await _context.VisitorLogs.AnyAsync(cancellationToken))
        {
            _context.VisitorLogs.Add(new VisitorLog
            {
                PropertyId = 1,
                PassNumber = "PASS-20261119-001",
                VisitorName = "Michael Bloomberg",
                IdType = "PASSPORT",
                IdNumber = "USA-992144810",
                Phone = "+1 212 555 7711",
                Purpose = "Executive Quarterly Strategic Review with GM",
                VisitingDepartment = "ADMIN",
                RoomOrArea = "Executive Boardroom Suite 1401",
                BadgeNumber = "SEC-VIS-08",
                CheckInTime = DateTimeOffset.UtcNow.AddHours(-2),
                Status = "IN_PREMISES",
                CreatedBy = "SYSTEM"
            });
            await _context.SaveChangesAsync(cancellationToken);
        }

        if (!await _context.IncidentReports.AnyAsync(cancellationToken))
        {
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin", cancellationToken);
            _context.IncidentReports.Add(new IncidentReport
            {
                PropertyId = 1,
                IncidentNumber = "INC-20261119-001",
                IncidentDate = DateTimeOffset.UtcNow.AddDays(-1),
                Location = "Service Corridor Level 3 East",
                Severity = "LOW",
                Category = "PROPERTY_DAMAGE",
                Description = "Minor condensation pipe drip identified during routine security patrol",
                ReportedByUserId = adminUser?.Id,
                Status = "RESOLVED",
                ActionTaken = "Engineering tightened valve seal, no damage to drywall or carpeting",
                ResolvedAt = DateTimeOffset.UtcNow.AddHours(-18),
                CreatedBy = "SYSTEM"
            });
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedDocumentNumberingAsync(CancellationToken cancellationToken)
    {
        await _context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS configuration.document_number_configurations (
                ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                ""PropertyId"" bigint NOT NULL,
                ""DocumentType"" character varying(100) NOT NULL,
                ""Module"" character varying(100) NOT NULL,
                ""Prefix"" character varying(20) NOT NULL,
                ""Suffix"" character varying(20),
                ""DateFormat"" character varying(20) NOT NULL,
                ""SequenceLength"" integer NOT NULL,
                ""StartingNumber"" bigint NOT NULL,
                ""ResetFrequency"" character varying(20) NOT NULL,
                ""CurrentSequence"" bigint NOT NULL,
                ""LastResetDate"" date NOT NULL,
                ""IsActive"" boolean NOT NULL,
                ""CreatedAt"" timestamp with time zone NOT NULL,
                ""CreatedBy"" character varying(100) NOT NULL,
                ""UpdatedAt"" timestamp with time zone,
                ""UpdatedBy"" character varying(100),
                ""Version"" integer NOT NULL DEFAULT 1
            );

            CREATE UNIQUE INDEX IF NOT EXISTS ix_doc_num_cfg_prop_type 
            ON configuration.document_number_configurations (""PropertyId"", ""DocumentType"");

            CREATE TABLE IF NOT EXISTS configuration.document_number_audits (
                ""Id"" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                ""PropertyId"" bigint NOT NULL,
                ""DocumentType"" character varying(100) NOT NULL,
                ""DocumentNumber"" character varying(100) NOT NULL,
                ""GeneratedBy"" character varying(100) NOT NULL,
                ""GeneratedAt"" timestamp with time zone NOT NULL,
                ""EntityId"" bigint,
                ""EntityType"" character varying(100),
                ""Status"" character varying(50) NOT NULL
            );

            CREATE INDEX IF NOT EXISTS ix_doc_num_audit_prop_type 
            ON configuration.document_number_audits (""PropertyId"", ""DocumentType"");

            CREATE INDEX IF NOT EXISTS ix_doc_num_audit_number 
            ON configuration.document_number_audits (""DocumentNumber"");
        ", cancellationToken);

        var configs = new[]
        {
            new { Type = "RESERVATION", Module = "Reservation", Prefix = "RES-", Suffix = "", DateFormat = "yyyyMMdd-", Length = 4 },
            new { Type = "JOURNAL_VOUCHER", Module = "Finance", Prefix = "JV-", Suffix = "", DateFormat = "yyyyMMdd-", Length = 4 },
            new { Type = "PAYMENT_VOUCHER", Module = "Finance", Prefix = "PV-", Suffix = "", DateFormat = "yyyyMMdd-", Length = 4 },
            new { Type = "RECEIPT_VOUCHER", Module = "Finance", Prefix = "RV-", Suffix = "", DateFormat = "yyyyMMdd-", Length = 4 },
            new { Type = "PURCHASE_REQUISITION", Module = "Purchase", Prefix = "PR-", Suffix = "", DateFormat = "yyyyMMdd-", Length = 4 },
            new { Type = "PURCHASE_ORDER", Module = "Purchase", Prefix = "PO-", Suffix = "", DateFormat = "yyyyMMdd-", Length = 4 },
            new { Type = "GOODS_RECEIPT", Module = "Purchase", Prefix = "GRN-", Suffix = "", DateFormat = "yyyyMMdd-", Length = 4 },
            new { Type = "WORK_ORDER", Module = "Maintenance", Prefix = "WO-", Suffix = "", DateFormat = "yyyyMMdd-", Length = 4 },
            new { Type = "RESTAURANT_ORDER", Module = "Restaurant", Prefix = "KOT-", Suffix = "", DateFormat = "yyyyMMdd-", Length = 4 },
            new { Type = "INVOICE", Module = "Finance", Prefix = "INV-", Suffix = "", DateFormat = "yyyyMMdd-", Length = 4 },
            new { Type = "BANQUET_BOOKING", Module = "Banquet", Prefix = "BNQ-", Suffix = "", DateFormat = "yyyyMMdd-", Length = 4 },
            new { Type = "INCIDENT_REPORT", Module = "Security", Prefix = "INC-", Suffix = "", DateFormat = "yyyyMMdd-", Length = 4 },
            new { Type = "VISITOR_PASS", Module = "Security", Prefix = "VIS-", Suffix = "", DateFormat = "yyyyMMdd-", Length = 4 }
        };

        foreach (var c in configs)
        {
            if (!await _context.DocumentNumberConfigurations.AnyAsync(x => x.DocumentType == c.Type && x.PropertyId == 1, cancellationToken))
            {
                _context.DocumentNumberConfigurations.Add(new DocumentNumberConfiguration
                {
                    PropertyId = 1,
                    DocumentType = c.Type,
                    Module = c.Module,
                    Prefix = c.Prefix,
                    Suffix = c.Suffix,
                    DateFormat = c.DateFormat,
                    SequenceLength = c.Length,
                    StartingNumber = 1,
                    ResetFrequency = "DAILY",
                    CurrentSequence = 0,
                    LastResetDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    IsActive = true,
                    CreatedBy = "SYSTEM"
                });
            }
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HotelOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase5OperationsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "banquet");

            migrationBuilder.EnsureSchema(
                name: "restaurant");

            migrationBuilder.EnsureSchema(
                name: "operations");

            migrationBuilder.EnsureSchema(
                name: "housekeeping");

            migrationBuilder.CreateTable(
                name: "banquet_venues",
                schema: "banquet",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    VenueCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VenueName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MaxCapacity = table.Column<int>(type: "integer", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DayRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "AVAILABLE"),
                    AmenitiesJson = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    EntrySource = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banquet_venues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dining_tables",
                schema: "restaurant",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    TableNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Section = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "MAIN_DINING"),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "AVAILABLE"),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    EntrySource = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dining_tables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "guest_service_requests",
                schema: "operations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    RequestNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RoomId = table.Column<long>(type: "bigint", nullable: false),
                    GuestId = table.Column<long>(type: "bigint", nullable: true),
                    RequestType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Department = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "HOUSEKEEPING"),
                    Priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "MEDIUM"),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "PENDING"),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AssignedToUserId = table.Column<long>(type: "bigint", nullable: true),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    EntrySource = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guest_service_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_guest_service_requests_guests_GuestId",
                        column: x => x.GuestId,
                        principalSchema: "pms",
                        principalTable: "guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_guest_service_requests_rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "pms",
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_guest_service_requests_users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "housekeeping_tasks",
                schema: "housekeeping",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    TaskNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RoomId = table.Column<long>(type: "bigint", nullable: false),
                    TaskType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "MEDIUM"),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "PENDING"),
                    AssignedToUserId = table.Column<long>(type: "bigint", nullable: true),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    InspectedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    InspectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ChecklistJson = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    EntrySource = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_housekeeping_tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_housekeeping_tasks_rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "pms",
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_housekeeping_tasks_users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_housekeeping_tasks_users_InspectedByUserId",
                        column: x => x.InspectedByUserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "laundry_orders",
                schema: "housekeeping",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OrderType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "GUEST"),
                    RoomId = table.Column<long>(type: "bigint", nullable: true),
                    GuestId = table.Column<long>(type: "bigint", nullable: true),
                    FolioId = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "RECEIVED"),
                    TotalPieces = table.Column<int>(type: "integer", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsExpress = table.Column<bool>(type: "boolean", nullable: false),
                    SpecialInstructions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DeliveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    EntrySource = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_laundry_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_laundry_orders_folios_FolioId",
                        column: x => x.FolioId,
                        principalSchema: "pms",
                        principalTable: "folios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_laundry_orders_guests_GuestId",
                        column: x => x.GuestId,
                        principalSchema: "pms",
                        principalTable: "guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_laundry_orders_rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "pms",
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "linen_items",
                schema: "housekeeping",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ParLevel = table.Column<int>(type: "integer", nullable: false),
                    CleanStock = table.Column<int>(type: "integer", nullable: false),
                    InUseStock = table.Column<int>(type: "integer", nullable: false),
                    InLaundryStock = table.Column<int>(type: "integer", nullable: false),
                    DiscardedStock = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true, defaultValue: "PCS"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    EntrySource = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linen_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "menu_items",
                schema: "restaurant",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "MAIN_COURSE"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PreparationTimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    DietaryType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "VEG"),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    EntrySource = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "banquet_bookings",
                schema: "banquet",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    BookingNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    VenueId = table.Column<long>(type: "bigint", nullable: false),
                    EventName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EventType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "WEDDING"),
                    ClientName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ClientPhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ClientEmail = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    GuestId = table.Column<long>(type: "bigint", nullable: true),
                    StartDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpectedGuests = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "INQUIRY"),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AdvancePaid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CateringPackage = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SetupStyle = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SpecialRequests = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    EntrySource = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banquet_bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_banquet_bookings_banquet_venues_VenueId",
                        column: x => x.VenueId,
                        principalSchema: "banquet",
                        principalTable: "banquet_venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_banquet_bookings_guests_GuestId",
                        column: x => x.GuestId,
                        principalSchema: "pms",
                        principalTable: "guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "restaurant_orders",
                schema: "restaurant",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OrderType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "DINE_IN"),
                    TableId = table.Column<long>(type: "bigint", nullable: true),
                    RoomId = table.Column<long>(type: "bigint", nullable: true),
                    GuestId = table.Column<long>(type: "bigint", nullable: true),
                    FolioId = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "PLACED"),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "UNPAID"),
                    PaymentMethod = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ServerUserId = table.Column<long>(type: "bigint", nullable: true),
                    SpecialInstructions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PlacedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PreparedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ServedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SettledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    EntrySource = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_restaurant_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_restaurant_orders_dining_tables_TableId",
                        column: x => x.TableId,
                        principalSchema: "restaurant",
                        principalTable: "dining_tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_restaurant_orders_folios_FolioId",
                        column: x => x.FolioId,
                        principalSchema: "pms",
                        principalTable: "folios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_restaurant_orders_guests_GuestId",
                        column: x => x.GuestId,
                        principalSchema: "pms",
                        principalTable: "guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_restaurant_orders_rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "pms",
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_restaurant_orders_users_ServerUserId",
                        column: x => x.ServerUserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "laundry_order_items",
                schema: "housekeeping",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LaundryOrderId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    GarmentName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    EntrySource = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_laundry_order_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_laundry_order_items_laundry_orders_LaundryOrderId",
                        column: x => x.LaundryOrderId,
                        principalSchema: "housekeeping",
                        principalTable: "laundry_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "restaurant_order_items",
                schema: "restaurant",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RestaurantOrderId = table.Column<long>(type: "bigint", nullable: false),
                    MenuItemId = table.Column<long>(type: "bigint", nullable: false),
                    ItemName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "QUEUED"),
                    Notes = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    EntrySource = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_restaurant_order_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_restaurant_order_items_menu_items_MenuItemId",
                        column: x => x.MenuItemId,
                        principalSchema: "restaurant",
                        principalTable: "menu_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_restaurant_order_items_restaurant_orders_RestaurantOrderId",
                        column: x => x.RestaurantOrderId,
                        principalSchema: "restaurant",
                        principalTable: "restaurant_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_banquet_bookings_GuestId",
                schema: "banquet",
                table: "banquet_bookings",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_banquet_bookings_PropertyId_BookingNumber",
                schema: "banquet",
                table: "banquet_bookings",
                columns: new[] { "PropertyId", "BookingNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_banquet_bookings_Status",
                schema: "banquet",
                table: "banquet_bookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_banquet_bookings_VenueId",
                schema: "banquet",
                table: "banquet_bookings",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_banquet_venues_PropertyId_VenueCode",
                schema: "banquet",
                table: "banquet_venues",
                columns: new[] { "PropertyId", "VenueCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_banquet_venues_Status",
                schema: "banquet",
                table: "banquet_venues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_dining_tables_PropertyId_TableNumber",
                schema: "restaurant",
                table: "dining_tables",
                columns: new[] { "PropertyId", "TableNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dining_tables_Section",
                schema: "restaurant",
                table: "dining_tables",
                column: "Section");

            migrationBuilder.CreateIndex(
                name: "IX_dining_tables_Status",
                schema: "restaurant",
                table: "dining_tables",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_guest_service_requests_AssignedToUserId",
                schema: "operations",
                table: "guest_service_requests",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_guest_service_requests_Department",
                schema: "operations",
                table: "guest_service_requests",
                column: "Department");

            migrationBuilder.CreateIndex(
                name: "IX_guest_service_requests_GuestId",
                schema: "operations",
                table: "guest_service_requests",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_guest_service_requests_PropertyId_RequestNumber",
                schema: "operations",
                table: "guest_service_requests",
                columns: new[] { "PropertyId", "RequestNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_guest_service_requests_RoomId",
                schema: "operations",
                table: "guest_service_requests",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_guest_service_requests_Status",
                schema: "operations",
                table: "guest_service_requests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_housekeeping_tasks_AssignedToUserId",
                schema: "housekeeping",
                table: "housekeeping_tasks",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_housekeeping_tasks_InspectedByUserId",
                schema: "housekeeping",
                table: "housekeeping_tasks",
                column: "InspectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_housekeeping_tasks_PropertyId_TaskNumber",
                schema: "housekeeping",
                table: "housekeeping_tasks",
                columns: new[] { "PropertyId", "TaskNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_housekeeping_tasks_RoomId",
                schema: "housekeeping",
                table: "housekeeping_tasks",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_housekeeping_tasks_Status",
                schema: "housekeeping",
                table: "housekeeping_tasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_laundry_order_items_LaundryOrderId",
                schema: "housekeeping",
                table: "laundry_order_items",
                column: "LaundryOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_laundry_orders_FolioId",
                schema: "housekeeping",
                table: "laundry_orders",
                column: "FolioId");

            migrationBuilder.CreateIndex(
                name: "IX_laundry_orders_GuestId",
                schema: "housekeeping",
                table: "laundry_orders",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_laundry_orders_PropertyId_OrderNumber",
                schema: "housekeeping",
                table: "laundry_orders",
                columns: new[] { "PropertyId", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_laundry_orders_RoomId",
                schema: "housekeeping",
                table: "laundry_orders",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_laundry_orders_Status",
                schema: "housekeeping",
                table: "laundry_orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_linen_items_Category",
                schema: "housekeeping",
                table: "linen_items",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_linen_items_PropertyId_ItemCode",
                schema: "housekeeping",
                table: "linen_items",
                columns: new[] { "PropertyId", "ItemCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_Category",
                schema: "restaurant",
                table: "menu_items",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_IsAvailable",
                schema: "restaurant",
                table: "menu_items",
                column: "IsAvailable");

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_PropertyId_ItemCode",
                schema: "restaurant",
                table: "menu_items",
                columns: new[] { "PropertyId", "ItemCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_restaurant_order_items_MenuItemId",
                schema: "restaurant",
                table: "restaurant_order_items",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_restaurant_order_items_RestaurantOrderId",
                schema: "restaurant",
                table: "restaurant_order_items",
                column: "RestaurantOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_restaurant_order_items_Status",
                schema: "restaurant",
                table: "restaurant_order_items",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_restaurant_orders_FolioId",
                schema: "restaurant",
                table: "restaurant_orders",
                column: "FolioId");

            migrationBuilder.CreateIndex(
                name: "IX_restaurant_orders_GuestId",
                schema: "restaurant",
                table: "restaurant_orders",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_restaurant_orders_OrderType",
                schema: "restaurant",
                table: "restaurant_orders",
                column: "OrderType");

            migrationBuilder.CreateIndex(
                name: "IX_restaurant_orders_PropertyId_OrderNumber",
                schema: "restaurant",
                table: "restaurant_orders",
                columns: new[] { "PropertyId", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_restaurant_orders_RoomId",
                schema: "restaurant",
                table: "restaurant_orders",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_restaurant_orders_ServerUserId",
                schema: "restaurant",
                table: "restaurant_orders",
                column: "ServerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_restaurant_orders_Status",
                schema: "restaurant",
                table: "restaurant_orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_restaurant_orders_TableId",
                schema: "restaurant",
                table: "restaurant_orders",
                column: "TableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "banquet_bookings",
                schema: "banquet");

            migrationBuilder.DropTable(
                name: "guest_service_requests",
                schema: "operations");

            migrationBuilder.DropTable(
                name: "housekeeping_tasks",
                schema: "housekeeping");

            migrationBuilder.DropTable(
                name: "laundry_order_items",
                schema: "housekeeping");

            migrationBuilder.DropTable(
                name: "linen_items",
                schema: "housekeeping");

            migrationBuilder.DropTable(
                name: "restaurant_order_items",
                schema: "restaurant");

            migrationBuilder.DropTable(
                name: "banquet_venues",
                schema: "banquet");

            migrationBuilder.DropTable(
                name: "laundry_orders",
                schema: "housekeeping");

            migrationBuilder.DropTable(
                name: "menu_items",
                schema: "restaurant");

            migrationBuilder.DropTable(
                name: "restaurant_orders",
                schema: "restaurant");

            migrationBuilder.DropTable(
                name: "dining_tables",
                schema: "restaurant");
        }
    }
}

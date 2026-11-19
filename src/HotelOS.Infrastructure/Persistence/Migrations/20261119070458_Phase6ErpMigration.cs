using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HotelOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase6ErpMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "finance");

            migrationBuilder.EnsureSchema(
                name: "maintenance");

            migrationBuilder.EnsureSchema(
                name: "hr");

            migrationBuilder.EnsureSchema(
                name: "security");

            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.EnsureSchema(
                name: "purchase");

            migrationBuilder.CreateTable(
                name: "accounts",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    AccountCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AccountType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assets",
                schema: "maintenance",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    AssetCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssetName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PurchaseDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PurchaseCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    WarrantyExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "OPERATIONAL"),
                    SerialNumber = table.Column<string>(type: "text", nullable: true),
                    Manufacturer = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    EmployeeCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DepartmentCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Designation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateOfJoining = table.Column<DateOnly>(type: "date", nullable: false),
                    EmploymentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "FULL_TIME"),
                    BasicSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "ACTIVE"),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    EmergencyContact = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_employees_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "incident_reports",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    IncidentNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IncidentDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "MEDIUM"),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ReportedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "REPORTED"),
                    ActionTaken = table.Column<string>(type: "text", nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_incident_reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_incident_reports_users_ReportedByUserId",
                        column: x => x.ReportedByUserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "inventory_items",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "PCS"),
                    UnitCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentStock = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReorderLevel = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MinimumStock = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MaximumStock = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LocationBin = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_inventory_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    InvoiceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReferenceId = table.Column<string>(type: "text", nullable: true),
                    ClientOrVendorName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "ISSUED"),
                    Notes = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_invoices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "journal_entries",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    EntryNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "POSTED"),
                    TotalDebit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCredit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_journal_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "purchase_requisitions",
                schema: "purchase",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    RequisitionNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DepartmentCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequestedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    RequiredByDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "MEDIUM"),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "DRAFT"),
                    TotalEstimatedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Justification = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_purchase_requisitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchase_requisitions_users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vendors",
                schema: "purchase",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    VendorCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VendorName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    TaxNumber = table.Column<string>(type: "text", nullable: true),
                    PaymentTermsDays = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "ACTIVE"),
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
                    table.PrimaryKey("PK_vendors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "visitor_logs",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    PassNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    VisitorName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IdType = table.Column<string>(type: "text", nullable: false),
                    IdNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Purpose = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    VisitingDepartment = table.Column<string>(type: "text", nullable: false),
                    RoomOrArea = table.Column<string>(type: "text", nullable: true),
                    CheckInTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CheckOutTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "IN_PREMISES"),
                    BadgeNumber = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_visitor_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "work_orders",
                schema: "maintenance",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    WorkOrderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AssetId = table.Column<long>(type: "bigint", nullable: true),
                    RoomId = table.Column<long>(type: "bigint", nullable: true),
                    DepartmentCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IssueDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "MEDIUM"),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "OPEN"),
                    AssignedToUserId = table.Column<long>(type: "bigint", nullable: true),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "text", nullable: true),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_work_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_work_orders_assets_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "maintenance",
                        principalTable: "assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_work_orders_rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "pms",
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_work_orders_users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "attendance_records",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckInTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    CheckOutTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Shift = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "MORNING"),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "PRESENT"),
                    OvertimeHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_attendance_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_attendance_records_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "hr",
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_transactions",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    TransactionNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    InventoryItemId = table.Column<long>(type: "bigint", nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DepartmentCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TransactionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_stock_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stock_transactions_inventory_items_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalSchema: "inventory",
                        principalTable: "inventory_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "journal_lines",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JournalEntryId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    DebitAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_journal_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_journal_lines_accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "finance",
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_journal_lines_journal_entries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalSchema: "finance",
                        principalTable: "journal_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchase_requisition_items",
                schema: "purchase",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseRequisitionId = table.Column<long>(type: "bigint", nullable: false),
                    ItemName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    EstimatedUnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalEstimatedPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_purchase_requisition_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchase_requisition_items_purchase_requisitions_PurchaseRe~",
                        column: x => x.PurchaseRequisitionId,
                        principalSchema: "purchase",
                        principalTable: "purchase_requisitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchase_orders",
                schema: "purchase",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PurchaseRequisitionId = table.Column<long>(type: "bigint", nullable: true),
                    VendorId = table.Column<long>(type: "bigint", nullable: false),
                    OrderDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "ISSUED"),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentTerms = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_purchase_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchase_orders_purchase_requisitions_PurchaseRequisitionId",
                        column: x => x.PurchaseRequisitionId,
                        principalSchema: "purchase",
                        principalTable: "purchase_requisitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_purchase_orders_vendors_VendorId",
                        column: x => x.VendorId,
                        principalSchema: "purchase",
                        principalTable: "vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_order_items",
                schema: "purchase",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseOrderId = table.Column<long>(type: "bigint", nullable: false),
                    ItemName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    QuantityOrdered = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    QuantityReceived = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_purchase_order_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchase_order_items_purchase_orders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "purchase",
                        principalTable: "purchase_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_AccountType",
                schema: "finance",
                table: "accounts",
                column: "AccountType");

            migrationBuilder.CreateIndex(
                name: "IX_accounts_PropertyId_AccountCode",
                schema: "finance",
                table: "accounts",
                columns: new[] { "PropertyId", "AccountCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assets_Category",
                schema: "maintenance",
                table: "assets",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_assets_PropertyId_AssetCode",
                schema: "maintenance",
                table: "assets",
                columns: new[] { "PropertyId", "AssetCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assets_Status",
                schema: "maintenance",
                table: "assets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_records_Date",
                schema: "hr",
                table: "attendance_records",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_records_EmployeeId",
                schema: "hr",
                table: "attendance_records",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_records_PropertyId_EmployeeId_Date",
                schema: "hr",
                table: "attendance_records",
                columns: new[] { "PropertyId", "EmployeeId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_DepartmentCode",
                schema: "hr",
                table: "employees",
                column: "DepartmentCode");

            migrationBuilder.CreateIndex(
                name: "IX_employees_PropertyId_EmployeeCode",
                schema: "hr",
                table: "employees",
                columns: new[] { "PropertyId", "EmployeeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_Status",
                schema: "hr",
                table: "employees",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_employees_UserId",
                schema: "hr",
                table: "employees",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_incident_reports_PropertyId_IncidentNumber",
                schema: "security",
                table: "incident_reports",
                columns: new[] { "PropertyId", "IncidentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_incident_reports_ReportedByUserId",
                schema: "security",
                table: "incident_reports",
                column: "ReportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_incident_reports_Severity",
                schema: "security",
                table: "incident_reports",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_incident_reports_Status",
                schema: "security",
                table: "incident_reports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_items_Category",
                schema: "inventory",
                table: "inventory_items",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_items_PropertyId_ItemCode",
                schema: "inventory",
                table: "inventory_items",
                columns: new[] { "PropertyId", "ItemCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_PropertyId_InvoiceNumber",
                schema: "finance",
                table: "invoices",
                columns: new[] { "PropertyId", "InvoiceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_Status",
                schema: "finance",
                table: "invoices",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_PropertyId_EntryNumber",
                schema: "finance",
                table: "journal_entries",
                columns: new[] { "PropertyId", "EntryNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_journal_lines_AccountId",
                schema: "finance",
                table: "journal_lines",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_journal_lines_JournalEntryId",
                schema: "finance",
                table: "journal_lines",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_items_PurchaseOrderId",
                schema: "purchase",
                table: "purchase_order_items",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_PropertyId_OrderNumber",
                schema: "purchase",
                table: "purchase_orders",
                columns: new[] { "PropertyId", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_PurchaseRequisitionId",
                schema: "purchase",
                table: "purchase_orders",
                column: "PurchaseRequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_Status",
                schema: "purchase",
                table: "purchase_orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_VendorId",
                schema: "purchase",
                table: "purchase_orders",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_requisition_items_PurchaseRequisitionId",
                schema: "purchase",
                table: "purchase_requisition_items",
                column: "PurchaseRequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_requisitions_PropertyId_RequisitionNumber",
                schema: "purchase",
                table: "purchase_requisitions",
                columns: new[] { "PropertyId", "RequisitionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_requisitions_RequestedByUserId",
                schema: "purchase",
                table: "purchase_requisitions",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_requisitions_Status",
                schema: "purchase",
                table: "purchase_requisitions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transactions_InventoryItemId",
                schema: "inventory",
                table: "stock_transactions",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transactions_PropertyId_TransactionNumber",
                schema: "inventory",
                table: "stock_transactions",
                columns: new[] { "PropertyId", "TransactionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_transactions_TransactionType",
                schema: "inventory",
                table: "stock_transactions",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_vendors_PropertyId_VendorCode",
                schema: "purchase",
                table: "vendors",
                columns: new[] { "PropertyId", "VendorCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vendors_Status",
                schema: "purchase",
                table: "vendors",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_logs_PropertyId_PassNumber",
                schema: "security",
                table: "visitor_logs",
                columns: new[] { "PropertyId", "PassNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_visitor_logs_Status",
                schema: "security",
                table: "visitor_logs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_AssetId",
                schema: "maintenance",
                table: "work_orders",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_AssignedToUserId",
                schema: "maintenance",
                table: "work_orders",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_PropertyId_WorkOrderNumber",
                schema: "maintenance",
                table: "work_orders",
                columns: new[] { "PropertyId", "WorkOrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_RoomId",
                schema: "maintenance",
                table: "work_orders",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_Status",
                schema: "maintenance",
                table: "work_orders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance_records",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "incident_reports",
                schema: "security");

            migrationBuilder.DropTable(
                name: "invoices",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "journal_lines",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "purchase_order_items",
                schema: "purchase");

            migrationBuilder.DropTable(
                name: "purchase_requisition_items",
                schema: "purchase");

            migrationBuilder.DropTable(
                name: "stock_transactions",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "visitor_logs",
                schema: "security");

            migrationBuilder.DropTable(
                name: "work_orders",
                schema: "maintenance");

            migrationBuilder.DropTable(
                name: "employees",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "accounts",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "journal_entries",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "purchase_orders",
                schema: "purchase");

            migrationBuilder.DropTable(
                name: "inventory_items",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "assets",
                schema: "maintenance");

            migrationBuilder.DropTable(
                name: "purchase_requisitions",
                schema: "purchase");

            migrationBuilder.DropTable(
                name: "vendors",
                schema: "purchase");
        }
    }
}

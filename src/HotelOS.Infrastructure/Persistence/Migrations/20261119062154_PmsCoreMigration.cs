using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HotelOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PmsCoreMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pms");

            migrationBuilder.CreateTable(
                name: "guests",
                schema: "pms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IdDocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IdDocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VipStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Standard"),
                    TotalStays = table.Column<int>(type: "integer", nullable: false),
                    LifetimeSpend = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SpecialPreferences = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_guests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                schema: "pms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    RoomNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FloorNumber = table.Column<int>(type: "integer", nullable: false),
                    RoomTypeCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RoomStatusCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "CLN"),
                    MaxOccupancy = table.Column<int>(type: "integer", nullable: false),
                    BaseRatePerNight = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsSmoking = table.Column<bool>(type: "boolean", nullable: false),
                    KeyCardUid = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExtraDataJson = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reservations",
                schema: "pms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    ConfirmationNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    GuestId = table.Column<long>(type: "bigint", nullable: false),
                    RoomTypeCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssignedRoomId = table.Column<long>(type: "bigint", nullable: true),
                    CheckInDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckOutDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AdultsCount = table.Column<int>(type: "integer", nullable: false),
                    ChildrenCount = table.Column<int>(type: "integer", nullable: false),
                    ReservationStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "CONF"),
                    BookingSource = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "DIRECT"),
                    RatePerNight = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalEstimatedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SpecialRequests = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ActualCheckInAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActualCheckOutAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CheckedInBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CheckedOutBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reservations_guests_GuestId",
                        column: x => x.GuestId,
                        principalSchema: "pms",
                        principalTable: "guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservations_rooms_AssignedRoomId",
                        column: x => x.AssignedRoomId,
                        principalSchema: "pms",
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "folios",
                schema: "pms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    ReservationId = table.Column<long>(type: "bigint", nullable: false),
                    GuestId = table.Column<long>(type: "bigint", nullable: false),
                    FolioNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Open"),
                    TotalCharges = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPayments = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_folios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_folios_guests_GuestId",
                        column: x => x.GuestId,
                        principalSchema: "pms",
                        principalTable: "guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_folios_reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalSchema: "pms",
                        principalTable: "reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "folio_items",
                schema: "pms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FolioId = table.Column<long>(type: "bigint", nullable: false),
                    ItemType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DepartmentCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentModeCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    TransactionReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PostedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PostedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_folio_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_folio_items_folios_FolioId",
                        column: x => x.FolioId,
                        principalSchema: "pms",
                        principalTable: "folios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_folio_items_FolioId",
                schema: "pms",
                table: "folio_items",
                column: "FolioId");

            migrationBuilder.CreateIndex(
                name: "IX_folios_GuestId",
                schema: "pms",
                table: "folios",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_folios_PropertyId_FolioNumber",
                schema: "pms",
                table: "folios",
                columns: new[] { "PropertyId", "FolioNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_folios_ReservationId",
                schema: "pms",
                table: "folios",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_folios_Status",
                schema: "pms",
                table: "folios",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_guests_PropertyId_Email",
                schema: "pms",
                table: "guests",
                columns: new[] { "PropertyId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_guests_PropertyId_Phone",
                schema: "pms",
                table: "guests",
                columns: new[] { "PropertyId", "Phone" });

            migrationBuilder.CreateIndex(
                name: "IX_reservations_AssignedRoomId",
                schema: "pms",
                table: "reservations",
                column: "AssignedRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_reservations_CheckInDate_CheckOutDate",
                schema: "pms",
                table: "reservations",
                columns: new[] { "CheckInDate", "CheckOutDate" });

            migrationBuilder.CreateIndex(
                name: "IX_reservations_GuestId",
                schema: "pms",
                table: "reservations",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_reservations_PropertyId_ConfirmationNumber",
                schema: "pms",
                table: "reservations",
                columns: new[] { "PropertyId", "ConfirmationNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservations_ReservationStatus",
                schema: "pms",
                table: "reservations",
                column: "ReservationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_rooms_PropertyId_RoomNumber",
                schema: "pms",
                table: "rooms",
                columns: new[] { "PropertyId", "RoomNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rooms_RoomStatusCode",
                schema: "pms",
                table: "rooms",
                column: "RoomStatusCode");

            migrationBuilder.CreateIndex(
                name: "IX_rooms_RoomTypeCode",
                schema: "pms",
                table: "rooms",
                column: "RoomTypeCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "folio_items",
                schema: "pms");

            migrationBuilder.DropTable(
                name: "folios",
                schema: "pms");

            migrationBuilder.DropTable(
                name: "reservations",
                schema: "pms");

            migrationBuilder.DropTable(
                name: "guests",
                schema: "pms");

            migrationBuilder.DropTable(
                name: "rooms",
                schema: "pms");
        }
    }
}

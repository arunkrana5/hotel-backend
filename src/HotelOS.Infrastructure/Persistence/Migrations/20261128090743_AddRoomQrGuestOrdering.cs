using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HotelOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomQrGuestOrdering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AcceptedAt",
                schema: "restaurant",
                table: "restaurant_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeliveredAt",
                schema: "restaurant",
                table: "restaurant_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryContactPhone",
                schema: "restaurant",
                table: "restaurant_orders",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryStaffName",
                schema: "restaurant",
                table: "restaurant_orders",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDeliveryMinutes",
                schema: "restaurant",
                table: "restaurant_orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestDisplayName",
                schema: "restaurant",
                table: "restaurant_orders",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GuestOrderingSessionId",
                schema: "restaurant",
                table: "restaurant_orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStayVerified",
                schema: "restaurant",
                table: "restaurant_orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                schema: "restaurant",
                table: "restaurant_orders",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "STAFF");

            migrationBuilder.CreateTable(
                name: "room_qr_codes",
                schema: "restaurant",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    RoomId = table.Column<long>(type: "bigint", nullable: false),
                    QrToken = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PinCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    RequirePin = table.Column<bool>(type: "boolean", nullable: false),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    GeneratedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ScanCount = table.Column<int>(type: "integer", nullable: false),
                    LastScannedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_room_qr_codes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_room_qr_codes_rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "pms",
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "guest_ordering_sessions",
                schema: "restaurant",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    RoomId = table.Column<long>(type: "bigint", nullable: false),
                    RoomQrCodeId = table.Column<long>(type: "bigint", nullable: false),
                    SessionToken = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ReservationId = table.Column<long>(type: "bigint", nullable: true),
                    GuestId = table.Column<long>(type: "bigint", nullable: true),
                    GuestName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    GuestPhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    IsStayVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerificationMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "AUTO"),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastActivityAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_guest_ordering_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_guest_ordering_sessions_guests_GuestId",
                        column: x => x.GuestId,
                        principalSchema: "pms",
                        principalTable: "guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_guest_ordering_sessions_reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalSchema: "pms",
                        principalTable: "reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_guest_ordering_sessions_room_qr_codes_RoomQrCodeId",
                        column: x => x.RoomQrCodeId,
                        principalSchema: "restaurant",
                        principalTable: "room_qr_codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_guest_ordering_sessions_rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "pms",
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_restaurant_orders_GuestOrderingSessionId",
                schema: "restaurant",
                table: "restaurant_orders",
                column: "GuestOrderingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_document_number_audits_DocumentNumber",
                schema: "configuration",
                table: "document_number_audits",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_document_number_audits_PropertyId_DocumentType",
                schema: "configuration",
                table: "document_number_audits",
                columns: new[] { "PropertyId", "DocumentType" });

            migrationBuilder.CreateIndex(
                name: "IX_document_number_configurations_PropertyId_DocumentType",
                schema: "configuration",
                table: "document_number_configurations",
                columns: new[] { "PropertyId", "DocumentType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_guest_ordering_sessions_GuestId",
                schema: "restaurant",
                table: "guest_ordering_sessions",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_guest_ordering_sessions_PropertyId_RoomId",
                schema: "restaurant",
                table: "guest_ordering_sessions",
                columns: new[] { "PropertyId", "RoomId" });

            migrationBuilder.CreateIndex(
                name: "IX_guest_ordering_sessions_ReservationId",
                schema: "restaurant",
                table: "guest_ordering_sessions",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_guest_ordering_sessions_RoomId",
                schema: "restaurant",
                table: "guest_ordering_sessions",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_guest_ordering_sessions_RoomQrCodeId",
                schema: "restaurant",
                table: "guest_ordering_sessions",
                column: "RoomQrCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_guest_ordering_sessions_SessionToken",
                schema: "restaurant",
                table: "guest_ordering_sessions",
                column: "SessionToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_room_qr_codes_PropertyId_RoomId",
                schema: "restaurant",
                table: "room_qr_codes",
                columns: new[] { "PropertyId", "RoomId" });

            migrationBuilder.CreateIndex(
                name: "IX_room_qr_codes_QrToken",
                schema: "restaurant",
                table: "room_qr_codes",
                column: "QrToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_room_qr_codes_RoomId",
                schema: "restaurant",
                table: "room_qr_codes",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_restaurant_orders_guest_ordering_sessions_GuestOrderingSess~",
                schema: "restaurant",
                table: "restaurant_orders",
                column: "GuestOrderingSessionId",
                principalSchema: "restaurant",
                principalTable: "guest_ordering_sessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_restaurant_orders_guest_ordering_sessions_GuestOrderingSess~",
                schema: "restaurant",
                table: "restaurant_orders");

            migrationBuilder.DropTable(
                name: "guest_ordering_sessions",
                schema: "restaurant");

            migrationBuilder.DropTable(
                name: "room_qr_codes",
                schema: "restaurant");

            migrationBuilder.DropIndex(
                name: "IX_restaurant_orders_GuestOrderingSessionId",
                schema: "restaurant",
                table: "restaurant_orders");

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                schema: "restaurant",
                table: "restaurant_orders");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                schema: "restaurant",
                table: "restaurant_orders");

            migrationBuilder.DropColumn(
                name: "DeliveryContactPhone",
                schema: "restaurant",
                table: "restaurant_orders");

            migrationBuilder.DropColumn(
                name: "DeliveryStaffName",
                schema: "restaurant",
                table: "restaurant_orders");

            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryMinutes",
                schema: "restaurant",
                table: "restaurant_orders");

            migrationBuilder.DropColumn(
                name: "GuestDisplayName",
                schema: "restaurant",
                table: "restaurant_orders");

            migrationBuilder.DropColumn(
                name: "GuestOrderingSessionId",
                schema: "restaurant",
                table: "restaurant_orders");

            migrationBuilder.DropColumn(
                name: "IsStayVerified",
                schema: "restaurant",
                table: "restaurant_orders");

            migrationBuilder.DropColumn(
                name: "Source",
                schema: "restaurant",
                table: "restaurant_orders");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HotelOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicMenuManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                schema: "restaurant",
                table: "restaurant_order_items",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OptionsPrice",
                schema: "restaurant",
                table: "restaurant_order_items",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SelectedAddOnsJson",
                schema: "restaurant",
                table: "restaurant_order_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedVariantsJson",
                schema: "restaurant",
                table: "restaurant_order_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddOnsJson",
                schema: "restaurant",
                table: "menu_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Allergens",
                schema: "restaurant",
                table: "menu_items",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicableOutlets",
                schema: "restaurant",
                table: "menu_items",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "ROOM_SERVICE,MAIN_RESTAURANT");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                schema: "restaurant",
                table: "menu_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                schema: "restaurant",
                table: "menu_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGuestVisible",
                schema: "restaurant",
                table: "menu_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "MenuCategoryId",
                schema: "restaurant",
                table: "menu_items",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MenuId",
                schema: "restaurant",
                table: "menu_items",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "restaurant",
                table: "menu_items",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "PUBLISHED");

            migrationBuilder.AddColumn<decimal>(
                name: "TaxPercent",
                schema: "restaurant",
                table: "menu_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 5.0m);

            migrationBuilder.AddColumn<string>(
                name: "VariantsJson",
                schema: "restaurant",
                table: "menu_items",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "menus",
                schema: "restaurant",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    MenuCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MenuName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MenuType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "ALL_DAY"),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "PUBLISHED"),
                    IsGuestVisible = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayPriority = table.Column<int>(type: "integer", nullable: false),
                    AvailableDays = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "MON,TUE,WED,THU,FRI,SAT,SUN"),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EffectiveUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApplicableOutlets = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, defaultValue: "ROOM_SERVICE,MAIN_RESTAURANT"),
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
                    table.PrimaryKey("PK_menus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "menu_categories",
                schema: "restaurant",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    MenuId = table.Column<long>(type: "bigint", nullable: true),
                    CategoryCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CategoryName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsGuestVisible = table.Column<bool>(type: "boolean", nullable: false),
                    ApplicableOutlets = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, defaultValue: "ROOM_SERVICE,MAIN_RESTAURANT"),
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
                    table.PrimaryKey("PK_menu_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_categories_menus_MenuId",
                        column: x => x.MenuId,
                        principalSchema: "restaurant",
                        principalTable: "menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_IsFeatured",
                schema: "restaurant",
                table: "menu_items",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_IsGuestVisible",
                schema: "restaurant",
                table: "menu_items",
                column: "IsGuestVisible");

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_MenuCategoryId",
                schema: "restaurant",
                table: "menu_items",
                column: "MenuCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_MenuId",
                schema: "restaurant",
                table: "menu_items",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_menu_categories_DisplayOrder",
                schema: "restaurant",
                table: "menu_categories",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_menu_categories_IsGuestVisible",
                schema: "restaurant",
                table: "menu_categories",
                column: "IsGuestVisible");

            migrationBuilder.CreateIndex(
                name: "IX_menu_categories_MenuId",
                schema: "restaurant",
                table: "menu_categories",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_menu_categories_PropertyId_CategoryCode",
                schema: "restaurant",
                table: "menu_categories",
                columns: new[] { "PropertyId", "CategoryCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_menus_IsGuestVisible",
                schema: "restaurant",
                table: "menus",
                column: "IsGuestVisible");

            migrationBuilder.CreateIndex(
                name: "IX_menus_PropertyId_MenuCode",
                schema: "restaurant",
                table: "menus",
                columns: new[] { "PropertyId", "MenuCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_menus_Status",
                schema: "restaurant",
                table: "menus",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_menu_items_menu_categories_MenuCategoryId",
                schema: "restaurant",
                table: "menu_items",
                column: "MenuCategoryId",
                principalSchema: "restaurant",
                principalTable: "menu_categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_menu_items_menus_MenuId",
                schema: "restaurant",
                table: "menu_items",
                column: "MenuId",
                principalSchema: "restaurant",
                principalTable: "menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_menu_items_menu_categories_MenuCategoryId",
                schema: "restaurant",
                table: "menu_items");

            migrationBuilder.DropForeignKey(
                name: "FK_menu_items_menus_MenuId",
                schema: "restaurant",
                table: "menu_items");

            migrationBuilder.DropTable(
                name: "menu_categories",
                schema: "restaurant");

            migrationBuilder.DropTable(
                name: "menus",
                schema: "restaurant");

            migrationBuilder.DropIndex(
                name: "IX_menu_items_IsFeatured",
                schema: "restaurant",
                table: "menu_items");

            migrationBuilder.DropIndex(
                name: "IX_menu_items_IsGuestVisible",
                schema: "restaurant",
                table: "menu_items");

            migrationBuilder.DropIndex(
                name: "IX_menu_items_MenuCategoryId",
                schema: "restaurant",
                table: "menu_items");

            migrationBuilder.DropIndex(
                name: "IX_menu_items_MenuId",
                schema: "restaurant",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                schema: "restaurant",
                table: "restaurant_order_items");

            migrationBuilder.DropColumn(
                name: "OptionsPrice",
                schema: "restaurant",
                table: "restaurant_order_items");

            migrationBuilder.DropColumn(
                name: "SelectedAddOnsJson",
                schema: "restaurant",
                table: "restaurant_order_items");

            migrationBuilder.DropColumn(
                name: "SelectedVariantsJson",
                schema: "restaurant",
                table: "restaurant_order_items");

            migrationBuilder.DropColumn(
                name: "AddOnsJson",
                schema: "restaurant",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "Allergens",
                schema: "restaurant",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "ApplicableOutlets",
                schema: "restaurant",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                schema: "restaurant",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                schema: "restaurant",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "IsGuestVisible",
                schema: "restaurant",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "MenuCategoryId",
                schema: "restaurant",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "MenuId",
                schema: "restaurant",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "restaurant",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "TaxPercent",
                schema: "restaurant",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "VariantsJson",
                schema: "restaurant",
                table: "menu_items");
        }
    }
}

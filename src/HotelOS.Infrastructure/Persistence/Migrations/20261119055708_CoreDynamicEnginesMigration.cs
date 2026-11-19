using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HotelOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CoreDynamicEnginesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "configuration");

            migrationBuilder.EnsureSchema(
                name: "masters");

            migrationBuilder.EnsureSchema(
                name: "workflow");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ChangesJson = table.Column<string>(type: "jsonb", nullable: true),
                    PerformedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PerformedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    PropertyId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dynamic_form_definitions",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_dynamic_form_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "master_categories",
                schema: "masters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_master_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "navigation_menus",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Path = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    RequiredPermission = table.Column<string>(type: "text", nullable: true),
                    RequiredRole = table.Column<string>(type: "text", nullable: true),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BadgeText = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_navigation_menus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_navigation_menus_navigation_menus_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "configuration",
                        principalTable: "navigation_menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflow_definitions",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    PropertyId = table.Column<long>(type: "bigint", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_workflow_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dynamic_form_fields",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FormDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    FieldKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FieldLabel = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FieldType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    MasterDefinitionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DefaultValue = table.Column<string>(type: "text", nullable: true),
                    Placeholder = table.Column<string>(type: "text", nullable: true),
                    ValidationRulesJson = table.Column<string>(type: "jsonb", nullable: true),
                    VisibilityRuleJson = table.Column<string>(type: "jsonb", nullable: true),
                    SectionName = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_dynamic_form_fields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dynamic_form_fields_dynamic_form_definitions_FormDefinition~",
                        column: x => x.FormDefinitionId,
                        principalSchema: "configuration",
                        principalTable: "dynamic_form_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "master_definitions",
                schema: "masters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ParentDefinitionId = table.Column<long>(type: "bigint", nullable: true),
                    IsPropertySpecific = table.Column<bool>(type: "boolean", nullable: false),
                    IsHierarchical = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_master_definitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_master_definitions_master_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "masters",
                        principalTable: "master_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_master_definitions_master_definitions_ParentDefinitionId",
                        column: x => x.ParentDefinitionId,
                        principalSchema: "masters",
                        principalTable: "master_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflow_steps",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkflowDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    StepName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    StepType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssignedRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AssignedDepartmentId = table.Column<long>(type: "bigint", nullable: true),
                    RequiredApprovalCount = table.Column<int>(type: "integer", nullable: false),
                    SlaMinutes = table.Column<int>(type: "integer", nullable: false),
                    TimeoutAction = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_workflow_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_steps_workflow_definitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalSchema: "workflow",
                        principalTable: "workflow_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "master_values",
                schema: "masters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MasterDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    PropertyId = table.Column<long>(type: "bigint", nullable: true),
                    ParentValueId = table.Column<long>(type: "bigint", nullable: true),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NumericValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    ExtraDataJson = table.Column<string>(type: "jsonb", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_master_values", x => x.Id);
                    table.ForeignKey(
                        name: "FK_master_values_master_definitions_MasterDefinitionId",
                        column: x => x.MasterDefinitionId,
                        principalSchema: "masters",
                        principalTable: "master_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_master_values_master_values_ParentValueId",
                        column: x => x.ParentValueId,
                        principalSchema: "masters",
                        principalTable: "master_values",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflow_instances",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkflowDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    WorkflowVersion = table.Column<int>(type: "integer", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<long>(type: "bigint", nullable: false),
                    CurrentStepId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InitiatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    InitiatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_workflow_instances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_instances_workflow_definitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalSchema: "workflow",
                        principalTable: "workflow_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workflow_instances_workflow_steps_CurrentStepId",
                        column: x => x.CurrentStepId,
                        principalSchema: "workflow",
                        principalTable: "workflow_steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflow_transitions",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkflowDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    FromStepId = table.Column<long>(type: "bigint", nullable: false),
                    ToStepId = table.Column<long>(type: "bigint", nullable: false),
                    ActionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConditionJson = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_workflow_transitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_transitions_workflow_definitions_WorkflowDefinitio~",
                        column: x => x.WorkflowDefinitionId,
                        principalSchema: "workflow",
                        principalTable: "workflow_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_transitions_workflow_steps_FromStepId",
                        column: x => x.FromStepId,
                        principalSchema: "workflow",
                        principalTable: "workflow_steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workflow_transitions_workflow_steps_ToStepId",
                        column: x => x.ToStepId,
                        principalSchema: "workflow",
                        principalTable: "workflow_steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflow_histories",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkflowInstanceId = table.Column<long>(type: "bigint", nullable: false),
                    StepId = table.Column<long>(type: "bigint", nullable: false),
                    ActionTaken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActionByUserId = table.Column<long>(type: "bigint", nullable: false),
                    ActionAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PreviousStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NewStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_histories_workflow_instances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalSchema: "workflow",
                        principalTable: "workflow_instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_histories_workflow_steps_StepId",
                        column: x => x.StepId,
                        principalSchema: "workflow",
                        principalTable: "workflow_steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EntityName_EntityId",
                schema: "audit",
                table: "audit_logs",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_PerformedAt",
                schema: "audit",
                table: "audit_logs",
                column: "PerformedAt");

            migrationBuilder.CreateIndex(
                name: "IX_dynamic_form_definitions_Code_Version",
                schema: "configuration",
                table: "dynamic_form_definitions",
                columns: new[] { "Code", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dynamic_form_fields_FormDefinitionId",
                schema: "configuration",
                table: "dynamic_form_fields",
                column: "FormDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_master_categories_Code",
                schema: "masters",
                table: "master_categories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_master_definitions_CategoryId",
                schema: "masters",
                table: "master_definitions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_master_definitions_Code",
                schema: "masters",
                table: "master_definitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_master_definitions_ParentDefinitionId",
                schema: "masters",
                table: "master_definitions",
                column: "ParentDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_master_values_MasterDefinitionId_IsActive",
                schema: "masters",
                table: "master_values",
                columns: new[] { "MasterDefinitionId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_master_values_MasterDefinitionId_PropertyId_Code",
                schema: "masters",
                table: "master_values",
                columns: new[] { "MasterDefinitionId", "PropertyId", "Code" });

            migrationBuilder.CreateIndex(
                name: "IX_master_values_ParentValueId",
                schema: "masters",
                table: "master_values",
                column: "ParentValueId");

            migrationBuilder.CreateIndex(
                name: "IX_navigation_menus_Code",
                schema: "configuration",
                table: "navigation_menus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_navigation_menus_ParentId",
                schema: "configuration",
                table: "navigation_menus",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definitions_Code_Version",
                schema: "workflow",
                table: "workflow_definitions",
                columns: new[] { "Code", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_histories_StepId",
                schema: "workflow",
                table: "workflow_histories",
                column: "StepId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_histories_WorkflowInstanceId",
                schema: "workflow",
                table: "workflow_histories",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instances_CurrentStepId",
                schema: "workflow",
                table: "workflow_instances",
                column: "CurrentStepId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instances_EntityType_EntityId",
                schema: "workflow",
                table: "workflow_instances",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instances_WorkflowDefinitionId",
                schema: "workflow",
                table: "workflow_instances",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_steps_WorkflowDefinitionId",
                schema: "workflow",
                table: "workflow_steps",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_transitions_FromStepId",
                schema: "workflow",
                table: "workflow_transitions",
                column: "FromStepId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_transitions_ToStepId",
                schema: "workflow",
                table: "workflow_transitions",
                column: "ToStepId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_transitions_WorkflowDefinitionId",
                schema: "workflow",
                table: "workflow_transitions",
                column: "WorkflowDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "dynamic_form_fields",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "master_values",
                schema: "masters");

            migrationBuilder.DropTable(
                name: "navigation_menus",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "workflow_histories",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "workflow_transitions",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "dynamic_form_definitions",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "master_definitions",
                schema: "masters");

            migrationBuilder.DropTable(
                name: "workflow_instances",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "master_categories",
                schema: "masters");

            migrationBuilder.DropTable(
                name: "workflow_steps",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "workflow_definitions",
                schema: "workflow");
        }
    }
}

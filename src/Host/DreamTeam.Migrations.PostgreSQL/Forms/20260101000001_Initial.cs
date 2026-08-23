using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace DreamTeam.Migrations.PostgreSQL.Forms;

/// <summary>
/// Forms module — initial schema. Creates the four FDS entities (ProcessTemplate,
/// FormVersion, ProcessInstance, Submission) with their indexes. All tables
/// live in the <c>forms</c> Postgres schema.
///
/// Hand-written for MVP-1 because dotnet-ef isn't installed in the prep
/// environment. Replace with `dotnet ef migrations add InitialSchema
/// --context FormsDbContext --output-dir Forms` once the tooling is in
/// place; the resulting migration will match this one.
///
/// All entities carry <c>TenantId</c> and audit columns (CreatedOnUtc,
/// CreatedBy, LastModifiedOnUtc, LastModifiedBy) — the base DbContext's
/// default-on tenant-isolation query filter and the
/// AuditableEntitySaveChangesInterceptor handle the writes at runtime.
/// </summary>
public partial class Initial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "forms");

        // =============================================================
        // ProcessTemplates — semantic wrapper for a recurring ritual
        // =============================================================
        migrationBuilder.CreateTable(
            name: "ProcessTemplates",
            schema: "forms",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                OwnerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                IsArchived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                DeletedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                DeletedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                LastModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                LastModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProcessTemplates", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProcessTemplates_TenantId_Slug",
            schema: "forms",
            table: "ProcessTemplates",
            columns: new[] { "TenantId", "Slug" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ProcessTemplates_TenantId_IsDeleted",
            schema: "forms",
            table: "ProcessTemplates",
            columns: new[] { "TenantId", "IsDeleted" });

        // =============================================================
        // FormVersions — immutable schema snapshot
        // =============================================================
        migrationBuilder.CreateTable(
            name: "FormVersions",
            schema: "forms",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ProcessTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                VersionNumber = table.Column<int>(type: "integer", nullable: false),
                Schema = table.Column<string>(type: "jsonb", nullable: false),
                Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                IsCurrent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                PublishedById = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                LastModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                LastModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FormVersions", x => x.Id);
                table.ForeignKey(
                    "FK_FormVersions_ProcessTemplates_ProcessTemplateId",
                    x => x.ProcessTemplateId,
                    principalSchema: "forms",
                    principalTable: "ProcessTemplates",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_FormVersions_TenantId_ProcessTemplateId_VersionNumber",
            schema: "forms",
            table: "FormVersions",
            columns: new[] { "TenantId", "ProcessTemplateId", "VersionNumber" },
            unique: true);

        // At most one IsCurrent=true per template. The migration's
        // HasFilter translates to a partial unique index in Postgres.
        migrationBuilder.CreateIndex(
            name: "IX_FormVersions_TenantId_ProcessTemplateId_IsCurrent",
            schema: "forms",
            table: "FormVersions",
            columns: new[] { "TenantId", "ProcessTemplateId", "IsCurrent" },
            unique: true,
            filter: "\"IsCurrent\" = true");

        // =============================================================
        // ProcessInstances — one occurrence of a template
        // =============================================================
        migrationBuilder.CreateTable(
            name: "ProcessInstances",
            schema: "forms",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                FormVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                PairUserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                LastModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                LastModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProcessInstances", x => x.Id);
                table.ForeignKey(
                    "FK_ProcessInstances_FormVersions_FormVersionId",
                    x => x.FormVersionId,
                    principalSchema: "forms",
                    principalTable: "FormVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProcessInstances_TenantId_ScheduledAt",
            schema: "forms",
            table: "ProcessInstances",
            columns: new[] { "TenantId", "ScheduledAt" });

        migrationBuilder.CreateIndex(
            name: "IX_ProcessInstances_TenantId_PairUserId_ScheduledAt",
            schema: "forms",
            table: "ProcessInstances",
            columns: new[] { "TenantId", "PairUserId", "ScheduledAt" });

        migrationBuilder.CreateIndex(
            name: "IX_ProcessInstances_TenantId_Status",
            schema: "forms",
            table: "ProcessInstances",
            columns: new[] { "TenantId", "Status" });

        // =============================================================
        // Submissions — append-only response
        // =============================================================
        migrationBuilder.CreateTable(
            name: "Submissions",
            schema: "forms",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ProcessInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                FormVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                AuthorId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Data = table.Column<string>(type: "jsonb", nullable: false),
                IsCompensating = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                CompensatesSubmissionId = table.Column<Guid>(type: "uuid", nullable: true),
                CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                LastModifiedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                LastModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Submissions", x => x.Id);
                table.ForeignKey(
                    "FK_Submissions_ProcessInstances_ProcessInstanceId",
                    x => x.ProcessInstanceId,
                    principalSchema: "forms",
                    principalTable: "ProcessInstances",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Submissions_TenantId_ProcessInstanceId_AuthorId",
            schema: "forms",
            table: "Submissions",
            columns: new[] { "TenantId", "ProcessInstanceId", "AuthorId" });

        migrationBuilder.CreateIndex(
            name: "IX_Submissions_TenantId_CompensatesSubmissionId",
            schema: "forms",
            table: "Submissions",
            columns: new[] { "TenantId", "CompensatesSubmissionId" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop in reverse FK order.
        migrationBuilder.DropTable(name: "Submissions", schema: "forms");
        migrationBuilder.DropTable(name: "ProcessInstances", schema: "forms");
        migrationBuilder.DropTable(name: "FormVersions", schema: "forms");
        migrationBuilder.DropTable(name: "ProcessTemplates", schema: "forms");
    }
}

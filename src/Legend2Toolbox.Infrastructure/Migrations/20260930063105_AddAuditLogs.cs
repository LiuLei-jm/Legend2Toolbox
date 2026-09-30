using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Legend2Toolbox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TraceId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    OccurredAtUnixMs = table.Column<long>(type: "INTEGER", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ActorUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    SubjectUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ActorType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Module = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TargetId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    TargetName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ClientIp = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    PeerIp = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Outcome = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    DataStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ErrorCode = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    CaptureIncomplete = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AuditLogId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    EntityId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ChangeType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    OldValues = table.Column<string>(type: "TEXT", nullable: false),
                    NewValues = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogDetails_AuditLogs_AuditLogId",
                        column: x => x.AuditLogId,
                        principalTable: "AuditLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogDetails_AuditLogId_Sequence",
                table: "AuditLogDetails",
                columns: new[] { "AuditLogId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActorUserId_OccurredAtUnixMs",
                table: "AuditLogs",
                columns: new[] { "ActorUserId", "OccurredAtUnixMs" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Module_TargetId_OccurredAtUnixMs",
                table: "AuditLogs",
                columns: new[] { "Module", "TargetId", "OccurredAtUnixMs" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OccurredAtUnixMs",
                table: "AuditLogs",
                column: "OccurredAtUnixMs");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_SubjectUserId_Action_OccurredAtUnixMs",
                table: "AuditLogs",
                columns: new[] { "SubjectUserId", "Action", "OccurredAtUnixMs" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogDetails");

            migrationBuilder.DropTable(
                name: "AuditLogs");
        }
    }
}

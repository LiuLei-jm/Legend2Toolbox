using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Legend2Toolbox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class newDbTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaterialFile_ScriptSet_ScriptSetId",
                table: "MaterialFile");

            migrationBuilder.DropForeignKey(
                name: "FK_ScriptFile_ScriptSet_ScriptSetId",
                table: "ScriptFile");

            migrationBuilder.DropForeignKey(
                name: "FK_ScriptSegment_ScriptFile_ScriptFileId",
                table: "ScriptSegment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScriptSet",
                table: "ScriptSet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScriptSegment",
                table: "ScriptSegment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScriptFile",
                table: "ScriptFile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MaterialFile",
                table: "MaterialFile");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ScriptSet");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ScriptSet");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "ScriptSet");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "ScriptSet");

            migrationBuilder.RenameTable(
                name: "ScriptSet",
                newName: "ScriptSets");

            migrationBuilder.RenameTable(
                name: "ScriptSegment",
                newName: "ScriptSegments");

            migrationBuilder.RenameTable(
                name: "ScriptFile",
                newName: "ScriptFiles");

            migrationBuilder.RenameTable(
                name: "MaterialFile",
                newName: "MaterialFiles");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "ScriptSets",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ScriptSegment_ScriptFileId",
                table: "ScriptSegments",
                newName: "IX_ScriptSegments_ScriptFileId");

            migrationBuilder.RenameIndex(
                name: "IX_ScriptFile_ScriptSetId",
                table: "ScriptFiles",
                newName: "IX_ScriptFiles_ScriptSetId");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "MaterialFiles",
                newName: "TargetPath");

            migrationBuilder.RenameIndex(
                name: "IX_MaterialFile_ScriptSetId",
                table: "MaterialFiles",
                newName: "IX_MaterialFiles_ScriptSetId");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "MaterialFiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "StoragePath",
                table: "MaterialFiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScriptSets",
                table: "ScriptSets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScriptSegments",
                table: "ScriptSegments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScriptFiles",
                table: "ScriptFiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MaterialFiles",
                table: "MaterialFiles",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ScriptSetDbDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScriptSetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TableType = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DataJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScriptSetDbDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScriptSetDbDatas_ScriptSets_ScriptSetId",
                        column: x => x.ScriptSetId,
                        principalTable: "ScriptSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScriptSetDbDatas_ScriptSetId_TableType",
                table: "ScriptSetDbDatas",
                columns: new[] { "ScriptSetId", "TableType" });

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialFiles_ScriptSets_ScriptSetId",
                table: "MaterialFiles",
                column: "ScriptSetId",
                principalTable: "ScriptSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ScriptFiles_ScriptSets_ScriptSetId",
                table: "ScriptFiles",
                column: "ScriptSetId",
                principalTable: "ScriptSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ScriptSegments_ScriptFiles_ScriptFileId",
                table: "ScriptSegments",
                column: "ScriptFileId",
                principalTable: "ScriptFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaterialFiles_ScriptSets_ScriptSetId",
                table: "MaterialFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ScriptFiles_ScriptSets_ScriptSetId",
                table: "ScriptFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ScriptSegments_ScriptFiles_ScriptFileId",
                table: "ScriptSegments");

            migrationBuilder.DropTable(
                name: "ScriptSetDbDatas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScriptSets",
                table: "ScriptSets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScriptSegments",
                table: "ScriptSegments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScriptFiles",
                table: "ScriptFiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MaterialFiles",
                table: "MaterialFiles");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "MaterialFiles");

            migrationBuilder.DropColumn(
                name: "StoragePath",
                table: "MaterialFiles");

            migrationBuilder.RenameTable(
                name: "ScriptSets",
                newName: "ScriptSet");

            migrationBuilder.RenameTable(
                name: "ScriptSegments",
                newName: "ScriptSegment");

            migrationBuilder.RenameTable(
                name: "ScriptFiles",
                newName: "ScriptFile");

            migrationBuilder.RenameTable(
                name: "MaterialFiles",
                newName: "MaterialFile");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ScriptSet",
                newName: "CreatedOn");

            migrationBuilder.RenameIndex(
                name: "IX_ScriptSegments_ScriptFileId",
                table: "ScriptSegment",
                newName: "IX_ScriptSegment_ScriptFileId");

            migrationBuilder.RenameIndex(
                name: "IX_ScriptFiles_ScriptSetId",
                table: "ScriptFile",
                newName: "IX_ScriptFile_ScriptSetId");

            migrationBuilder.RenameColumn(
                name: "TargetPath",
                table: "MaterialFile",
                newName: "FilePath");

            migrationBuilder.RenameIndex(
                name: "IX_MaterialFiles_ScriptSetId",
                table: "MaterialFile",
                newName: "IX_MaterialFile_ScriptSetId");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ScriptSet",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ScriptSet",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "ScriptSet",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedOn",
                table: "ScriptSet",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScriptSet",
                table: "ScriptSet",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScriptSegment",
                table: "ScriptSegment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScriptFile",
                table: "ScriptFile",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MaterialFile",
                table: "MaterialFile",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialFile_ScriptSet_ScriptSetId",
                table: "MaterialFile",
                column: "ScriptSetId",
                principalTable: "ScriptSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ScriptFile_ScriptSet_ScriptSetId",
                table: "ScriptFile",
                column: "ScriptSetId",
                principalTable: "ScriptSet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ScriptSegment_ScriptFile_ScriptFileId",
                table: "ScriptSegment",
                column: "ScriptFileId",
                principalTable: "ScriptFile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

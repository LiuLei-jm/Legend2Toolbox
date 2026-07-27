using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Legend2Toolbox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinishedCardNumberPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_CardNumberPaths_CardNubmerPathId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_SecurityKeys_SecurityKeyId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CardNubmerPathId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_SecurityKeyId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CardNubmerPathId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SecurityKeyId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "CardNumberPaths");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "SecurityKeys",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "BasePath",
                table: "CardNumberPaths",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "CardNumberPaths",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CardNumberPaths_UserId",
                table: "CardNumberPaths",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CardNumberPaths_Users_UserId",
                table: "CardNumberPaths",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardNumberPaths_Users_UserId",
                table: "CardNumberPaths");

            migrationBuilder.DropIndex(
                name: "IX_CardNumberPaths_UserId",
                table: "CardNumberPaths");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "CardNumberPaths");

            migrationBuilder.AddColumn<Guid>(
                name: "CardNubmerPathId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecurityKeyId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "SecurityKeys",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048);

            migrationBuilder.AlterColumn<string>(
                name: "BasePath",
                table: "CardNumberPaths",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "CardNumberPaths",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CardNubmerPathId",
                table: "Users",
                column: "CardNubmerPathId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SecurityKeyId",
                table: "Users",
                column: "SecurityKeyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_CardNumberPaths_CardNubmerPathId",
                table: "Users",
                column: "CardNubmerPathId",
                principalTable: "CardNumberPaths",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_SecurityKeys_SecurityKeyId",
                table: "Users",
                column: "SecurityKeyId",
                principalTable: "SecurityKeys",
                principalColumn: "Id");
        }
    }
}

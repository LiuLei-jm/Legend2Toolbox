using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Legend2Toolbox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CardNubmerPathId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastLoginAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecurityKeyId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CardNumberPaths",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BasePath = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    AllowCustomPath = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardNumberPaths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CardNumbers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerName = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DurationInDays = table.Column<int>(type: "integer", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FaceValue = table.Column<double>(type: "double precision", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Cdk = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    IsExpiredNotificationSent = table.Column<bool>(type: "boolean", nullable: false),
                    LastCheckedForConnection = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardNumbers_Users_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SecurityKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityKeys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CardNubmerPathId",
                table: "Users",
                column: "CardNubmerPathId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SecurityKeyId",
                table: "Users",
                column: "SecurityKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_CardNumbers_ApplicationUserId",
                table: "CardNumbers",
                column: "ApplicationUserId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_CardNumberPaths_CardNubmerPathId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_SecurityKeys_SecurityKeyId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "CardNumberPaths");

            migrationBuilder.DropTable(
                name: "CardNumbers");

            migrationBuilder.DropTable(
                name: "SecurityKeys");

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
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SecurityKeyId",
                table: "Users");
        }
    }
}

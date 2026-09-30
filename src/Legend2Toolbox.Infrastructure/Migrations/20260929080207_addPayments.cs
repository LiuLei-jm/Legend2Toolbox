using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Legend2Toolbox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MembershipPaymentOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DurationDays = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    TradeNo = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<string>(type: "TEXT", nullable: false),
                    PaidOn = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipPaymentOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipPaymentOrders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPaymentOrders_OrderId",
                table: "MembershipPaymentOrders",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPaymentOrders_UserId_CreatedOn",
                table: "MembershipPaymentOrders",
                columns: new[] { "UserId", "CreatedOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MembershipPaymentOrders");
        }
    }
}

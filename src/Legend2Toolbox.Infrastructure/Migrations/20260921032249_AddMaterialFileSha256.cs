using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Legend2Toolbox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialFileSha256 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sha256",
                table: "MaterialFiles",
                type: "TEXT",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sha256",
                table: "MaterialFiles");
        }
    }
}

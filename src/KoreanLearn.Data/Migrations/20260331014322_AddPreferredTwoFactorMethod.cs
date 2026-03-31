using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KoreanLearn.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredTwoFactorMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreferredTwoFactorMethod",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredTwoFactorMethod",
                table: "AspNetUsers");
        }
    }
}

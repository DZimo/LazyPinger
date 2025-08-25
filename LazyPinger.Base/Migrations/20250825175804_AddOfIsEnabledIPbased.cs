using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LazyPinger.Base.Migrations
{
    /// <inheritdoc />
    public partial class AddOfIsEnabledIPbased : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsIpBased",
                table: "DevicePings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsIpBased",
                table: "DevicePings");
        }
    }
}

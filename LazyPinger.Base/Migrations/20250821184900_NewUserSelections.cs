using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LazyPinger.Base.Migrations
{
    /// <inheritdoc />
    public partial class NewUserSelections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoRestart",
                table: "UserSelections",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AutoRestartTime",
                table: "UserSelections",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoRestart",
                table: "UserSelections");

            migrationBuilder.DropColumn(
                name: "AutoRestartTime",
                table: "UserSelections");
        }
    }
}

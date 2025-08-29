using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LazyPinger.Base.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDeviceGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserSelectionID",
                table: "DevicesGroups",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DevicesGroups_UserSelectionID",
                table: "DevicesGroups",
                column: "UserSelectionID");

            migrationBuilder.AddForeignKey(
                name: "FK_DevicesGroups_UserSelections_UserSelectionID",
                table: "DevicesGroups",
                column: "UserSelectionID",
                principalTable: "UserSelections",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DevicesGroups_UserSelections_UserSelectionID",
                table: "DevicesGroups");

            migrationBuilder.DropIndex(
                name: "IX_DevicesGroups_UserSelectionID",
                table: "DevicesGroups");

            migrationBuilder.DropColumn(
                name: "UserSelectionID",
                table: "DevicesGroups");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LazyPinger.Base.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedUserPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferences_UserSelections_UserSelectionID",
                table: "UserPreferences");

            migrationBuilder.DropIndex(
                name: "IX_UserPreferences_UserSelectionID",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "UserSelectionID",
                table: "UserPreferences");

            migrationBuilder.AddColumn<int>(
                name: "UserPreferenceID",
                table: "UserSelections",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserSelections_UserPreferenceID",
                table: "UserSelections",
                column: "UserPreferenceID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSelections_UserPreferences_UserPreferenceID",
                table: "UserSelections",
                column: "UserPreferenceID",
                principalTable: "UserPreferences",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSelections_UserPreferences_UserPreferenceID",
                table: "UserSelections");

            migrationBuilder.DropIndex(
                name: "IX_UserSelections_UserPreferenceID",
                table: "UserSelections");

            migrationBuilder.DropColumn(
                name: "UserPreferenceID",
                table: "UserSelections");

            migrationBuilder.AddColumn<int>(
                name: "UserSelectionID",
                table: "UserPreferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserSelectionID",
                table: "UserPreferences",
                column: "UserSelectionID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferences_UserSelections_UserSelectionID",
                table: "UserPreferences",
                column: "UserSelectionID",
                principalTable: "UserSelections",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

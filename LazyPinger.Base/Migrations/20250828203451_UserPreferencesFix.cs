using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LazyPinger.Base.Migrations
{
    /// <inheritdoc />
    public partial class UserPreferencesFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserSelectionID",
                table: "DevicePings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    UserSelectionID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserPreferences_UserSelections_UserSelectionID",
                        column: x => x.UserSelectionID,
                        principalTable: "UserSelections",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DevicePings_UserSelectionID",
                table: "DevicePings",
                column: "UserSelectionID");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserSelectionID",
                table: "UserPreferences",
                column: "UserSelectionID");

            migrationBuilder.AddForeignKey(
                name: "FK_DevicePings_UserSelections_UserSelectionID",
                table: "DevicePings",
                column: "UserSelectionID",
                principalTable: "UserSelections",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DevicePings_UserSelections_UserSelectionID",
                table: "DevicePings");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropIndex(
                name: "IX_DevicePings_UserSelectionID",
                table: "DevicePings");

            migrationBuilder.DropColumn(
                name: "UserSelectionID",
                table: "DevicePings");
        }
    }
}

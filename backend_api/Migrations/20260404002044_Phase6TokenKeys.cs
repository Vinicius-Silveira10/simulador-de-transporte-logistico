using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogisticsTrackingAPI.Migrations
{
    /// <inheritdoc />
    public partial class Phase6TokenKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessKey",
                table: "Players",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessKey",
                table: "Players");
        }
    }
}

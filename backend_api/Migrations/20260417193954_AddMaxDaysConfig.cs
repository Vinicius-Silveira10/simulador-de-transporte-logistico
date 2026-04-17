using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogisticsTrackingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxDaysConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxDays",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxDays",
                table: "Players");
        }
    }
}

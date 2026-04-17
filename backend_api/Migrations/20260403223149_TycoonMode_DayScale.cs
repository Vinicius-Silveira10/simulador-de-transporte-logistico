using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogisticsTrackingAPI.Migrations
{
    /// <inheritdoc />
    public partial class TycoonMode_DayScale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentDay",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentDay",
                table: "Players");
        }
    }
}

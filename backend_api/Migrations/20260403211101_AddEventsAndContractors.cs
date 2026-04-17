using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogisticsTrackingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddEventsAndContractors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContractorNPC",
                table: "Trips",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncidentLogs",
                table: "Trips",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractorNPC",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "IncidentLogs",
                table: "Trips");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogisticsTrackingAPI.Migrations
{
    /// <inheritdoc />
    public partial class ExpansionPack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasAdvancedGPS",
                table: "Players",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasBankLoan",
                table: "Players",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasPremiumTires",
                table: "Players",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LoanDebt",
                table: "Players",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasAdvancedGPS",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "HasBankLoan",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "HasPremiumTires",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "LoanDebt",
                table: "Players");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKycEligibilityUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreditScore",
                table: "CustomerEligibilityProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EmploymentType",
                table: "CustomerEligibilityProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ExistingLoanCount",
                table: "CustomerEligibilityProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ExistingMonthlyObligations",
                table: "CustomerEligibilityProfiles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditScore",
                table: "CustomerEligibilityProfiles");

            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "CustomerEligibilityProfiles");

            migrationBuilder.DropColumn(
                name: "ExistingLoanCount",
                table: "CustomerEligibilityProfiles");

            migrationBuilder.DropColumn(
                name: "ExistingMonthlyObligations",
                table: "CustomerEligibilityProfiles");
        }
    }
}

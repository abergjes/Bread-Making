using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreadMaking.App.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingAndTagsToBakeOutcome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBestLoaf",
                table: "BakeOutcomes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OverallScore",
                table: "BakeOutcomes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "BakeOutcomes",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBestLoaf",
                table: "BakeOutcomes");

            migrationBuilder.DropColumn(
                name: "OverallScore",
                table: "BakeOutcomes");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "BakeOutcomes");
        }
    }
}

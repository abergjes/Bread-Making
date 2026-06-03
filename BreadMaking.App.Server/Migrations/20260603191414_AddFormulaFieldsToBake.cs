using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreadMaking.App.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFormulaFieldsToBake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "HydrationPct",
                table: "Bakes",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "InoculationPct",
                table: "Bakes",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SaltPct",
                table: "Bakes",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StarterActivity",
                table: "Bakes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalFlourGrams",
                table: "Bakes",
                type: "REAL",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 108,
                column: "StepMin",
                value: 30);

            migrationBuilder.UpdateData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 308,
                column: "StepMin",
                value: 30);

            migrationBuilder.UpdateData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 508,
                column: "StepMin",
                value: 30);

            migrationBuilder.UpdateData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 708,
                column: "StepMin",
                value: 30);

            migrationBuilder.UpdateData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 908,
                column: "StepMin",
                value: 30);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HydrationPct",
                table: "Bakes");

            migrationBuilder.DropColumn(
                name: "InoculationPct",
                table: "Bakes");

            migrationBuilder.DropColumn(
                name: "SaltPct",
                table: "Bakes");

            migrationBuilder.DropColumn(
                name: "StarterActivity",
                table: "Bakes");

            migrationBuilder.DropColumn(
                name: "TotalFlourGrams",
                table: "Bakes");

            migrationBuilder.UpdateData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 108,
                column: "StepMin",
                value: 60);

            migrationBuilder.UpdateData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 308,
                column: "StepMin",
                value: 60);

            migrationBuilder.UpdateData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 508,
                column: "StepMin",
                value: 60);

            migrationBuilder.UpdateData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 708,
                column: "StepMin",
                value: 60);

            migrationBuilder.UpdateData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 908,
                column: "StepMin",
                value: 60);
        }
    }
}

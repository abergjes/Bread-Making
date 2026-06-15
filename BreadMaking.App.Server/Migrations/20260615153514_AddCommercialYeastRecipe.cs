using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BreadMaking.App.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCommercialYeastRecipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Recipes",
                columns: new[] { "Id", "CreatedByLabel", "FrictionFactorC", "GrainProfileId", "IsUserDefined", "Method", "Name", "TargetDoughTempC", "TargetHydrationPct" },
                values: new object[] { 15, null, 4.0, 1, false, 5, "Modern wheat — Commercial yeast", 25.0, 72.0 });

            migrationBuilder.InsertData(
                table: "RecipeSteps",
                columns: new[] { "Id", "DefaultDurationMin", "Description", "MaxDurationMin", "MinDurationMin", "Name", "Order", "Phase", "RecipeId", "StepMin", "TargetTempC" },
                values: new object[,]
                {
                    { 1501, 5, "Rough mix of flour and water only — no yeast, no salt yet.", 15, 3, "Mix flour + water", 1, "Mix", 15, 5, null },
                    { 1502, 20, "Cover and rest. Enzymes hydrate the flour and begin softening gluten.", 30, 15, "Autolyse rest", 2, "Rest", 15, 5, null },
                    { 1503, 5, "Add instant yeast (0.5–2% of flour weight) and salt. Fold to incorporate.", 10, 3, "Add yeast + salt", 3, "Mix", 15, 5, null },
                    { 1504, 90, "3–4 sets of stretch & folds every 20 min. Dough should roughly double in volume.", 120, 60, "Bulk fermentation", 4, "Bulk", 15, 15, 25.0 },
                    { 1505, 10, "Gentle pre-shape on a lightly floured bench.", 20, 5, "Pre-shape", 5, "Shape", 15, 5, null },
                    { 1506, 20, "Leave uncovered. Gluten relaxes before final shaping.", 30, 15, "Bench rest", 6, "Shape", 15, 5, null },
                    { 1507, 10, "Build surface tension. Place in floured banneton or greased tin.", 20, 5, "Final shape", 7, "Shape", 15, 5, null },
                    { 1508, 60, "Proof until visibly puffed and the dough springs back slowly when poked.", 90, 45, "Room-temperature proof", 8, "Proof", 15, 15, 25.0 },
                    { 1509, 45, "Cast iron must be screaming hot before the loaf goes in.", 60, 30, "Preheat + Dutch oven", 9, "Bake", 15, 15, 250.0 },
                    { 1510, 20, "Steam trapped in the Dutch oven drives oven spring and crust formation.", 25, 15, "Bake covered", 10, "Bake", 15, 5, 250.0 },
                    { 1511, 20, "Achieve deep golden crust. Internal temperature should reach 96–98 °C.", 25, 15, "Bake uncovered", 11, "Bake", 15, 5, 220.0 },
                    { 1512, 60, "Do not cut early — the crumb is still setting during cooling.", 90, 45, "Cool on rack", 12, "Cool", 15, 15, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1501);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1502);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1503);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1504);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1505);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1506);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1507);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1508);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1509);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1510);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1511);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1512);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 15);
        }
    }
}

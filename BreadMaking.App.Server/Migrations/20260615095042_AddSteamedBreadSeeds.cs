using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BreadMaking.App.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSteamedBreadSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "GrainProfiles",
                columns: new[] { "Id", "FlavorNotes", "GlutenStrength", "HistoricalOrigin", "HydrationAdjustPct", "MaxAutolyseMinutes", "Name", "NeedsBinder", "NutritionHighlights", "Ploidy", "UsageNotes" },
                values: new object[] { 18, "Neutral, soft and pillowy; designed to carry fillings and steam-imparted sweetness", "Weak (cake/pastry)", "Low-protein milling fractions; used throughout East Asia for millennia in steamed breads", -15.0, 0, "Low-protein wheat", false, "9–11% protein; low gluten gives tender, fine crumb; lower in fibre than whole grain", "Hexaploid", "Use cake or pastry flour for Mantou / Baozi; not suitable for crust-forming bakes" });

            migrationBuilder.InsertData(
                table: "Recipes",
                columns: new[] { "Id", "CreatedByLabel", "FrictionFactorC", "GrainProfileId", "IsUserDefined", "Method", "Name", "TargetDoughTempC", "TargetHydrationPct" },
                values: new object[] { 13, null, 0.0, 18, false, 3, "Low-protein wheat — Steamed", 26.0, 57.0 });

            migrationBuilder.InsertData(
                table: "RecipeSteps",
                columns: new[] { "Id", "DefaultDurationMin", "Description", "MaxDurationMin", "MinDurationMin", "Name", "Order", "Phase", "RecipeId", "StepMin", "TargetTempC" },
                values: new object[,]
                {
                    { 1301, 10, "Combine cake/pastry flour with warm water (38–40 °C). Add yeast and sugar. Mix until smooth.", 20, 5, "Mix flour + water", 1, "Mix", 13, 5, null },
                    { 1302, 60, "Cover and keep warm. Rise until doubled. Keep dough temperature 26–28 °C for even, predictable rise.", 90, 45, "Bulk fermentation — rise until doubled", 2, "Bulk", 13, 15, 28.0 },
                    { 1303, 10, "Punch down to degas. Divide into equal portions (50–80 g for buns). Cover resting pieces.", 15, 5, "Knock back + portion", 3, "Shape", 13, 5, null },
                    { 1304, 15, "Roll each piece smooth or flatten and add filling. Place on parchment squares in the steamer.", 20, 10, "Final shape", 4, "Shape", 13, 5, null },
                    { 1305, 20, "Proof in steamer with lid on but heat OFF. Buns should puff slightly and feel soft. Do not over-proof.", 30, 15, "Final proof", 5, "Proof", 13, 5, 28.0 },
                    { 1306, 15, "Bring water to a vigorous simmer, then steam. Line lid with a cloth to prevent drip marks.", 18, 12, "Steam", 6, "Bake", 13, 2, 100.0 },
                    { 1307, 3, "Tilt lid slightly for 2–3 min before removing entirely — prevents skin collapse from cold air shock.", 5, 2, "Rest with lid ajar", 7, "Bake", 13, 1, null },
                    { 1308, 15, "Remove from steamer and cool briefly on a rack. Best eaten warm within the hour.", 20, 10, "Cool on rack", 8, "Cool", 13, 5, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1301);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1302);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1303);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1304);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1305);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1306);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1307);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1308);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 18);
        }
    }
}

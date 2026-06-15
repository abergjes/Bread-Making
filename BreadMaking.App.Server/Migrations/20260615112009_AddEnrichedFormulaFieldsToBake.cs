using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BreadMaking.App.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrichedFormulaFieldsToBake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ButterPct",
                table: "Bakes",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "EggPct",
                table: "Bakes",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPullmanTin",
                table: "Bakes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "MilkPct",
                table: "Bakes",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MilkPowderPct",
                table: "Bakes",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SugarPct",
                table: "Bakes",
                type: "REAL",
                nullable: true);

            migrationBuilder.InsertData(
                table: "GrainProfiles",
                columns: new[] { "Id", "FlavorNotes", "GlutenStrength", "HistoricalOrigin", "HydrationAdjustPct", "MaxAutolyseMinutes", "Name", "NeedsBinder", "NutritionHighlights", "Ploidy", "UsageNotes" },
                values: new object[] { 19, "Rich, buttery, subtly sweet; crumb is cloud-soft with a gentle pull", "Strong", "Shokupan (食パン) perfected in Japan in the early 20th century; Tangzhong method popularised by Yvonne Chen (2007)", 0.0, 0, "Enriched", false, "Higher in fat and sugar than lean bread; roux pre-gelatinises starch for moisture retention", "Hexaploid", "Use high-protein bread flour (12–14%) for structure to hold the enrichment; develop gluten before adding fat" });

            migrationBuilder.InsertData(
                table: "Recipes",
                columns: new[] { "Id", "CreatedByLabel", "FrictionFactorC", "GrainProfileId", "IsUserDefined", "Method", "Name", "TargetDoughTempC", "TargetHydrationPct" },
                values: new object[] { 14, null, 4.0, 19, false, 4, "Enriched — Shokupan (Tangzhong)", 26.0, 65.0 });

            migrationBuilder.InsertData(
                table: "RecipeSteps",
                columns: new[] { "Id", "DefaultDurationMin", "Description", "MaxDurationMin", "MinDurationMin", "Name", "Order", "Phase", "RecipeId", "StepMin", "TargetTempC" },
                values: new object[,]
                {
                    { 1401, 10, "Cook flour (6% of total) + liquid (5× flour weight) to ~65 °C, stirring constantly until a thick, glossy paste. Starch gelatinises — this is the key to the pillowy crumb.", 15, 8, "Prepare Tangzhong", 1, "Mix", 14, 2, 65.0 },
                    { 1402, 30, "Spread on a plate or press cling film to the surface to prevent a skin. Must reach 25 °C before mixing — adding it hot will kill the yeast.", 60, 20, "Cool Tangzhong", 2, "Rest", 14, 10, null },
                    { 1403, 15, "Combine flour, milk, egg, sugar, salt, yeast, and cooled Tangzhong. Mix until shaggy, then knead to a smooth, non-sticky dough. No butter yet — gluten must develop first.", 20, 10, "Mix dough (pre-butter)", 3, "Mix", 14, 5, null },
                    { 1404, 15, "Add cold butter in small cubes while the dough is mixing. Dough will look broken — keep going. Stop when it passes the window-pane test: silky, elastic, and no longer sticky.", 25, 10, "Add butter (window-pane)", 4, "Mix", 14, 5, null },
                    { 1405, 60, "Cover and keep warm at 26–28 °C. Enriched doughs rise faster than sourdough — watch for doubling in size, not just time elapsed.", 90, 45, "Bulk fermentation (until doubled)", 5, "Bulk", 14, 15, 27.0 },
                    { 1406, 10, "Scale portions equally. Gently degas and pre-shape into rounds. Cover and rest 5 min.", 15, 5, "Divide + pre-shape", 6, "Shape", 14, 5, null },
                    { 1407, 15, "Rest covered — gluten tightens after pre-shape. This relaxation makes the final roll-and-fold possible without tearing.", 20, 10, "Bench rest", 7, "Shape", 14, 5, null },
                    { 1408, 15, "Roll each piece flat, fold the sides in, roll up tightly, and place seam-down in the greased tin. For Pullman, fill the tin 70% and add the lid.", 20, 10, "Final shape + tin", 8, "Shape", 14, 5, null },
                    { 1409, 60, "Proof at 28–30 °C until dough is 80–90% of tin height (open tin: dome 2–3 cm above rim). Over-proofing collapses the crumb after baking.", 90, 45, "Final proof (80–90% tin height)", 9, "Proof", 14, 15, 29.0 },
                    { 1410, 30, "Pullman: bake with lid closed for 25 min, then remove lid for 5 min for colour. Open tin: bake uncovered 30 min. Internal temp target ~88 °C.", 35, 25, "Bake (Pullman lidded / open tin)", 10, "Bake", 14, 5, 190.0 },
                    { 1411, 60, "Cool in tin 5 min, then unmould onto a rack. Slice only when fully cool — the crumb is still setting as it cools and cuts gummy if sliced hot.", 120, 30, "Cool on rack", 11, "Cool", 14, 15, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1401);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1402);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1403);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1404);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1405);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1406);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1407);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1408);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1409);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1410);

            migrationBuilder.DeleteData(
                table: "RecipeSteps",
                keyColumn: "Id",
                keyValue: 1411);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DropColumn(
                name: "ButterPct",
                table: "Bakes");

            migrationBuilder.DropColumn(
                name: "EggPct",
                table: "Bakes");

            migrationBuilder.DropColumn(
                name: "IsPullmanTin",
                table: "Bakes");

            migrationBuilder.DropColumn(
                name: "MilkPct",
                table: "Bakes");

            migrationBuilder.DropColumn(
                name: "MilkPowderPct",
                table: "Bakes");

            migrationBuilder.DropColumn(
                name: "SugarPct",
                table: "Bakes");
        }
    }
}

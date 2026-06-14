using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BreadMaking.App.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddEncyclopediaFieldsToGrainProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FlavorNotes",
                table: "GrainProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HistoricalOrigin",
                table: "GrainProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NutritionHighlights",
                table: "GrainProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsageNotes",
                table: "GrainProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CrumbNotes",
                table: "BakeOutcomes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProofingResult",
                table: "BakeOutcomes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FlavorNotes", "HistoricalOrigin", "NutritionHighlights", "UsageNotes" },
                values: new object[] { "Mild, slightly sweet; neutral backdrop that lets fermentation flavours shine", "Bred since 19th century from emmer × einkorn crosses; global staple", "High gluten (13–14%); refined versions stripped of bran and germ", "Universal base flour; ideal 100% for most sourdough and sandwich loaves" });

            migrationBuilder.UpdateData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "FlavorNotes", "HistoricalOrigin", "NutritionHighlights", "UsageNotes" },
                values: new object[] { "Earthy, nutty, slightly bitter from bran; deeper character than white wheat", "Same hexaploid wheat as white flour; nothing removed in milling", "Retains bran and germ; high fibre, B vitamins, iron, zinc", "25–50% blend for nutrition boost; absorbs 5% more water than white" });

            migrationBuilder.UpdateData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "FlavorNotes", "HistoricalOrigin", "NutritionHighlights", "UsageNotes" },
                values: new object[] { "Sour, dark, slightly earthy; pronounced complexity with long fermentation", "Secale cereale; cultivated in Central Europe since ~2000 BC; Nordic staple", "High pentosan (soluble fibre); low GI; high lysine; rich in B vitamins", "10–30% blend for flavour depth; high percentage gives dense Nordic-style crumb" });

            migrationBuilder.UpdateData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "FlavorNotes", "HistoricalOrigin", "NutritionHighlights", "UsageNotes" },
                values: new object[] { "Mildly nutty, slightly sweet; lighter character than whole wheat", "Triticum spelta; hexaploid ancient grain cultivated in Europe since ~5000 BC", "Higher protein than modern wheat; soluble form; better B vitamin profile", "Sub 1:1 for wheat but handle gently; gluten is extensible, not elastic" });

            migrationBuilder.UpdateData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "FlavorNotes", "HistoricalOrigin", "NutritionHighlights", "UsageNotes" },
                values: new object[] { "Rich, buttery, almost corn-like; complex malty sweetness", "First cultivated wheat; diploid; Fertile Crescent ~10 000 BC", "High carotenoids (golden crumb); good zinc and B6; less starch than wheat", "15–30% max in blends; very weak gluten — short autolyse, folds only" });

            migrationBuilder.UpdateData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "FlavorNotes", "HistoricalOrigin", "NutritionHighlights", "UsageNotes" },
                values: new object[] { "Earthy, nutty, rustic; hint of bitterness from bran", "Tetraploid; domesticated in Fertile Crescent ~8000 BC; key ancient grain", "High fibre, iron, magnesium; richer protein than modern wheat", "30–50% in sourdough blends; contributes open crumb at lower ratios" });

            migrationBuilder.UpdateData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "FlavorNotes", "HistoricalOrigin", "NutritionHighlights", "UsageNotes" },
                values: new object[] { "Sweet, buttery, slightly rich; milder sour note than modern wheat sourdough", "Khorasan wheat; tetraploid; ancient Egyptian grain; trademarked as KAMUT®", "25% more protein than modern wheat; high selenium and zinc", "50–100% sourdough loaves; benefits from longer autolyse (30–45 min)" });

            migrationBuilder.UpdateData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "FlavorNotes", "HistoricalOrigin", "NutritionHighlights", "UsageNotes" },
                values: new object[] { "Earthy, molasses-like, slightly chocolatey; strong and distinctive", "Grass grain from Ethiopia; staple for millennia; world's smallest cereal grain", "Gluten-free; very high iron and calcium; good fibre; complete amino acids", "10–20% in GF blends with binder; backbone of Ethiopian injera at 100%" });

            migrationBuilder.UpdateData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "FlavorNotes", "HistoricalOrigin", "NutritionHighlights", "UsageNotes" },
                values: new object[] { "Mild, slightly sweet, neutral; versatile GF base flour", "Grass grain originating in Africa ~8000 years ago; 5th most grown cereal worldwide", "Gluten-free; high protein; good antioxidants; cholesterol-lowering compounds", "Base GF flour 50–60%; combine with tapioca starch and psyllium husk binder" });

            migrationBuilder.InsertData(
                table: "GrainProfiles",
                columns: new[] { "Id", "FlavorNotes", "GlutenStrength", "HistoricalOrigin", "HydrationAdjustPct", "MaxAutolyseMinutes", "Name", "NeedsBinder", "NutritionHighlights", "Ploidy", "UsageNotes" },
                values: new object[,]
                {
                    { 10, "Malty, slightly sweet; classic fermentation base note", "Very low", "One of the first cultivated grains; Fertile Crescent ~10 000 BC; foundation of beer", 0.0, 30, "Barley", false, "Very high in soluble beta-glucan fibre; low GI; good selenium", "Diploid", "10–30% blend for malty depth; high beta-glucan makes dough sticky" },
                    { 11, "Rich, golden, slightly sweet and nutty; dense satisfying crumb", "Strong (stiff)", "Tetraploid wheat; Mediterranean staple; primarily milled for pasta and couscous", 5.0, 30, "Durum / Semolina", false, "High protein; rich in carotenoids giving yellow colour; good iron", "Tetraploid", "10–40% for golden crumb and richness; classic in pane di Altamura (100%)" },
                    { 12, "Mild rye-wheat hybrid; slightly earthy with a hint of sweetness", "Moderate", "First man-made grain; rye x wheat hybrid developed in Scotland 1875", 5.0, 40, "Triticale", false, "Higher protein and lysine than wheat; good fibre; bred for nutrition", "Hexaploid", "20–50% blend; behaves like mild rye; excellent starter food" },
                    { 13, "Creamy, mild, slightly toasty; adds sweetness and a chewy crust", "None (GF)", "Avena sativa; cultivated in Europe since ~3000 years ago; Scottish staple grain", 10.0, 0, "Oat", true, "Naturally gluten-free (check cross-contamination); high soluble beta-glucan; heart-healthy", "Hexaploid", "10–20% rolled or flour; use with psyllium binder in GF loaves" },
                    { 14, "Bold, earthy, slightly bitter; pronounced and distinctive flavour", "None (GF)", "Not a true cereal; Fagopyrum genus; Central Asia; cultivated ~8000 years ago", 5.0, 0, "Buckwheat", true, "Gluten-free; complete protein (all 9 amino acids); high rutin antioxidant", null, "10–20% in blends for strong flavour; classic in blini and French galettes" },
                    { 15, "Earthy, slightly grassy and peppery; intense flavour dominates blends", "None (GF)", "Pseudocereal of the Americas; Aztec sacred staple ~6000 years ago", 5.0, 0, "Amaranth", true, "Gluten-free; high quality protein (high lysine); rich in iron and calcium", null, "5–10% maximum — bold flavour; best in multigrain or seeded loaves" },
                    { 16, "Mild, slightly nutty and earthy; bitter if seeds not rinsed (saponins)", "None (GF)", "Pseudocereal from Andean South America; Inca sacred crop; ~7000 years ago", 5.0, 0, "Quinoa", true, "Gluten-free; complete protein; high magnesium, folate, and phosphorus", null, "5–15% as flour; rinse seeds before milling; toast for richer flavour" },
                    { 17, "Mild, slightly sweet and corn-like; light and delicate character", "None (GF)", "Panicum miliaceum; one of the first domesticated grains; Asia and Africa ~10 000 BC", 0.0, 0, "Millet", true, "Gluten-free; good iron and B vitamins; alkaline-forming grain", null, "10–20% in GF blends for lightness; contributes golden colour to crumb" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "GrainProfiles",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DropColumn(
                name: "FlavorNotes",
                table: "GrainProfiles");

            migrationBuilder.DropColumn(
                name: "HistoricalOrigin",
                table: "GrainProfiles");

            migrationBuilder.DropColumn(
                name: "NutritionHighlights",
                table: "GrainProfiles");

            migrationBuilder.DropColumn(
                name: "UsageNotes",
                table: "GrainProfiles");

            migrationBuilder.DropColumn(
                name: "CrumbNotes",
                table: "BakeOutcomes");

            migrationBuilder.DropColumn(
                name: "ProofingResult",
                table: "BakeOutcomes");
        }
    }
}

using BreadMaking.App.Shared;

namespace BreadMaking.App.Server.Data;

/// <summary>
/// All HasData seed values. IDs are stable and must not change once migrated.
/// RecipeStep IDs use recipeId * 100 + stepOrder (e.g. Recipe 3, Step 2 → ID 302).
/// </summary>
public static class SeedData
{
    public static readonly GrainProfileEntity[] GrainProfiles =
    [
        // — existing 9 grains (baker's guide §15 encyclopedia data added in M17) —
        new() { Id = 1, Name = "Modern wheat",    Ploidy = "Hexaploid",  GlutenStrength = "Strong",             HydrationAdjustPct =   0, MaxAutolyseMinutes = 60, NeedsBinder = false,
            FlavorNotes = "Mild, slightly sweet; neutral backdrop that lets fermentation flavours shine",
            NutritionHighlights = "High gluten (13–14%); refined versions stripped of bran and germ",
            UsageNotes = "Universal base flour; ideal 100% for most sourdough and sandwich loaves",
            HistoricalOrigin = "Bred since 19th century from emmer × einkorn crosses; global staple" },
        new() { Id = 2, Name = "Whole grain",     Ploidy = "Hexaploid",  GlutenStrength = "Strong",             HydrationAdjustPct =   5, MaxAutolyseMinutes = 60, NeedsBinder = false,
            FlavorNotes = "Earthy, nutty, slightly bitter from bran; deeper character than white wheat",
            NutritionHighlights = "Retains bran and germ; high fibre, B vitamins, iron, zinc",
            UsageNotes = "25–50% blend for nutrition boost; absorbs 5% more water than white",
            HistoricalOrigin = "Same hexaploid wheat as white flour; nothing removed in milling" },
        new() { Id = 3, Name = "Rye",             Ploidy = null,         GlutenStrength = "Very low (bran)",    HydrationAdjustPct =  15, MaxAutolyseMinutes = 60, NeedsBinder = false,
            FlavorNotes = "Sour, dark, slightly earthy; pronounced complexity with long fermentation",
            NutritionHighlights = "High pentosan (soluble fibre); low GI; high lysine; rich in B vitamins",
            UsageNotes = "10–30% blend for flavour depth; high percentage gives dense Nordic-style crumb",
            HistoricalOrigin = "Secale cereale; cultivated in Central Europe since ~2000 BC; Nordic staple" },
        new() { Id = 4, Name = "Spelt",           Ploidy = "Hexaploid",  GlutenStrength = "Moderate (fragile)", HydrationAdjustPct =  -5, MaxAutolyseMinutes = 30, NeedsBinder = false,
            FlavorNotes = "Mildly nutty, slightly sweet; lighter character than whole wheat",
            NutritionHighlights = "Higher protein than modern wheat; soluble form; better B vitamin profile",
            UsageNotes = "Sub 1:1 for wheat but handle gently; gluten is extensible, not elastic",
            HistoricalOrigin = "Triticum spelta; hexaploid ancient grain cultivated in Europe since ~5000 BC" },
        new() { Id = 5, Name = "Einkorn",         Ploidy = "Diploid",    GlutenStrength = "Very weak",          HydrationAdjustPct = -15, MaxAutolyseMinutes = 15, NeedsBinder = false,
            FlavorNotes = "Rich, buttery, almost corn-like; complex malty sweetness",
            NutritionHighlights = "High carotenoids (golden crumb); good zinc and B6; less starch than wheat",
            UsageNotes = "15–30% max in blends; very weak gluten — short autolyse, folds only",
            HistoricalOrigin = "First cultivated wheat; diploid; Fertile Crescent ~10 000 BC" },
        new() { Id = 6, Name = "Emmer (farro)",   Ploidy = "Tetraploid", GlutenStrength = "Weak",               HydrationAdjustPct = -10, MaxAutolyseMinutes = 20, NeedsBinder = false,
            FlavorNotes = "Earthy, nutty, rustic; hint of bitterness from bran",
            NutritionHighlights = "High fibre, iron, magnesium; richer protein than modern wheat",
            UsageNotes = "30–50% in sourdough blends; contributes open crumb at lower ratios",
            HistoricalOrigin = "Tetraploid; domesticated in Fertile Crescent ~8000 BC; key ancient grain" },
        new() { Id = 7, Name = "Kamut (khorasan)", Ploidy = "Tetraploid", GlutenStrength = "Strong",            HydrationAdjustPct =  10, MaxAutolyseMinutes = 45, NeedsBinder = false,
            FlavorNotes = "Sweet, buttery, slightly rich; milder sour note than modern wheat sourdough",
            NutritionHighlights = "25% more protein than modern wheat; high selenium and zinc",
            UsageNotes = "50–100% sourdough loaves; benefits from longer autolyse (30–45 min)",
            HistoricalOrigin = "Khorasan wheat; tetraploid; ancient Egyptian grain; trademarked as KAMUT®" },
        new() { Id = 8, Name = "Teff",            Ploidy = null,         GlutenStrength = "None (GF)",          HydrationAdjustPct =   0, MaxAutolyseMinutes =  0, NeedsBinder = true,
            FlavorNotes = "Earthy, molasses-like, slightly chocolatey; strong and distinctive",
            NutritionHighlights = "Gluten-free; very high iron and calcium; good fibre; complete amino acids",
            UsageNotes = "10–20% in GF blends with binder; backbone of Ethiopian injera at 100%",
            HistoricalOrigin = "Grass grain from Ethiopia; staple for millennia; world's smallest cereal grain" },
        new() { Id = 9, Name = "Sorghum",         Ploidy = null,         GlutenStrength = "None (GF)",          HydrationAdjustPct =   0, MaxAutolyseMinutes =  0, NeedsBinder = true,
            FlavorNotes = "Mild, slightly sweet, neutral; versatile GF base flour",
            NutritionHighlights = "Gluten-free; high protein; good antioxidants; cholesterol-lowering compounds",
            UsageNotes = "Base GF flour 50–60%; combine with tapioca starch and psyllium husk binder",
            HistoricalOrigin = "Grass grain originating in Africa ~8000 years ago; 5th most grown cereal worldwide" },

        // — 8 additional encyclopedia grains (no recipes; reference only) —
        new() { Id = 10, Name = "Barley",          Ploidy = "Diploid",   GlutenStrength = "Very low",           HydrationAdjustPct =   0, MaxAutolyseMinutes = 30, NeedsBinder = false,
            FlavorNotes = "Malty, slightly sweet; classic fermentation base note",
            NutritionHighlights = "Very high in soluble beta-glucan fibre; low GI; good selenium",
            UsageNotes = "10–30% blend for malty depth; high beta-glucan makes dough sticky",
            HistoricalOrigin = "One of the first cultivated grains; Fertile Crescent ~10 000 BC; foundation of beer" },
        new() { Id = 11, Name = "Durum / Semolina", Ploidy = "Tetraploid", GlutenStrength = "Strong (stiff)",   HydrationAdjustPct =   5, MaxAutolyseMinutes = 30, NeedsBinder = false,
            FlavorNotes = "Rich, golden, slightly sweet and nutty; dense satisfying crumb",
            NutritionHighlights = "High protein; rich in carotenoids giving yellow colour; good iron",
            UsageNotes = "10–40% for golden crumb and richness; classic in pane di Altamura (100%)",
            HistoricalOrigin = "Tetraploid wheat; Mediterranean staple; primarily milled for pasta and couscous" },
        new() { Id = 12, Name = "Triticale",       Ploidy = "Hexaploid", GlutenStrength = "Moderate",           HydrationAdjustPct =   5, MaxAutolyseMinutes = 40, NeedsBinder = false,
            FlavorNotes = "Mild rye-wheat hybrid; slightly earthy with a hint of sweetness",
            NutritionHighlights = "Higher protein and lysine than wheat; good fibre; bred for nutrition",
            UsageNotes = "20–50% blend; behaves like mild rye; excellent starter food",
            HistoricalOrigin = "First man-made grain; rye x wheat hybrid developed in Scotland 1875" },
        new() { Id = 13, Name = "Oat",             Ploidy = "Hexaploid", GlutenStrength = "None (GF)",          HydrationAdjustPct =  10, MaxAutolyseMinutes =  0, NeedsBinder = true,
            FlavorNotes = "Creamy, mild, slightly toasty; adds sweetness and a chewy crust",
            NutritionHighlights = "Naturally gluten-free (check cross-contamination); high soluble beta-glucan; heart-healthy",
            UsageNotes = "10–20% rolled or flour; use with psyllium binder in GF loaves",
            HistoricalOrigin = "Avena sativa; cultivated in Europe since ~3000 years ago; Scottish staple grain" },
        new() { Id = 14, Name = "Buckwheat",       Ploidy = null,        GlutenStrength = "None (GF)",          HydrationAdjustPct =   5, MaxAutolyseMinutes =  0, NeedsBinder = true,
            FlavorNotes = "Bold, earthy, slightly bitter; pronounced and distinctive flavour",
            NutritionHighlights = "Gluten-free; complete protein (all 9 amino acids); high rutin antioxidant",
            UsageNotes = "10–20% in blends for strong flavour; classic in blini and French galettes",
            HistoricalOrigin = "Not a true cereal; Fagopyrum genus; Central Asia; cultivated ~8000 years ago" },
        new() { Id = 15, Name = "Amaranth",        Ploidy = null,        GlutenStrength = "None (GF)",          HydrationAdjustPct =   5, MaxAutolyseMinutes =  0, NeedsBinder = true,
            FlavorNotes = "Earthy, slightly grassy and peppery; intense flavour dominates blends",
            NutritionHighlights = "Gluten-free; high quality protein (high lysine); rich in iron and calcium",
            UsageNotes = "5–10% maximum — bold flavour; best in multigrain or seeded loaves",
            HistoricalOrigin = "Pseudocereal of the Americas; Aztec sacred staple ~6000 years ago" },
        new() { Id = 16, Name = "Quinoa",          Ploidy = null,        GlutenStrength = "None (GF)",          HydrationAdjustPct =   5, MaxAutolyseMinutes =  0, NeedsBinder = true,
            FlavorNotes = "Mild, slightly nutty and earthy; bitter if seeds not rinsed (saponins)",
            NutritionHighlights = "Gluten-free; complete protein; high magnesium, folate, and phosphorus",
            UsageNotes = "5–15% as flour; rinse seeds before milling; toast for richer flavour",
            HistoricalOrigin = "Pseudocereal from Andean South America; Inca sacred crop; ~7000 years ago" },
        new() { Id = 17, Name = "Millet",          Ploidy = null,        GlutenStrength = "None (GF)",          HydrationAdjustPct =   0, MaxAutolyseMinutes =  0, NeedsBinder = true,
            FlavorNotes = "Mild, slightly sweet and corn-like; light and delicate character",
            NutritionHighlights = "Gluten-free; good iron and B vitamins; alkaline-forming grain",
            UsageNotes = "10–20% in GF blends for lightness; contributes golden colour to crumb",
            HistoricalOrigin = "Panicum miliaceum; one of the first domesticated grains; Asia and Africa ~10 000 BC" },

        // Steamed bread grain (M22)
        new() { Id = 18, Name = "Low-protein wheat", Ploidy = "Hexaploid", GlutenStrength = "Weak (cake/pastry)", HydrationAdjustPct = -15, MaxAutolyseMinutes = 0, NeedsBinder = false,
            FlavorNotes = "Neutral, soft and pillowy; designed to carry fillings and steam-imparted sweetness",
            NutritionHighlights = "9–11% protein; low gluten gives tender, fine crumb; lower in fibre than whole grain",
            UsageNotes = "Use cake or pastry flour for Mantou / Baozi; not suitable for crust-forming bakes",
            HistoricalOrigin = "Low-protein milling fractions; used throughout East Asia for millennia in steamed breads" },

        // Enriched/milk-bread grain (M23)
        new() { Id = 19, Name = "Enriched", Ploidy = "Hexaploid", GlutenStrength = "Strong", HydrationAdjustPct = 0, MaxAutolyseMinutes = 0, NeedsBinder = false,
            FlavorNotes = "Rich, buttery, subtly sweet; crumb is cloud-soft with a gentle pull",
            NutritionHighlights = "Higher in fat and sugar than lean bread; roux pre-gelatinises starch for moisture retention",
            UsageNotes = "Use high-protein bread flour (12–14%) for structure to hold the enrichment; develop gluten before adding fat",
            HistoricalOrigin = "Shokupan (食パン) perfected in Japan in the early 20th century; Tangzhong method popularised by Yvonne Chen (2007)" },
    ];

    // Recipes: grainProfileId × method.  Rye/WholeGrain fall back to Modern wheat via BakeSessionService.
    public static readonly Recipe[] Recipes =
    [
        new() { Id = 13, GrainProfileId = 18, Name = "Low-protein wheat — Steamed", Method = BakeMethod.Steamed,   TargetHydrationPct = 57, TargetDoughTempC = 26, FrictionFactorC = 0 },
        new() { Id = 14, GrainProfileId = 19, Name = "Enriched — Shokupan (Tangzhong)", Method = BakeMethod.Enriched, TargetHydrationPct = 65, TargetDoughTempC = 26, FrictionFactorC = 4 },
        new() { Id =  1, GrainProfileId = 1, Name = "Modern wheat — Autolyse",      Method = BakeMethod.Autolyse,     TargetHydrationPct = 72, TargetDoughTempC = 25, FrictionFactorC = 4 },
        new() { Id =  2, GrainProfileId = 1, Name = "Modern wheat — Fermentolyse",  Method = BakeMethod.Fermentolyse, TargetHydrationPct = 72, TargetDoughTempC = 25, FrictionFactorC = 4 },
        new() { Id =  3, GrainProfileId = 4, Name = "Spelt — Autolyse",             Method = BakeMethod.Autolyse,     TargetHydrationPct = 68, TargetDoughTempC = 24, FrictionFactorC = 4 },
        new() { Id =  4, GrainProfileId = 4, Name = "Spelt — Fermentolyse",         Method = BakeMethod.Fermentolyse, TargetHydrationPct = 68, TargetDoughTempC = 24, FrictionFactorC = 4 },
        new() { Id =  5, GrainProfileId = 5, Name = "Einkorn — Autolyse",           Method = BakeMethod.Autolyse,     TargetHydrationPct = 62, TargetDoughTempC = 24, FrictionFactorC = 4 },
        new() { Id =  6, GrainProfileId = 5, Name = "Einkorn — Fermentolyse",       Method = BakeMethod.Fermentolyse, TargetHydrationPct = 62, TargetDoughTempC = 24, FrictionFactorC = 4 },
        new() { Id =  7, GrainProfileId = 6, Name = "Emmer — Autolyse",             Method = BakeMethod.Autolyse,     TargetHydrationPct = 65, TargetDoughTempC = 24, FrictionFactorC = 4 },
        new() { Id =  8, GrainProfileId = 6, Name = "Emmer — Fermentolyse",         Method = BakeMethod.Fermentolyse, TargetHydrationPct = 65, TargetDoughTempC = 24, FrictionFactorC = 4 },
        new() { Id =  9, GrainProfileId = 7, Name = "Kamut — Autolyse",             Method = BakeMethod.Autolyse,     TargetHydrationPct = 78, TargetDoughTempC = 25, FrictionFactorC = 4 },
        new() { Id = 10, GrainProfileId = 7, Name = "Kamut — Fermentolyse",         Method = BakeMethod.Fermentolyse, TargetHydrationPct = 78, TargetDoughTempC = 25, FrictionFactorC = 4 },
        new() { Id = 11, GrainProfileId = 8, Name = "Teff — Soaker",                Method = BakeMethod.Other,        TargetHydrationPct = 95, TargetDoughTempC = 24, FrictionFactorC = 0 },
        new() { Id = 12, GrainProfileId = 9, Name = "Sorghum — Soaker",             Method = BakeMethod.Other,        TargetHydrationPct = 90, TargetDoughTempC = 24, FrictionFactorC = 0 },
    ];

    public static readonly MeasurementType[] MeasurementTypes =
    [
        new() { Id = 1, Name = "Dough temp",    Unit = "°C",  Category = "InProcess", MinValid =  10, MaxValid =  40, DefaultForPhase = "Mix"  },
        new() { Id = 2, Name = "Aliquot rise",  Unit = "%",   Category = "InProcess", MinValid =   0, MaxValid = 200, DefaultForPhase = "Bulk" },
        new() { Id = 3, Name = "pH",            Unit = "pH",  Category = "InProcess", MinValid = 3.0, MaxValid = 7.0, DefaultForPhase = "Bulk" },
        new() { Id = 4, Name = "TTA",           Unit = "mL",  Category = "InProcess", MinValid =   0, MaxValid =  30, DefaultForPhase = "Bulk" },
        new() { Id = 5, Name = "Internal temp", Unit = "°C",  Category = "Outcome",   MinValid =  80, MaxValid = 110, DefaultForPhase = "Bake" },
    ];

    public static readonly RecipeStep[] RecipeSteps = BuildAllSteps();

    // ── Step builders ────────────────────────────────────────────────────────

    private static RecipeStep[] BuildAllSteps()
    {
        var steps = new List<RecipeStep>();

        // Modern wheat (at 22°C sweet-spot values from TempBands in BreadAdvisorService)
        AddAutolyseSteps(steps,  recipeId:  1, restDefault: 50, restMin: 40, restMax: 60, bulkDefault: 300, bulkMin: 240, bulkMax: 360);
        AddFermentoSteps(steps,  recipeId:  2, restDefault: 75, restMin: 60, restMax: 90, bulkDefault: 300, bulkMin: 240, bulkMax: 360);

        // Spelt — MaxAutolyseMinutes=30; fermentolyse capped same
        AddAutolyseSteps(steps,  recipeId:  3, restDefault: 20, restMin: 15, restMax: 30, bulkDefault: 240, bulkMin: 180, bulkMax: 300);
        AddFermentoSteps(steps,  recipeId:  4, restDefault: 25, restMin: 20, restMax: 30, bulkDefault: 240, bulkMin: 180, bulkMax: 300);

        // Einkorn — MaxAutolyseMinutes=15; very fast bulk
        AddAutolyseSteps(steps,  recipeId:  5, restDefault: 10, restMin:  5, restMax: 15, bulkDefault: 180, bulkMin: 120, bulkMax: 240);
        AddFermentoSteps(steps,  recipeId:  6, restDefault: 13, restMin: 10, restMax: 15, bulkDefault: 180, bulkMin: 120, bulkMax: 240);

        // Emmer — MaxAutolyseMinutes=20; fast bulk
        AddAutolyseSteps(steps,  recipeId:  7, restDefault: 15, restMin: 10, restMax: 20, bulkDefault: 210, bulkMin: 150, bulkMax: 270);
        AddFermentoSteps(steps,  recipeId:  8, restDefault: 18, restMin: 15, restMax: 20, bulkDefault: 210, bulkMin: 150, bulkMax: 270);

        // Kamut — MaxAutolyseMinutes=45; slightly slower bulk
        AddAutolyseSteps(steps,  recipeId:  9, restDefault: 37, restMin: 30, restMax: 45, bulkDefault: 330, bulkMin: 270, bulkMax: 420);
        AddFermentoSteps(steps,  recipeId: 10, restDefault: 40, restMin: 35, restMax: 45, bulkDefault: 330, bulkMin: 270, bulkMax: 420);

        // Gluten-free batter process
        AddSoakerSteps(steps, recipeId: 11, soakerDefault: 40, soakerMin: 30, soakerMax: 60);
        AddSoakerSteps(steps, recipeId: 12, soakerDefault: 45, soakerMin: 30, soakerMax: 60);

        // Steamed bread (Mantou / Baozi)
        AddSteamedSteps(steps, recipeId: 13);

        // Enriched milk bread (Shokupan / Tangzhong)
        AddEnrichedSteps(steps, recipeId: 14);

        return steps.ToArray();
    }

    private static void AddAutolyseSteps(
        List<RecipeStep> steps, int recipeId,
        int restDefault, int restMin, int restMax,
        int bulkDefault, int bulkMin, int bulkMax)
    {
        int id = recipeId * 100;
        steps.AddRange(
        [
            S(++id, recipeId, 1,  "Mix",   "Mix flour + water",
                5,   3,   15,  5, null,
                "Rough mix of flour and water only — no salt, no starter yet."),
            S(++id, recipeId, 2,  "Rest",  "Autolyse rest",
                restDefault, restMin, restMax, 5, null,
                "Cover and leave undisturbed. Enzymes hydrate flour and begin softening gluten."),
            S(++id, recipeId, 3,  "Mix",   "Add salt + starter",
                5,   3,   15,  5, null,
                "Dimple salt in and fold to incorporate. Add starter and fold in fully."),
            S(++id, recipeId, 4,  "Bulk",  "Bulk fermentation",
                bulkDefault, bulkMin, bulkMax, 15, 23.0,
                "4–6 sets of stretch & folds every 30 min. Dough should grow 50–75%."),
            S(++id, recipeId, 5,  "Shape", "Pre-shape",
                10,  5,   20,  5, null,
                "Gentle pre-shape. Flour the bench lightly."),
            S(++id, recipeId, 6,  "Shape", "Bench rest",
                30,  20,  45,  5, null,
                "Leave uncovered on bench. Let gluten relax before final shape."),
            S(++id, recipeId, 7,  "Shape", "Final shape",
                10,  5,   20,  5, null,
                "Build surface tension. Place seam-side up in floured banneton."),
            S(++id, recipeId, 8,  "Proof", "Cold proof",
            // Changed the stepMin from 60 to 30 minutes (2026-06-03)
                960, 480, 1440, 30, 4.0,
                "Retard in fridge overnight. Develops flavour and structure."),
            S(++id, recipeId, 9,  "Bake",  "Preheat + Dutch oven",
                45,  30,  60,  15, 250.0,
                "Cast iron must be screaming hot before the loaf goes in."),
            S(++id, recipeId, 10, "Bake",  "Bake covered",
                20,  15,  25,  5,  250.0,
                "Steam inside Dutch oven drives oven spring and crust formation."),
            S(++id, recipeId, 11, "Bake",  "Bake uncovered",
                20,  15,  30,  5,  220.0,
                "Achieve deep caramelised crust. Internal temp should reach 96–98 °C."),
            S(++id, recipeId, 12, "Cool",  "Cool on rack",
                120, 60,  180, 30, null,
                "Crumb is still setting during cooling — do not cut early."),
        ]);
    }

    private static void AddFermentoSteps(
        List<RecipeStep> steps, int recipeId,
        int restDefault, int restMin, int restMax,
        int bulkDefault, int bulkMin, int bulkMax)
    {
        int id = recipeId * 100;
        steps.AddRange(
        [
            S(++id, recipeId, 1,  "Mix",   "Mix flour + water + starter",
                5,   3,   15,  5, null,
                "Rough mix of flour, water, and starter — no salt yet."),
            S(++id, recipeId, 2,  "Rest",  "Fermentolyse rest",
                restDefault, restMin, restMax, 5, null,
                "Cover and leave undisturbed. Fermentation begins — watch for over-activity in warm kitchens."),
            S(++id, recipeId, 3,  "Mix",   "Add salt",
                5,   3,   15,  5, null,
                "Dimple salt in and fold to incorporate thoroughly."),
            S(++id, recipeId, 4,  "Bulk",  "Bulk fermentation",
                bulkDefault, bulkMin, bulkMax, 15, 23.0,
                "4–6 sets of stretch & folds every 30 min. Dough should grow 50–75%."),
            S(++id, recipeId, 5,  "Shape", "Pre-shape",
                10,  5,   20,  5, null,
                "Gentle pre-shape. Flour the bench lightly."),
            S(++id, recipeId, 6,  "Shape", "Bench rest",
                30,  20,  45,  5, null,
                "Leave uncovered on bench. Let gluten relax before final shape."),
            S(++id, recipeId, 7,  "Shape", "Final shape",
                10,  5,   20,  5, null,
                "Build surface tension. Place seam-side up in floured banneton."),
            S(++id, recipeId, 8,  "Proof", "Cold proof",
                960, 480, 1440, 60, 4.0,
                "Retard in fridge overnight. Develops flavour and structure."),
            S(++id, recipeId, 9,  "Bake",  "Preheat + Dutch oven",
                45,  30,  60,  15, 250.0,
                "Cast iron must be screaming hot before the loaf goes in."),
            S(++id, recipeId, 10, "Bake",  "Bake covered",
                20,  15,  25,  5,  250.0,
                "Steam inside Dutch oven drives oven spring and crust formation."),
            S(++id, recipeId, 11, "Bake",  "Bake uncovered",
                20,  15,  30,  5,  220.0,
                "Achieve deep caramelised crust. Internal temp should reach 96–98 °C."),
            S(++id, recipeId, 12, "Cool",  "Cool on rack",
                120, 60,  180, 30, null,
                "Crumb is still setting during cooling — do not cut early."),
        ]);
    }

    private static void AddSoakerSteps(
        List<RecipeStep> steps, int recipeId,
        int soakerDefault, int soakerMin, int soakerMax)
    {
        int id = recipeId * 100;
        steps.AddRange(
        [
            S(++id, recipeId, 1, "Mix",   "Whisk batter",
                5,  3,  10, 5, null,
                "Combine flour, liquid, and binder (psyllium / xanthan / egg). No kneading."),
            S(++id, recipeId, 2, "Rest",  "Soaker rest",
                soakerDefault, soakerMin, soakerMax, 5, null,
                "Cover and rest. Starch absorbs liquid — reduces grittiness, improves crumb texture."),
            S(++id, recipeId, 3, "Mix",   "Add remaining ingredients",
                5,  3,  10, 5, null,
                "Fold in salt, any sweetener, fat, and starter if using sourdough."),
            S(++id, recipeId, 4, "Proof", "Ferment / proof",
                90, 60, 180, 15, 25.0,
                "Gluten-free batter rises quickly and less dramatically. Watch for bubbles."),
            S(++id, recipeId, 5, "Bake",  "Bake in tin (covered)",
                20, 15,  25, 5, 220.0,
                "Cover with foil or lid to trap steam and prevent early over-crust."),
            S(++id, recipeId, 6, "Bake",  "Bake uncovered",
                30, 25,  40, 5, 200.0,
                "Until crust is set and internal temperature reaches 96–98 °C."),
            S(++id, recipeId, 7, "Cool",  "Cool on rack",
                90, 60, 120, 15, null,
                "Gluten-free crumb continues setting during cooling — do not cut early."),
        ]);
    }

    private static void AddSteamedSteps(List<RecipeStep> steps, int recipeId)
    {
        int id = recipeId * 100;
        steps.AddRange(
        [
            S(++id, recipeId, 1, "Mix",   "Mix flour + water",
                10,  5,  20, 5, null,
                "Combine cake/pastry flour with warm water (38–40 °C). Add yeast and sugar. Mix until smooth."),
            S(++id, recipeId, 2, "Bulk",  "Bulk fermentation — rise until doubled",
                60, 45,  90, 15, 28.0,
                "Cover and keep warm. Rise until doubled. Keep dough temperature 26–28 °C for even, predictable rise."),
            S(++id, recipeId, 3, "Shape", "Knock back + portion",
                10,  5,  15, 5, null,
                "Punch down to degas. Divide into equal portions (50–80 g for buns). Cover resting pieces."),
            S(++id, recipeId, 4, "Shape", "Final shape",
                15, 10,  20, 5, null,
                "Roll each piece smooth or flatten and add filling. Place on parchment squares in the steamer."),
            S(++id, recipeId, 5, "Proof", "Final proof",
                20, 15,  30, 5, 28.0,
                "Proof in steamer with lid on but heat OFF. Buns should puff slightly and feel soft. Do not over-proof."),
            S(++id, recipeId, 6, "Bake",  "Steam",
                15, 12,  18, 2, 100.0,
                "Bring water to a vigorous simmer, then steam. Line lid with a cloth to prevent drip marks."),
            S(++id, recipeId, 7, "Bake",  "Rest with lid ajar",
                3,   2,   5, 1, null,
                "Tilt lid slightly for 2–3 min before removing entirely — prevents skin collapse from cold air shock."),
            S(++id, recipeId, 8, "Cool",  "Cool on rack",
                15, 10,  20, 5, null,
                "Remove from steamer and cool briefly on a rack. Best eaten warm within the hour."),
        ]);
    }

    private static void AddEnrichedSteps(List<RecipeStep> steps, int recipeId)
    {
        int id = recipeId * 100;
        steps.AddRange(
        [
            S(++id, recipeId,  1, "Mix",   "Prepare Tangzhong",
                10,  8,  15, 2, 65.0,
                "Cook flour (6% of total) + liquid (5× flour weight) to ~65 °C, stirring constantly until a thick, glossy paste. Starch gelatinises — this is the key to the pillowy crumb."),
            S(++id, recipeId,  2, "Rest",  "Cool Tangzhong",
                30, 20,  60, 10, null,
                "Spread on a plate or press cling film to the surface to prevent a skin. Must reach 25 °C before mixing — adding it hot will kill the yeast."),
            S(++id, recipeId,  3, "Mix",   "Mix dough (pre-butter)",
                15, 10,  20, 5, null,
                "Combine flour, milk, egg, sugar, salt, yeast, and cooled Tangzhong. Mix until shaggy, then knead to a smooth, non-sticky dough. No butter yet — gluten must develop first."),
            S(++id, recipeId,  4, "Mix",   "Add butter (window-pane)",
                15, 10,  25, 5, null,
                "Add cold butter in small cubes while the dough is mixing. Dough will look broken — keep going. Stop when it passes the window-pane test: silky, elastic, and no longer sticky."),
            S(++id, recipeId,  5, "Bulk",  "Bulk fermentation (until doubled)",
                60, 45,  90, 15, 27.0,
                "Cover and keep warm at 26–28 °C. Enriched doughs rise faster than sourdough — watch for doubling in size, not just time elapsed."),
            S(++id, recipeId,  6, "Shape", "Divide + pre-shape",
                10,  5,  15, 5, null,
                "Scale portions equally. Gently degas and pre-shape into rounds. Cover and rest 5 min."),
            S(++id, recipeId,  7, "Shape", "Bench rest",
                15, 10,  20, 5, null,
                "Rest covered — gluten tightens after pre-shape. This relaxation makes the final roll-and-fold possible without tearing."),
            S(++id, recipeId,  8, "Shape", "Final shape + tin",
                15, 10,  20, 5, null,
                "Roll each piece flat, fold the sides in, roll up tightly, and place seam-down in the greased tin. For Pullman, fill the tin 70% and add the lid."),
            S(++id, recipeId,  9, "Proof", "Final proof (80–90% tin height)",
                60, 45,  90, 15, 29.0,
                "Proof at 28–30 °C until dough is 80–90% of tin height (open tin: dome 2–3 cm above rim). Over-proofing collapses the crumb after baking."),
            S(++id, recipeId, 10, "Bake",  "Bake (Pullman lidded / open tin)",
                30, 25,  35, 5, 190.0,
                "Pullman: bake with lid closed for 25 min, then remove lid for 5 min for colour. Open tin: bake uncovered 30 min. Internal temp target ~88 °C."),
            S(++id, recipeId, 11, "Cool",  "Cool on rack",
                60, 30, 120, 15, null,
                "Cool in tin 5 min, then unmould onto a rack. Slice only when fully cool — the crumb is still setting as it cools and cuts gummy if sliced hot."),
        ]);
    }

    private static RecipeStep S(
        int id, int recipeId, int order, string phase, string name,
        int defaultMin, int min, int max, int stepMin,
        double? targetTempC, string? description) =>
        new()
        {
            Id = id, RecipeId = recipeId, Order = order,
            Phase = phase, Name = name,
            DefaultDurationMin = defaultMin, MinDurationMin = min, MaxDurationMin = max,
            StepMin = stepMin, TargetTempC = targetTempC, Description = description,
        };
}

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
        new() { Id = 1, Name = "Modern wheat",     Ploidy = "Hexaploid",  GlutenStrength = "Strong",            HydrationAdjustPct =   0, MaxAutolyseMinutes = 60, NeedsBinder = false },
        new() { Id = 2, Name = "Whole grain",       Ploidy = "Hexaploid",  GlutenStrength = "Strong",            HydrationAdjustPct =   5, MaxAutolyseMinutes = 60, NeedsBinder = false },
        new() { Id = 3, Name = "Rye",               Ploidy = null,         GlutenStrength = "Very low (bran)",   HydrationAdjustPct =  15, MaxAutolyseMinutes = 60, NeedsBinder = false },
        new() { Id = 4, Name = "Spelt",             Ploidy = "Hexaploid",  GlutenStrength = "Moderate (fragile)", HydrationAdjustPct =  -5, MaxAutolyseMinutes = 30, NeedsBinder = false },
        new() { Id = 5, Name = "Einkorn",           Ploidy = "Diploid",    GlutenStrength = "Very weak",         HydrationAdjustPct = -15, MaxAutolyseMinutes = 15, NeedsBinder = false },
        new() { Id = 6, Name = "Emmer (farro)",     Ploidy = "Tetraploid", GlutenStrength = "Weak",              HydrationAdjustPct = -10, MaxAutolyseMinutes = 20, NeedsBinder = false },
        new() { Id = 7, Name = "Kamut (khorasan)",  Ploidy = "Tetraploid", GlutenStrength = "Strong",            HydrationAdjustPct =  10, MaxAutolyseMinutes = 45, NeedsBinder = false },
        new() { Id = 8, Name = "Teff",              Ploidy = null,         GlutenStrength = "None (GF)",         HydrationAdjustPct =   0, MaxAutolyseMinutes =  0, NeedsBinder = true  },
        new() { Id = 9, Name = "Sorghum",           Ploidy = null,         GlutenStrength = "None (GF)",         HydrationAdjustPct =   0, MaxAutolyseMinutes =  0, NeedsBinder = true  },
    ];

    // Recipes: grainProfileId × method.  Rye/WholeGrain fall back to Modern wheat via BakeSessionService.
    public static readonly Recipe[] Recipes =
    [
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

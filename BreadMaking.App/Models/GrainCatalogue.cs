namespace BreadMaking.App.Models;

public static class GrainCatalogue
{
    // To add a new grain: (1) add enum value to FlourType, (2) add an entry here. That's it.
    public static readonly Dictionary<FlourType, GrainProfile> All = new()
    {
        [FlourType.White] = new GrainProfile(
            DisplayName: "White / Strong",
            Icon: "🌾",
            Description: "High-protein bread flour. Benefits most from a rest at any hydration above 65%.",
            IsGlutenFree: false,
            IsEnriched: false,
            IsLowGlutenAncient: false,
            MaxRestMinutes: 60,
            SoakerMinutes: 0,
            HydrationNote: "Standard hydration 65–80% — follow your recipe.",
            MixingNote: "Normal kneading; use the windowpane test."
        ),
        [FlourType.WholeGrain] = new GrainProfile(
            DisplayName: "Whole Grain / Wheat",
            Icon: "🌿",
            Description: "Contains bran that cuts gluten. Needs a longer rest — 45–60 min minimum.",
            IsGlutenFree: false,
            IsEnriched: false,
            IsLowGlutenAncient: false,
            MaxRestMinutes: 60,
            SoakerMinutes: 0,
            HydrationNote: "Bran absorbs water — add 5% extra hydration vs. white flour.",
            MixingNote: "Normal kneading; bran is sharp so don't over-develop."
        ),
        [FlourType.Rye] = new GrainProfile(
            DisplayName: "Rye",
            Icon: "🍂",
            Description: "Very high bran content, almost no gluten. Long rest helps water absorption.",
            IsGlutenFree: false,
            IsEnriched: false,
            IsLowGlutenAncient: false,
            MaxRestMinutes: 60,
            SoakerMinutes: 0,
            HydrationNote: "Rye is very thirsty — expect 75–85% hydration in high-rye loaves.",
            MixingNote: "Stir to combine rather than knead; pure rye forms a thick batter, not dough."
        ),
        [FlourType.Spelt] = new GrainProfile(
            DisplayName: "Spelt",
            Icon: "🌱",
            Description: "Ancient grain with fragile gluten. Rest improves extensibility — keep it short (20–30 min).",
            IsGlutenFree: false,
            IsEnriched: false,
            IsLowGlutenAncient: false,
            MaxRestMinutes: 30,
            SoakerMinutes: 0,
            HydrationNote: "Similar to wheat or 5% lower — spelt absorbs water quickly, add gradually.",
            MixingNote: "Stop the moment the dough is smooth — over-mixing destroys spelt gluten."
        ),
        [FlourType.Enriched] = new GrainProfile(
            DisplayName: "Enriched (Brioche / Milk Bread)",
            Icon: "🧈",
            Description: "Fat and eggs prevent proper gluten formation. Skip the rest entirely.",
            IsGlutenFree: false,
            IsEnriched: true,
            IsLowGlutenAncient: false,
            MaxRestMinutes: 0,
            SoakerMinutes: 0,
            HydrationNote: "Follow enriched recipe exactly — fat content governs dough texture.",
            MixingNote: "Develop gluten before adding fat; use the slap-and-fold or stand mixer."
        ),
        [FlourType.Einkorn] = new GrainProfile(
            DisplayName: "Einkorn",
            Icon: "🟡",
            Description: "Oldest cultivated wheat — very fragile gluten. Skip the rest or ≤15 min only.",
            IsGlutenFree: false,
            IsEnriched: false,
            IsLowGlutenAncient: true,
            MaxRestMinutes: 15,
            SoakerMinutes: 0,
            HydrationNote: "Reduce by 10–20% vs. your usual wheat recipe — einkorn absorbs less water.",
            MixingNote: "Folds only — never knead aggressively. Mix just to combine."
        ),
        [FlourType.Emmer] = new GrainProfile(
            DisplayName: "Emmer (Farro)",
            Icon: "🟤",
            Description: "Sticky, weak-gluten ancient wheat. A short autolyse of up to 20 min helps bran hydration.",
            IsGlutenFree: false,
            IsEnriched: false,
            IsLowGlutenAncient: true,
            MaxRestMinutes: 20,
            SoakerMinutes: 0,
            HydrationNote: "Reduce by 5–15% vs. modern wheat — blend 30–50% emmer with bread wheat for structure.",
            MixingNote: "Gentle folds; stop as soon as the dough comes together."
        ),
        [FlourType.Kamut] = new GrainProfile(
            DisplayName: "Kamut (Khorasan)",
            Icon: "🌕",
            Description: "Large-kernelled ancient wheat with strong, forgiving gluten. Full autolyse 30–45 min.",
            IsGlutenFree: false,
            IsEnriched: false,
            IsLowGlutenAncient: false,
            MaxRestMinutes: 45,
            SoakerMinutes: 0,
            HydrationNote: "Increase by 5–15% — Kamut is thirsty. Add water progressively until supple.",
            MixingNote: "Normal kneading is fine — Kamut tolerates and benefits from gluten development."
        ),
        [FlourType.Teff] = new GrainProfile(
            DisplayName: "Teff",
            Icon: "🫘",
            Description: "Gluten-free East-African grass. Use a soaker — autolyse concept does not apply.",
            IsGlutenFree: true,
            IsEnriched: false,
            IsLowGlutenAncient: false,
            MaxRestMinutes: 60,
            SoakerMinutes: 40,
            HydrationNote: "Batter consistency — effective hydration 90–120%. Always add a binder (psyllium/xanthan/egg).",
            MixingNote: "Whisk to combine; do not over-work or the crumb turns gummy."
        ),
        [FlourType.Sorghum] = new GrainProfile(
            DisplayName: "Sorghum",
            Icon: "⚪",
            Description: "Gluten-free, mild-flavoured workhorse flour. Soaker 30–60 min reduces grittiness.",
            IsGlutenFree: true,
            IsEnriched: false,
            IsLowGlutenAncient: false,
            MaxRestMinutes: 60,
            SoakerMinutes: 45,
            HydrationNote: "Low water-holding — blend with rice flour or starches and add extra moisture.",
            MixingNote: "Always blend (30–50% of GF mix); never use as a stand-alone kneaded dough."
        ),
    };
}

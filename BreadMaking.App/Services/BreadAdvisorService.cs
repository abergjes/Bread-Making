using BreadMaking.App.Models;

namespace BreadMaking.App.Services;

public class BreadAdvisorService
{
    private record TempBand(
        double MaxTemp,
        int AutoMin, int AutoSweet, int AutoMax,
        int FerroMin, int FerroSweet, int FerroMax,
        string FerroRisk
    );

    // Rest duration ranges from document v3 Tables 7.1 and 7.2
    private static readonly TempBand[] TempBands =
    [
        new(18, 60, 75, 90,  90, 105, 120, "Low"),
        new(20, 45, 60, 75,  75,  90, 105, "Low"),
        new(22, 40, 50, 60,  60,  75,  90, "Moderate"),
        new(24, 30, 37, 45,  45,  57,  70, "Moderate"),
        new(26, 25, 30, 35,  35,  45,  55, "High"),
        new(double.MaxValue, 20, 25, 30, 25, 32, 40, "Very High")
    ];

    public BreadRecommendation GetRecommendation(BreadInputs inputs)
    {
        var (grain, blendNote) = ResolveGrain(inputs);
        var rec = GetRecommendationCore(inputs, grain);
        rec.BlendNote = blendNote;
        return rec;
    }

    // ── Blend resolution ─────────────────────────────────────────────────────

    private static (GrainProfile Grain, string BlendNote) ResolveGrain(BreadInputs inputs)
    {
        var primary = GrainCatalogue.All[inputs.FlourType];
        if (!inputs.IsBlend)
            return (primary, "");

        var secondary = GrainCatalogue.All[inputs.SecondaryFlourType!.Value];
        int pPct = inputs.PrimaryFlourPercent;
        int sPct = inputs.SecondaryFlourPercent;

        bool pWeak  = IsFragileGrain(inputs.FlourType, primary);
        bool sWeak  = IsFragileGrain(inputs.SecondaryFlourType.Value, secondary);
        int weakPct = (pWeak ? pPct : 0) + (sWeak ? sPct : 0);
        int gfPct   = (primary.IsGlutenFree ? pPct : 0) + (secondary.IsGlutenFree ? sPct : 0);

        bool effectiveGF       = gfPct > 75;
        bool effectiveEnriched = primary.IsEnriched && pPct >= 50;
        bool effectiveAncient  = weakPct >= 40 && (primary.IsLowGlutenAncient || secondary.IsLowGlutenAncient);

        // Max rest from Table 10.4 (fragile/low-gluten fraction)
        int tableMax     = weakPct switch { <= 20 => 45, <= 40 => 40, <= 60 => 30, <= 80 => 20, _ => 15 };
        int weightedMax  = (pPct * primary.MaxRestMinutes + sPct * secondary.MaxRestMinutes) / 100;
        int effectiveMax = effectiveGF ? 0 : Math.Min(tableMax, Math.Max(weightedMax, tableMax - 10));
        int soakerMins   = effectiveGF ? Math.Max(primary.SoakerMinutes, secondary.SoakerMinutes) : 0;

        string hydAdj  = weakPct switch { <= 20 => "+0–3%", <= 40 => "+3–6%", <= 60 => "+5–10%", <= 80 => "+8–15%", _ => "grain-specific" };
        string hydNote = $"With {weakPct}% fragile-gluten flour in the blend, target approximately {hydAdj} hydration vs. your usual recipe.";
        string mixNote = weakPct > 40
            ? "Stretch-and-folds only — aggressive kneading tears the weak gluten network."
            : primary.MixingNote;

        var blendedGrain = new GrainProfile(
            DisplayName:        $"{pPct}% {primary.DisplayName} / {sPct}% {secondary.DisplayName}",
            Icon:               primary.Icon,
            Description:        "",
            IsGlutenFree:       effectiveGF,
            IsEnriched:         effectiveEnriched,
            IsLowGlutenAncient: effectiveAncient,
            MaxRestMinutes:     effectiveMax,
            SoakerMinutes:      soakerMins,
            HydrationNote:      hydNote,
            MixingNote:         mixNote
        );

        string blendNote = BuildBlendNote(primary, secondary, pPct, sPct, weakPct, gfPct, hydAdj, effectiveMax, effectiveGF);
        return (blendedGrain, blendNote);
    }

    // A grain is "fragile" (draws down the gluten budget) if it's an ancient low-gluten wheat,
    // Spelt, or gluten-free. Standard wholegrain/rye carry decent structure and are treated separately.
    private static bool IsFragileGrain(FlourType type, GrainProfile profile)
        => profile.IsLowGlutenAncient || profile.IsGlutenFree || type == FlourType.Spelt;

    private static string BuildBlendNote(
        GrainProfile p, GrainProfile s,
        int pPct, int sPct, int weakPct, int gfPct,
        string hydAdj, int maxRest, bool isGF)
    {
        string family;
        if (isGF)                                            family = "gluten-free blend — treat as a batter, no kneading";
        else if (gfPct > 0)                                  family = $"wheat blend with {gfPct}% gluten-free flour";
        else if (p.IsLowGlutenAncient && s.IsLowGlutenAncient) family = "ancient + ancient blend — no strong-wheat backbone";
        else if (p.IsLowGlutenAncient || s.IsLowGlutenAncient) family = "ancient grain + strong wheat";
        else                                                 family = "wholegrain / specialty blend";

        var parts = new List<string> { $"{pPct}% {p.DisplayName} + {sPct}% {s.DisplayName} — {family}." };

        if (!isGF)
        {
            parts.Add(weakPct switch
            {
                <= 20 => $"At {weakPct}% fragile-gluten flour the dough behaves close to a standard wheat bake.",
                <= 40 => $"At {weakPct}% you're in the classic sweet-spot: solid structure with meaningful flavour from the specialty grain.",
                <= 60 => $"At {weakPct}% fragile-gluten flour expect a moderately open crumb and stickier dough — shape with care.",
                _     => $"At {weakPct}% fragile-gluten flour the dough will be slack and sticky. Use a banneton or tin for support."
            });
            parts.Add($"Hydration: approximately {hydAdj} vs. your usual recipe.");
            parts.Add(maxRest > 0
                ? $"Rest: keep the autolyse to {maxRest} min or less for this flour ratio."
                : "Rest: skip the autolyse — the gluten is too fragile to benefit.");
        }
        else
        {
            parts.Add("Add a binder (psyllium husk, xanthan gum, or egg) to replace gluten structure. Use a soaker rather than an autolyse.");
        }

        return string.Join(" ", parts);
    }

    // ── Core recommendation logic ─────────────────────────────────────────────

    private BreadRecommendation GetRecommendationCore(BreadInputs inputs, GrainProfile grain)
    {
        var band = GetTempBand(inputs.KitchenTemperatureC);

        if (grain.IsSteamed)
            return BuildSteamed(inputs, grain);

        if (grain.IsGlutenFree)
            return BuildSoaker(inputs, grain);

        if (grain.IsEnriched)
            return BuildEnriched(inputs, grain);

        if (grain.IsLowGlutenAncient)
        {
            if (grain.MaxRestMinutes == 0)
                return BuildSkip(inputs, $"{grain.DisplayName} has very fragile gluten that degrades quickly. Skip the rest entirely and proceed directly to gentle mixing.");
            var (rMin, rSweet, rMax) = CapToGrain(band.AutoMin, band.AutoSweet, band.AutoMax, grain.MaxRestMinutes);
            return BuildAutolyse(inputs, rMin, rSweet, rMax, $"{grain.DisplayName} has very extensible but weak gluten — a long rest only worsens the slack, sticky dough. Keep the autolyse to {grain.MaxRestMinutes} minutes at most.");
        }

        if (!inputs.HasSourdoughStarter && inputs.HydrationPercent < 65)
            return BuildSkip(inputs, "Low-hydration doughs (under 65%) with commercial yeast form gluten readily during kneading. A rest phase adds time without meaningful benefit.");

        if (!inputs.HasSourdoughStarter)
        {
            if (inputs.HasMeaningfulBranContent)
            {
                var minDur = Math.Max(band.AutoSweet, 45);
                return BuildAutolyse(inputs, minDur, minDur, Math.Max(band.AutoMax, minDur), "No sourdough starter is available, so fermentolyse is not an option. Whole grain and rye flours need at least 45 minutes to fully hydrate the bran.");
            }
            var (aMin, aSweet, aMax) = CapToGrain(band.AutoMin, band.AutoSweet, band.AutoMax, grain.MaxRestMinutes);
            return BuildAutolyse(inputs, aMin, aSweet, aMax, "No sourdough starter is available, so fermentolyse is not an option. Autolyse gives you better extensibility and reduces kneading time with commercial yeast.");
        }

        if (inputs.StarterActivity == StarterActivity.PastPeak)
        {
            var (aMin, aSweet, aMax) = CapToGrain(band.AutoMin, band.AutoSweet, band.AutoMax, grain.MaxRestMinutes);
            return BuildAutolyse(inputs, aMin, aSweet, aMax, "Your starter is past peak. Using it in fermentolyse would introduce excessive acidity, risking over-weakened gluten. Use autolyse instead and add the starter after the rest.");
        }

        if (inputs.StarterActivity == StarterActivity.JustFed)
        {
            var (aMin, aSweet, aMax) = CapToGrain(band.AutoMin, band.AutoSweet, band.AutoMax, grain.MaxRestMinutes);
            return BuildAutolyse(inputs, aMin, aSweet, aMax, "A freshly-fed starter hasn't reached peak activity yet. Fermentolyse relies on active fermentation — use autolyse now and wait until the starter peaks before your next bake.");
        }

        if (inputs.KitchenTemperatureC > 24)
        {
            var (aMin, aSweet, aMax) = CapToGrain(band.AutoMin, band.AutoSweet, band.AutoMax, grain.MaxRestMinutes);
            return BuildAutolyse(inputs, aMin, aSweet, aMax, $"At {inputs.KitchenTemperatureC:F0}°C your kitchen is warm enough that fermentolyse carries a high risk of over-fermentation. Autolyse keeps things safe and predictable.");
        }

        if (inputs.HydrationPercent <= 68)
        {
            var (aMin, aSweet, aMax) = CapToGrain(band.AutoMin, band.AutoSweet, band.AutoMax, grain.MaxRestMinutes);
            return BuildAutolyse(inputs, aMin, aSweet, aMax, "Stiff doughs (≤68% hydration) carry elevated fermentolyse risk — reduced water concentrates organic acids and can over-weaken the gluten. Autolyse gives you full control.");
        }

        if (inputs.Experience == BakerExperience.Novice)
        {
            var (aMin, aSweet, aMax) = CapToGrain(band.AutoMin, band.AutoSweet, band.AutoMax, grain.MaxRestMinutes);
            return BuildAutolyse(inputs, aMin, aSweet, aMax, "As a beginner baker, autolyse is the safer and more predictable choice. Fermentolyse requires experience reading dough feel and starter activity — master autolyse first.");
        }

        if (inputs.DominantFlourType is FlourType.Spelt or FlourType.Kamut)
        {
            var (aMin, aSweet, aMax) = CapToGrain(band.AutoMin, band.AutoSweet, band.AutoMax, grain.MaxRestMinutes);
            return BuildAutolyse(inputs, aMin, aSweet, aMax, $"{grain.DisplayName} benefits from a controlled autolyse of up to {grain.MaxRestMinutes} minutes. The gluten is present but sensitive — keep the rest short and precise.");
        }

        if (inputs.HasMeaningfulBranContent)
        {
            var fSweet = Math.Max(band.FerroSweet, 60);
            return BuildFermentolyse(inputs, band.FerroMin, fSweet, band.FerroMax, "Whole grain and rye flours contain bran that cuts through gluten strands. The organic acids produced during fermentolyse help condition the dough and counteract some of that weakening effect.");
        }

        if (inputs.KitchenTemperatureC < 20)
            return BuildFermentolyse(inputs, band.FerroMin, band.FerroSweet, band.FerroMax, $"At {inputs.KitchenTemperatureC:F0}°C your kitchen is cool, keeping fermentation slow and controlled. This is an ideal temperature for fermentolyse — you get the flavour benefit with very low over-fermentation risk.");

        if (inputs.FlavourGoal == FlavourGoal.ComplexTangy)
            return BuildFermentolyse(inputs, band.FerroMin, band.FerroSweet, band.FerroMax, "For a complex, tangy sourdough profile, fermentolyse gives you a head start: lactic acid bacteria begin producing organic acids during the rest itself, compressing total fermentation time and deepening flavour complexity.");

        if (inputs.HydrationPercent >= 75 && inputs.FlavourGoal == FlavourGoal.MildOpenCrumb)
        {
            var (aMin, aSweet, aMax) = CapToGrain(band.AutoMin, band.AutoSweet, band.AutoMax, grain.MaxRestMinutes);
            return BuildAutolyse(inputs, aMin, aSweet, aMax, "For a mild, open crumb at high hydration, autolyse delivers the cleanest gluten development. The passive rest hydrates the flour fully and builds extensibility without the unpredictability of early fermentation.");
        }

        return BuildFermentolyse(inputs, band.FerroMin, band.FerroSweet, band.FerroMax,
            "Your conditions are well-suited to fermentolyse. With a peak-activity starter, moderate temperature, and well-suited flour, you'll gain earlier flavour development and slightly shorter bulk fermentation time.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static TempBand GetTempBand(double tempC)
    {
        foreach (var b in TempBands)
            if (tempC <= b.MaxTemp)
                return b;
        return TempBands[^1];
    }

    private static (int Min, int Sweet, int Max) CapToGrain(int min, int sweet, int max, int grainMax)
        => (Math.Min(min, grainMax), Math.Min(sweet, grainMax), Math.Min(max, grainMax));

    // ── Builders ─────────────────────────────────────────────────────────────

    private BreadRecommendation BuildAutolyse(BreadInputs inputs, int restMin, int restSweet, int restMax, string reason)
    {
        return new BreadRecommendation
        {
            Method = RestMethod.Autolyse,
            RestDurationMin = restMin,
            RestDurationSweetSpot = restSweet,
            RestDurationMax = restMax,
            Headline = "Use Autolyse",
            Reason = reason,
            RiskLevel = "Low",
            Pros = AutolysePros(inputs),
            Cons = AutolyseCons(inputs),
            Tips = AutolyseTips(inputs),
            Timeline = BuildTimeline(inputs, RestMethod.Autolyse, restSweet)
        };
    }

    private BreadRecommendation BuildFermentolyse(BreadInputs inputs, int restMin, int restSweet, int restMax, string reason)
    {
        return new BreadRecommendation
        {
            Method = RestMethod.Fermentolyse,
            RestDurationMin = restMin,
            RestDurationSweetSpot = restSweet,
            RestDurationMax = restMax,
            Headline = "Use Fermentolyse",
            Reason = reason,
            RiskLevel = GetTempBand(inputs.KitchenTemperatureC).FerroRisk,
            Pros = FermentolysePros(inputs),
            Cons = FermentolyseCons(inputs),
            Tips = FermentolyseTips(inputs),
            Timeline = BuildTimeline(inputs, RestMethod.Fermentolyse, restSweet)
        };
    }

    private BreadRecommendation BuildSkip(BreadInputs inputs, string reason)
    {
        return new BreadRecommendation
        {
            Method = RestMethod.Skip,
            RestDurationMin = 0,
            RestDurationSweetSpot = 0,
            RestDurationMax = 0,
            Headline = "Skip the Rest Phase",
            Reason = reason,
            RiskLevel = "None",
            Tips = SkipTips(inputs),
            Timeline = BuildTimeline(inputs, RestMethod.Skip, 0)
        };
    }

    private BreadRecommendation BuildSoaker(BreadInputs inputs, GrainProfile grain)
    {
        return new BreadRecommendation
        {
            Method = RestMethod.Soaker,
            RestDurationMin = grain.SoakerMinutes,
            RestDurationSweetSpot = grain.SoakerMinutes,
            RestDurationMax = grain.SoakerMinutes,
            Headline = "Use a Soaker",
            Reason = $"{grain.DisplayName} is gluten-free — the autolyse and fermentolyse techniques rely on gluten development, which this grain cannot provide. Instead, use a soaker: rest the flour in its liquid for {grain.SoakerMinutes} minutes to fully hydrate the starch, reduce grittiness, and improve crumb texture.",
            RiskLevel = "Low",
            Tips = SoakerTips(inputs, grain),
            Timeline = BuildTimeline(inputs, RestMethod.Soaker, grain.SoakerMinutes)
        };
    }

    private BreadRecommendation BuildEnriched(BreadInputs inputs, GrainProfile grain)
    {
        return new BreadRecommendation
        {
            Method = RestMethod.Enriched,
            RestDurationMin = 0,
            RestDurationSweetSpot = 0,
            RestDurationMax = 0,
            Headline = "Enriched dough — Shokupan / Milk Bread",
            Reason = "Fat and eggs coat the gluten strands, so neither autolyse nor fermentolyse improve hydration here. Instead the dough relies on full kneading to develop the gluten before butter is added — a Japanese shokupan approach using Tangzhong (water-roux) for an extra-soft crumb.",
            RiskLevel = "Low",
            Tips = EnrichedTips(inputs),
            Timeline = BuildEnrichedTimeline()
        };
    }

    private static List<string> EnrichedTips(BreadInputs inputs) =>
    [
        "Prepare the Tangzhong first and let it cool fully before mixing — aim for 25 °C when it enters the dough.",
        "Develop gluten to near-windowpane stage BEFORE adding butter; fat added too early prevents gluten formation.",
        "Add cold butter in small pieces at medium mixer speed — the dough will look broken, then come back together.",
        "Final proof to 80–90% of pullman tin height (or until visibly domed in an open tin).",
        "Bake at 190 °C — lower than sourdough to prevent overbrowning from the sugar and milk.",
        "Internal temp target ~88 °C — lower than lean bread because enrichment raises the starch gelatinisation temperature.",
        "Cool fully in the tin for 5 min, then unmould and cool on a rack — cutting too early compresses the crumb.",
    ];

    private static List<TimelineStep> BuildEnrichedTimeline() =>
    [
        new() { Phase = "Prepare Tangzhong",         DurationLabel = "10 min",    TempLabel = "~65 °C",   Notes = "Cook flour (6% of total) + liquid (5× flour weight) to ~65 °C, stirring constantly. Starch gelatinises — this is the key to the soft crumb." },
        new() { Phase = "Cool Tangzhong",            DurationLabel = "30 min",    TempLabel = "25 °C",    Notes = "Spread on a plate or cover with film touching the surface. Must reach room temp before mixing into dough." },
        new() { Phase = "Mix dough (autolyse-style)", DurationLabel = "15 min",   TempLabel = "Ambient",  Notes = "Combine flour, milk, egg, sugar, yeast, and cooled Tangzhong. Mix until shaggy, then knead to smooth dough. No butter yet." },
        new() { Phase = "Add butter (window-pane)",   DurationLabel = "15 min",   TempLabel = "Ambient",  Notes = "Add cold butter in small cubes. Knead until fully absorbed and dough passes the window-pane test — smooth, silky, elastic." },
        new() { Phase = "Bulk (until doubled)",       DurationLabel = "60 min",   TempLabel = "26–28 °C", Notes = "Cover and proof in a warm spot. Enriched doughs rise faster than sourdough — watch for doubling, not time." },
        new() { Phase = "Divide + pre-shape",         DurationLabel = "10 min",   TempLabel = "Ambient",  Notes = "Scale portions equally. Light pre-shape into rounds. Rest 5 min covered." },
        new() { Phase = "Bench rest",                 DurationLabel = "15 min",   TempLabel = "Ambient",  Notes = "Gluten is tight after pre-shape — rest lets it relax before the final roll-and-fold." },
        new() { Phase = "Final shape + tin",          DurationLabel = "15 min",   TempLabel = "Ambient",  Notes = "Roll each piece flat, fold the sides in, roll up tightly, and place seam-down in the greased tin. For Pullman, fill tin 70%." },
        new() { Phase = "Final proof (80–90% height)", DurationLabel = "60 min",  TempLabel = "28–30 °C", Notes = "Proof until dough is 80–90% of tin height (open tin: dome 2–3 cm above rim). Over-proofing collapses the crumb after baking." },
        new() { Phase = "Bake (Pullman lid on / open tin)", DurationLabel = "30 min", TempLabel = "190 °C / 375 °F", Notes = "Pullman: bake with lid closed for 25 min, then remove lid for 5 min. Open tin: bake uncovered 30 min until deep golden." },
        new() { Phase = "Cool on rack",               DurationLabel = "60 min",   TempLabel = "Room temp",Notes = "Cool in tin 5 min, then unmould. Slice only when fully cool — the crumb sets as it cools." },
    ];

    private BreadRecommendation BuildSteamed(BreadInputs inputs, GrainProfile grain)
    {
        return new BreadRecommendation
        {
            Method = RestMethod.Steamed,
            RestDurationMin = 0,
            RestDurationSweetSpot = 0,
            RestDurationMax = 0,
            Headline = "Steamed bread — Mantou / Baozi",
            Reason = "Low-protein wheat flour (9–11% protein) cannot form the strong gluten needed for oven-spring. Instead the dough is steamed at 100 °C — moisture keeps the crust soft and the crumb fine and pillowy. Autolyse and fermentolyse do not apply here.",
            RiskLevel = "Low",
            Tips = SteamedTips(inputs),
            Timeline = BuildSteamedTimeline()
        };
    }

    private static List<string> SteamedTips(BreadInputs inputs) =>
    [
        "Mix until smooth — no windowpane test needed; low protein means short kneading (5–8 min).",
        "Proof until visibly puffed (about doubled) before steaming — 45–60 min at 25 °C.",
        "Vigorous simmer throughout: a lazy simmer produces dense, gummy crumb.",
        "Line the steamer lid with a cloth to catch condensation drops — lid drips cause wrinkles.",
        "Do not open the lid during the first 10 min — temperature shock collapses the crumb.",
        "Rest 2–3 min with the lid slightly ajar before removing; abrupt cooling causes skin wrinkling.",
        "Buns freeze well — steam from frozen for 8–10 min to refresh."
    ];

    private static List<TimelineStep> BuildSteamedTimeline() =>
    [
        new() { Phase = "Mix flour + water",         DurationLabel = "10 min",   TempLabel = "Ambient",  Notes = "Mix flour, water, yeast/starter, sugar, and oil until a smooth, non-sticky dough forms." },
        new() { Phase = "Bulk (until doubled)",      DurationLabel = "60 min",   TempLabel = "25–28 °C", Notes = "Cover. Wait for the dough to roughly double. Warm kitchen shortens this to 30–40 min." },
        new() { Phase = "Knock back + portion",      DurationLabel = "10 min",   TempLabel = "Ambient",  Notes = "Punch down. Divide into even pieces. Each piece should feel smooth and tight." },
        new() { Phase = "Final shape",               DurationLabel = "15 min",   TempLabel = "Ambient",  Notes = "Roll each piece into a smooth ball (Mantou) or fill and pleat (Baozi). Place on parchment squares." },
        new() { Phase = "Final proof",               DurationLabel = "20 min",   TempLabel = "25–28 °C", Notes = "Rest until noticeably puffed and soft to the touch — do not over-proof or the crumb collapses." },
        new() { Phase = "Steam",                     DurationLabel = "15 min",   TempLabel = "100 °C",   Notes = "Vigorous simmer. Cloth-lined lid. Do not open for first 10 min." },
        new() { Phase = "Rest in steamer (lid off)", DurationLabel = "3 min",    TempLabel = "Ambient",  Notes = "Lid ajar 2–3 min before full removal — prevents skin from wrinkling due to sudden cold air." },
        new() { Phase = "Cool on rack",              DurationLabel = "15 min",   TempLabel = "Room temp",Notes = "Enjoy warm. Cool fully before storing or freezing." },
    ];

    // ── Pros / Cons ───────────────────────────────────────────────────────────

    private List<string> AutolysePros(BreadInputs inputs)
    {
        var pros = new List<string>
        {
            "Safe and predictable — zero fermentation risk.",
            "Reduces mixing and kneading time by 30–50%."
        };
        if (inputs.HydrationPercent >= 75)
            pros.Add("Delivers the cleanest gluten development at high hydration for an open crumb.");
        return pros;
    }

    private List<string> AutolyseCons(BreadInputs inputs)
    {
        var cons = new List<string>();
        if (inputs.FlavourGoal == FlavourGoal.ComplexTangy)
            cons.Add("No acid development during the rest — won't contribute to a tangy flavour profile.");
        if (inputs.HasSourdoughStarter)
            cons.Add("Starter must be incorporated separately after the rest, not during.");
        return cons;
    }

    private List<string> FermentolysePros(BreadInputs inputs)
    {
        var pros = new List<string>
        {
            "Jumpstarts flavour — lactic acids begin building during the rest itself.",
            "Slightly shortens total bulk fermentation time."
        };
        if (inputs.KitchenTemperatureC < 20)
            pros.Add("Cool kitchen keeps fermentation slow and controlled — ideal conditions for this method.");
        return pros;
    }

    private List<string> FermentolyseCons(BreadInputs inputs)
    {
        var cons = new List<string>();
        if (inputs.KitchenTemperatureC >= 22)
            cons.Add("Over-fermentation risk rises with kitchen temperature — watch the dough closely.");
        cons.Add("Requires a well-fed starter at peak activity — sluggish starters won't benefit.");
        return cons;
    }

    // ── Tips ─────────────────────────────────────────────────────────────────

    private List<string> AutolyseTips(BreadInputs inputs)
    {
        var grain = GrainCatalogue.All[inputs.FlourType];
        var tips = new List<string>
        {
            "Mix only flour and water — no salt, no yeast, no starter yet.",
            "Cover with cling film or a damp cloth to prevent a skin forming.",
            "Use your dough temperature as your guide, not the clock."
        };
        if (inputs.KitchenTemperatureC > 25)
            tips.Add("Your kitchen is warm — use cold water to help keep dough temperature under control.");
        if (inputs.HasMeaningfulBranContent)
            tips.Add("Bran-heavy flours need longer hydration — don't rush the rest.");
        if (grain.IsLowGlutenAncient)
            tips.Add($"Handle {grain.DisplayName} gently — folds only, no aggressive kneading. The gluten is fragile.");
        if (inputs.IsBlend)
            tips.Add("For a blend, mix both flours together before adding water so they hydrate evenly.");
        if (!string.IsNullOrEmpty(grain.HydrationNote))
            tips.Add(grain.HydrationNote);
        return tips;
    }

    private List<string> FermentolyseTips(BreadInputs inputs)
    {
        var grain = GrainCatalogue.All[inputs.FlourType];
        var tips = new List<string>
        {
            "Include flour, water, and your starter — no salt.",
            "Use a starter at peak activity for best results.",
            "Watch the dough closely — signs of over-fermentation are bubbles and a slack, sticky texture.",
            "Cover and do not disturb during the rest."
        };
        if (inputs.KitchenTemperatureC >= 22)
            tips.Add("Keep the rest on the shorter end of the range at your kitchen temperature.");
        if (inputs.IsBlend)
            tips.Add("In a blend, mix all flours together with the water before adding starter for even hydration.");
        if (!string.IsNullOrEmpty(grain.MixingNote))
            tips.Add(grain.MixingNote);
        return tips;
    }

    private List<string> SkipTips(BreadInputs inputs)
    {
        var grain = GrainCatalogue.All[inputs.FlourType];
        var tips = new List<string>
        {
            "Proceed directly to mixing all ingredients together.",
            "Ensure you knead thoroughly to develop gluten.",
            "Use the windowpane test to confirm gluten development before bulk fermentation."
        };
        if (grain.IsLowGlutenAncient)
            tips.Add($"{grain.DisplayName} dough is sticky and slack — use gentle folds instead of kneading and handle minimally.");
        return tips;
    }

    private List<string> SoakerTips(BreadInputs inputs, GrainProfile grain)
    {
        return
        [
            $"Rest {grain.DisplayName} flour in all its liquid for {grain.SoakerMinutes} minutes before mixing.",
            "A binder is essential — add psyllium husk, xanthan gum, or egg to replace gluten structure.",
            grain.MixingNote,
            "Bake in a tin for loaves, or pour thin onto a hot griddle for flatbreads.",
            "Bake to a higher internal temperature (96–98°C) to ensure the crumb is fully set."
        ];
    }

    // ── Timeline ─────────────────────────────────────────────────────────────

    private List<TimelineStep> BuildTimeline(BreadInputs inputs, RestMethod method, int restMinutes)
    {
        bool isSourdough = inputs.HasSourdoughStarter;
        var steps = new List<TimelineStep>();

        if (method == RestMethod.Soaker)
        {
            steps.Add(new TimelineStep { Phase = "Whisk batter", DurationLabel = "5 min", TempLabel = "Ambient", Notes = "Combine flour, liquid, and binder (psyllium/xanthan/egg). No kneading." });
            steps.Add(new TimelineStep { Phase = "Soaker rest", DurationLabel = $"{restMinutes} min", TempLabel = "Ambient", Notes = "Cover and rest. Starch fully absorbs liquid — reduces grittiness and improves crumb texture.", IsRestPhase = true, RestMethod = method });
            steps.Add(new TimelineStep { Phase = "Add remaining ingredients", DurationLabel = "5 min", TempLabel = "Ambient", Notes = "Fold in salt, any sweetener, and fat. For sourdough add starter now." });
            steps.Add(new TimelineStep { Phase = "Ferment / proof", DurationLabel = "1–3 hours", TempLabel = "24–26°C", Notes = "Gluten-free batter rises more quickly and less dramatically than wheat dough — watch for bubbles." });
            steps.Add(new TimelineStep { Phase = "Bake in tin (covered)", DurationLabel = "20 min", TempLabel = "220°C / 430°F", Notes = "Cover with foil or a lid to trap steam and prevent over-crust early." });
            steps.Add(new TimelineStep { Phase = "Bake uncovered", DurationLabel = "25–35 min", TempLabel = "200°C / 390°F", Notes = "Until crust is set and internal temperature reaches 96–98°C." });
            steps.Add(new TimelineStep { Phase = "Cool on wire rack", DurationLabel = "1–2 hours", TempLabel = "Room temp", Notes = "Gluten-free crumb continues setting during cooling — do not cut early." });
            return steps;
        }

        if (method == RestMethod.Skip)
        {
            steps.Add(new TimelineStep { Phase = "Mix all ingredients", DurationLabel = "10–15 min", TempLabel = "Ambient", Notes = "Combine flour, water, salt, and yeast/starter together." });
            steps.Add(new TimelineStep { Phase = "Knead", DurationLabel = "8–12 min", TempLabel = "Ambient", Notes = "Develop gluten fully. Use windowpane test." });
        }
        else
        {
            var mixNote = method == RestMethod.Fermentolyse
                ? "Rough mix of flour, water, and starter — no salt yet."
                : "Rough mix of flour and water only — no salt, no yeast.";
            steps.Add(new TimelineStep { Phase = "Mix flour + water" + (method == RestMethod.Fermentolyse ? " + starter" : ""), DurationLabel = "10–15 min", TempLabel = "20–24°C", Notes = mixNote });
            steps.Add(new TimelineStep
            {
                Phase = method == RestMethod.Autolyse ? "Autolyse rest" : "Fermentolyse rest",
                DurationLabel = $"{restMinutes} min",
                TempLabel = $"{inputs.KitchenTemperatureC:F0}°C",
                Notes = method == RestMethod.Autolyse
                    ? "Cover and leave undisturbed. Enzymes hydrate flour and begin softening gluten."
                    : "Cover and leave undisturbed. Fermentation begins — watch for over-activity in warm kitchens.",
                IsRestPhase = true,
                RestMethod = method
            });
            steps.Add(new TimelineStep { Phase = "Add salt" + (isSourdough && method == RestMethod.Autolyse ? " + starter" : ""), DurationLabel = "5–10 min", TempLabel = "Ambient", Notes = "Dimple salt into the dough and fold to incorporate. Add starter now if using autolyse." });
        }

        if (isSourdough)
        {
            steps.Add(new TimelineStep { Phase = "Bulk fermentation", DurationLabel = "4–6 hours", TempLabel = "22–24°C", Notes = "4–6 sets of stretch & folds every 30 min. Dough should grow 50–75%." });
            steps.Add(new TimelineStep { Phase = "Pre-shape + bench rest", DurationLabel = "25–35 min", TempLabel = "Ambient", Notes = "Gentle pre-shape, then 20–30 min uncovered bench rest." });
            steps.Add(new TimelineStep { Phase = "Final shape", DurationLabel = "5–10 min", TempLabel = "Ambient", Notes = "Build surface tension. Place in floured banneton seam-side up." });
            steps.Add(new TimelineStep { Phase = "Cold proof", DurationLabel = "8–16 hours", TempLabel = "4–5°C (fridge)", Notes = "Retard in the fridge overnight. Develops flavour and structure." });
            steps.Add(new TimelineStep { Phase = "Preheat + Dutch oven", DurationLabel = "45–60 min", TempLabel = "250°C / 480°F", Notes = "Cast iron must be screaming hot before the loaf goes in." });
            steps.Add(new TimelineStep { Phase = "Bake covered", DurationLabel = "20 min", TempLabel = "250°C / 480°F", Notes = "Steam inside the Dutch oven drives oven spring and crust formation." });
            steps.Add(new TimelineStep { Phase = "Bake uncovered", DurationLabel = "20–25 min", TempLabel = "220°C / 425°F", Notes = "Achieve deep caramelised crust. Internal temp should reach 96–98°C." });
        }
        else
        {
            steps.Add(new TimelineStep { Phase = "First rise (bulk)", DurationLabel = "1–1.5 hours", TempLabel = "24°C / 75°F", Notes = "Until doubled in volume." });
            steps.Add(new TimelineStep { Phase = "Punch down + pre-shape", DurationLabel = "5 min", TempLabel = "Ambient", Notes = "Gentle handling." });
            steps.Add(new TimelineStep { Phase = "Bench rest", DurationLabel = "15–20 min", TempLabel = "Ambient", Notes = "Covered — let gluten relax before final shape." });
            steps.Add(new TimelineStep { Phase = "Final shape", DurationLabel = "5–10 min", TempLabel = "Ambient", Notes = "Shape to tin or free-form." });
            steps.Add(new TimelineStep { Phase = "Second proof", DurationLabel = "45–60 min", TempLabel = "24–26°C", Notes = "Until 1.5× original volume." });
            steps.Add(new TimelineStep { Phase = "Bake with steam", DurationLabel = "10–15 min", TempLabel = "230°C / 450°F", Notes = "Steam pan or spray for open crust." });
            steps.Add(new TimelineStep { Phase = "Bake without steam", DurationLabel = "20–25 min", TempLabel = "210°C / 410°F", Notes = "Until deep golden and hollow-sounding when tapped." });
        }

        steps.Add(new TimelineStep { Phase = "Cool on wire rack", DurationLabel = isSourdough ? "1–2 hours" : "30–60 min", TempLabel = "Room temp", Notes = "Do not cut early — the crumb is still setting during cooling." });
        return steps;
    }
}

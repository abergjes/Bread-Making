using BreadMaking.App.Models;

namespace BreadMaking.App.Services;

public class BreadAdvisorService
{
    // Sweet-spot rest durations from the document v2 temperature tables (Section 7)
    private static readonly (double MaxTemp, int AutolyseSweetSpot, int FermentolyseSweetSpot)[] TempTable =
    [
        (16, 70, 110),
        (18, 60,  90),
        (20, 50,  65),
        (22, 45,  50),
        (24, 35,  38),
        (double.MaxValue, 30, 28)
    ];

    private static readonly (double MaxTemp, string Risk)[] FermentolyseRiskTable =
    [
        (20, "Low"),
        (22, "Moderate"),
        (double.MaxValue, "High")
    ];

    public BreadRecommendation GetRecommendation(BreadInputs inputs)
    {
        var grain = GrainCatalogue.All[inputs.FlourType];
        var (autolyseMin, fermentolyseMin) = GetSweetSpots(inputs.KitchenTemperatureC);

        // Gluten-free grains — autolyse concept does not apply; use a soaker
        if (grain.IsGlutenFree)
            return BuildSoaker(inputs, grain);

        // Enriched doughs — fat and eggs interfere with gluten formation
        if (grain.IsEnriched)
            return BuildSkip(inputs, "Enriched doughs (brioche, milk bread) contain fat and eggs that interfere with gluten hydration. Neither method adds benefit here — proceed directly to mixing.");

        // Low-gluten ancient wheats (Einkorn, Emmer) — long rest worsens slack dough
        if (grain.IsLowGlutenAncient)
        {
            if (grain.MaxRestMinutes == 0)
                return BuildSkip(inputs, $"{grain.DisplayName} has very fragile gluten that degrades quickly. Skip the rest entirely and proceed directly to gentle mixing.");
            var shortRest = Math.Min(autolyseMin, grain.MaxRestMinutes);
            return BuildAutolyse(inputs, shortRest, $"{grain.DisplayName} has very extensible but weak gluten — a long rest only worsens the slack, sticky dough. Keep the autolyse to {grain.MaxRestMinutes} minutes at most.");
        }

        // Low-hydration commercial yeast — gluten forms easily without help
        if (!inputs.HasSourdoughStarter && inputs.HydrationPercent < 65)
            return BuildSkip(inputs, "Low-hydration doughs (under 65%) with commercial yeast form gluten readily during kneading. A rest phase adds time without meaningful benefit.");

        // No starter — must use autolyse or skip
        if (!inputs.HasSourdoughStarter)
        {
            var duration = inputs.FlourType is FlourType.WholeGrain or FlourType.Rye
                ? Math.Max(autolyseMin, 45)
                : Math.Min(autolyseMin, grain.MaxRestMinutes);
            return BuildAutolyse(inputs, duration, "No sourdough starter is available, so fermentolyse is not an option. Autolyse gives you better extensibility and reduces kneading time with commercial yeast.");
        }

        // Starter past peak — too acidic for fermentolyse
        if (inputs.StarterActivity == StarterActivity.PastPeak)
        {
            var dur = Math.Min(autolyseMin, grain.MaxRestMinutes);
            return BuildAutolyse(inputs, dur, "Your starter is past peak. Using it in fermentolyse would introduce excessive acidity, risking over-weakened gluten. Use autolyse instead and add the starter after the rest.");
        }

        // Starter just fed — not yet active enough for fermentolyse
        if (inputs.StarterActivity == StarterActivity.JustFed)
        {
            var dur = Math.Min(autolyseMin, grain.MaxRestMinutes);
            return BuildAutolyse(inputs, dur, "A freshly-fed starter hasn't reached peak activity yet. Fermentolyse relies on active fermentation — use autolyse now and wait until the starter peaks before your next bake.");
        }

        // Hot kitchen — over-fermentation risk too high
        if (inputs.KitchenTemperatureC > 24)
        {
            var dur = Math.Min(autolyseMin, grain.MaxRestMinutes);
            return BuildAutolyse(inputs, dur, $"At {inputs.KitchenTemperatureC:F0}°C your kitchen is warm enough that fermentolyse carries a high risk of over-fermentation. Autolyse keeps things safe and predictable.");
        }

        // Novice baker — steer towards the safer method
        if (inputs.Experience == BakerExperience.Novice)
        {
            var dur = Math.Min(autolyseMin, grain.MaxRestMinutes);
            return BuildAutolyse(inputs, dur, "As a beginner baker, autolyse is the safer and more predictable choice. Fermentolyse requires experience reading dough feel and starter activity — master autolyse first.");
        }

        // Spelt / Kamut sourdough — short autolyse (fragile or thirsty; fermentolyse OK for whole versions)
        if (inputs.FlourType is FlourType.Spelt or FlourType.Kamut)
        {
            var dur = Math.Min(autolyseMin, grain.MaxRestMinutes);
            return BuildAutolyse(inputs, dur, $"{grain.DisplayName} benefits from a controlled autolyse of up to {grain.MaxRestMinutes} minutes. The gluten is present but sensitive — keep the rest short and precise.");
        }

        // Rye-heavy or whole wheat — acids help manage bran
        if (inputs.FlourType is FlourType.Rye or FlourType.WholeGrain)
        {
            var dur = Math.Max(fermentolyseMin, 60);
            return BuildFermentolyse(inputs, dur, "Whole grain and rye flours contain bran that cuts through gluten strands. The organic acids produced during fermentolyse help condition the dough and counteract some of that weakening effect.");
        }

        // Cool kitchen — fermentolyse is safer and gives flavour benefit
        if (inputs.KitchenTemperatureC < 20)
            return BuildFermentolyse(inputs, fermentolyseMin, $"At {inputs.KitchenTemperatureC:F0}°C your kitchen is cool, keeping fermentation slow and controlled. This is an ideal temperature for fermentolyse — you get the flavour benefit with very low over-fermentation risk.");

        // Complex tangy flavour goal — fermentolyse wins
        if (inputs.FlavourGoal == FlavourGoal.ComplexTangy)
            return BuildFermentolyse(inputs, fermentolyseMin, "For a complex, tangy sourdough profile, fermentolyse gives you a head start: lactic acid bacteria begin producing organic acids during the rest itself, compressing total fermentation time and deepening flavour complexity.");

        // High hydration — both work, but autolyse gives cleaner gluten
        if (inputs.HydrationPercent >= 75 && inputs.FlavourGoal == FlavourGoal.MildOpenCrumb)
        {
            var dur = Math.Min(autolyseMin, grain.MaxRestMinutes);
            return BuildAutolyse(inputs, dur, "For a mild, open crumb at high hydration, autolyse delivers the cleanest gluten development. The passive rest hydrates the flour fully and builds extensibility without the unpredictability of early fermentation.");
        }

        // Default moderate conditions — fermentolyse for sourdough with peak starter
        return BuildFermentolyse(inputs, Math.Min(fermentolyseMin, grain.MaxRestMinutes),
            "Your conditions are well-suited to fermentolyse. With a peak-activity starter, moderate temperature, and well-suited flour, you'll gain earlier flavour development and slightly shorter bulk fermentation time.");
    }

    private (int Autolyse, int Fermentolyse) GetSweetSpots(double tempC)
    {
        foreach (var row in TempTable)
            if (tempC <= row.MaxTemp)
                return (row.AutolyseSweetSpot, row.FermentolyseSweetSpot);
        return (25, 32);
    }

    private string GetFermentolyseRisk(double tempC)
    {
        foreach (var row in FermentolyseRiskTable)
            if (tempC <= row.MaxTemp)
                return row.Risk;
        return "High";
    }

    private BreadRecommendation BuildAutolyse(BreadInputs inputs, int minutes, string reason)
    {
        return new BreadRecommendation
        {
            Method = RestMethod.Autolyse,
            RestDurationMin = minutes,
            Headline = "Use Autolyse",
            Reason = reason,
            RiskLevel = "Low",
            Tips = AutolyseTips(inputs),
            Timeline = BuildTimeline(inputs, RestMethod.Autolyse, minutes)
        };
    }

    private BreadRecommendation BuildFermentolyse(BreadInputs inputs, int minutes, string reason)
    {
        return new BreadRecommendation
        {
            Method = RestMethod.Fermentolyse,
            RestDurationMin = minutes,
            Headline = "Use Fermentolyse",
            Reason = reason,
            RiskLevel = GetFermentolyseRisk(inputs.KitchenTemperatureC),
            Tips = FermentolyseTips(inputs),
            Timeline = BuildTimeline(inputs, RestMethod.Fermentolyse, minutes)
        };
    }

    private BreadRecommendation BuildSkip(BreadInputs inputs, string reason)
    {
        return new BreadRecommendation
        {
            Method = RestMethod.Skip,
            RestDurationMin = 0,
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
            Headline = "Use a Soaker",
            Reason = $"{grain.DisplayName} is gluten-free — the autolyse and fermentolyse techniques rely on gluten development, which this grain cannot provide. Instead, use a soaker: rest the flour in its liquid for {grain.SoakerMinutes} minutes to fully hydrate the starch, reduce grittiness, and improve crumb texture.",
            RiskLevel = "Low",
            Tips = SoakerTips(inputs, grain),
            Timeline = BuildTimeline(inputs, RestMethod.Soaker, grain.SoakerMinutes)
        };
    }

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
        if (inputs.FlourType is FlourType.WholeGrain or FlourType.Rye)
            tips.Add("Bran-heavy flours need longer hydration — don't rush the rest.");
        if (grain.IsLowGlutenAncient)
            tips.Add($"Handle {grain.DisplayName} gently — folds only, no aggressive kneading. The gluten is fragile.");
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

    private List<TimelineStep> BuildTimeline(BreadInputs inputs, RestMethod method, int restMinutes)
    {
        bool isSourdough = inputs.HasSourdoughStarter;
        var steps = new List<TimelineStep>();

        if (method == RestMethod.Soaker)
        {
            steps.Add(new TimelineStep { Phase = "Whisk batter", DurationLabel = "5 min", TempLabel = "Ambient", Notes = "Combine flour, liquid, and binder (psyllium/xanthan/egg). No kneading." });
            steps.Add(new TimelineStep
            {
                Phase = "Soaker rest",
                DurationLabel = $"{restMinutes} min",
                TempLabel = "Ambient",
                Notes = "Cover and rest. Starch fully absorbs liquid — reduces grittiness and improves crumb texture.",
                IsRestPhase = true,
                RestMethod = method
            });
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

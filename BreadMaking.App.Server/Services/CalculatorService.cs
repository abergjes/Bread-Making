using BreadMaking.App.Shared.Dtos;

namespace BreadMaking.App.Server.Services;

// Pure-math service — no I/O, no EF, no DI. All methods are static.
// Source: baker's guide §48 (scaling, DDT, hydration, cost) and §54 (water-roux).
public static class CalculatorService
{
    // §48.1 — Baker's-% scaling
    public static ScaleResult Scale(List<IngredientPct> formula, decimal targetDoughGrams)
    {
        var tfp   = formula.Sum(i => i.Percent);
        var flour = targetDoughGrams / (tfp / 100m);
        var rows  = formula
            .Select(i => new ScaleResultRow(i.Name, i.Percent,
                         Math.Round(flour * i.Percent / 100m, 1)))
            .ToList();
        return new ScaleResult(rows, Math.Round(tfp, 1),
            Math.Round(rows.Sum(r => r.Grams), 1));
    }

    // §48.2 — Batch scaling with yield / loss
    public static BatchResult Batch(BatchRequest req)
    {
        var doughPerLoaf = req.BakedWeightG / (1m - req.BakeLossPct / 100m);
        var batchDough   = req.Loaves * doughPerLoaf / (1m - req.ScaleLossPct / 100m);
        return new BatchResult(
            Math.Round(doughPerLoaf, 1),
            Math.Round(batchDough, 1),
            Scale(req.Formula, batchDough));
    }

    // §48.3 — DDT water temperature
    // frictionC presets (baker's guide §50.4):
    //   hand-folds ~2, hand-knead ~3, stand mixer ~10, spiral ~14, intensive ~24
    public static DdtResult Ddt(DdtRequest req)
    {
        var factors = new List<decimal> { req.FlourC, req.RoomC, req.FrictionC };
        if (req.PrefermentC is decimal p) factors.Add(p);
        int     n     = factors.Count + 1;   // +1 for the water term
        decimal water = req.Ddt * n - factors.Sum();
        return new DdtResult(Math.Round(water, 1), n);
    }

    // §48.4 — Levain split & true hydration
    public static HydrationResult Hydration(HydrationRequest req)
    {
        var totalWater = req.TotalFlourG * req.TargetHydrationPct / 100m;
        var lf = req.LevainGrams / (1m + req.LevainHydrationPct / 100m);
        var lw = req.LevainGrams - lf;
        var df = req.TotalFlourG - lf;
        var dw = totalWater - lw;
        // check: (lw + dw) / (lf + df) should equal TargetHydrationPct
        var checkHydration = (lw + dw) / (lf + df) * 100m;
        return new HydrationResult(
            Math.Round(lf, 1), Math.Round(lw, 1),
            Math.Round(df, 1), Math.Round(dw, 1),
            Math.Round(checkHydration, 1));
    }

    // §48.5 — Cost per loaf
    public static CostResult Cost(CostRequest req)
    {
        var ingredientCost = req.Ingredients
            .Sum(i => i.Grams * i.PricePer100g / 100m);
        var batchCost = ingredientCost + req.EnergyCost + req.LabourCost
                      + req.PackagingCost + req.OverheadCost;
        return new CostResult(
            Math.Round(ingredientCost, 4),
            Math.Round(batchCost, 4),
            Math.Round(batchCost / req.SaleableLoaves, 4));
    }

    // §54.3 — Water-roux fold (Tangzhong: ratio 5.0 · Yudane: ratio 1.0)
    public static RouxResult Roux(RouxRequest req)
    {
        var totalLiquid = req.TotalFlour * req.HydrationPct / 100m;
        var rf = Math.Round(req.TotalFlour * req.RouxFlourSharePct / 100m, 1);
        var rl = Math.Round(rf * req.RouxRatio, 1);
        var df = Math.Round(req.TotalFlour - rf, 1);
        var dl = Math.Round(totalLiquid - rl, 1);
        return new RouxResult(
            rf, rl, df, dl,
            Math.Round(rf + df, 1),
            Math.Round(rl + dl, 1),
            Math.Round((rl + dl) / req.TotalFlour * 100m, 1));
    }
}

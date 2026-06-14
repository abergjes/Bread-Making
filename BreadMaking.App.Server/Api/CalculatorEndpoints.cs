using BreadMaking.App.Server.Services;
using BreadMaking.App.Shared.Dtos;

namespace BreadMaking.App.Server.Api;

public static class CalculatorEndpoints
{
    public static IEndpointRouteBuilder MapCalculatorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/calculators");

        // §48.1 — Baker's-% scaling
        group.MapPost("/scale", (ScaleRequest req) =>
        {
            if (req.Formula is not { Count: > 0 })
                return Results.BadRequest(new { error = "Formula must have at least one ingredient." });
            if (req.TargetDoughGrams <= 0)
                return Results.BadRequest(new { error = "Target dough weight must be greater than zero." });
            return Results.Ok(CalculatorService.Scale(req.Formula, req.TargetDoughGrams));
        });

        // §48.2 — Batch scaling with yield / loss
        group.MapPost("/batch", (BatchRequest req) =>
        {
            if (req.Loaves <= 0)
                return Results.BadRequest(new { error = "Loaf count must be greater than zero." });
            if (req.BakedWeightG <= 0)
                return Results.BadRequest(new { error = "Baked weight must be greater than zero." });
            if (req.BakeLossPct is < 0 or > 50)
                return Results.BadRequest(new { error = "Bake loss must be between 0% and 50%." });
            if (req.ScaleLossPct is < 0 or > 20)
                return Results.BadRequest(new { error = "Scale loss must be between 0% and 20%." });
            if (req.Formula is not { Count: > 0 })
                return Results.BadRequest(new { error = "Formula must have at least one ingredient." });
            return Results.Ok(CalculatorService.Batch(req));
        });

        // §48.3 — DDT water temperature
        group.MapPost("/ddt", (DdtRequest req) =>
        {
            if (req.Ddt is < 10 or > 40)
                return Results.BadRequest(new { error = "Target DDT must be between 10 °C and 40 °C." });
            if (req.FrictionC is < 0 or > 35)
                return Results.BadRequest(new { error = "Friction factor must be between 0 °C and 35 °C." });
            return Results.Ok(CalculatorService.Ddt(req));
        });

        // §48.4 — Levain split & true hydration
        group.MapPost("/hydration", (HydrationRequest req) =>
        {
            if (req.TotalFlourG <= 0)
                return Results.BadRequest(new { error = "Total flour must be greater than zero." });
            if (req.TargetHydrationPct is < 40 or > 200)
                return Results.BadRequest(new { error = "Hydration must be between 40% and 200%." });
            if (req.LevainGrams <= 0)
                return Results.BadRequest(new { error = "Levain weight must be greater than zero." });
            if (req.LevainGrams >= req.TotalFlourG)
                return Results.BadRequest(new { error = "Levain weight must be less than total flour." });
            if (req.LevainHydrationPct is < 20 or > 300)
                return Results.BadRequest(new { error = "Levain hydration must be between 20% and 300%." });
            return Results.Ok(CalculatorService.Hydration(req));
        });

        // §48.5 — Cost per loaf
        group.MapPost("/cost", (CostRequest req) =>
        {
            if (req.SaleableLoaves <= 0)
                return Results.BadRequest(new { error = "Saleable loaf count must be greater than zero." });
            return Results.Ok(CalculatorService.Cost(req));
        });

        // §54.3 — Water-roux fold (Tangzhong or Yudane)
        group.MapPost("/roux", (RouxRequest req) =>
        {
            if (req.TotalFlour <= 0)
                return Results.BadRequest(new { error = "Total flour must be greater than zero." });
            if (req.HydrationPct is < 40 or > 200)
                return Results.BadRequest(new { error = "Hydration must be between 40% and 200%." });
            if (req.RouxFlourSharePct is < 1 or > 20)
                return Results.BadRequest(new { error = "Roux flour share must be between 1% and 20%." });
            if (req.RouxRatio != 5m && req.RouxRatio != 1m)
                return Results.BadRequest(new { error = "Roux ratio must be 5 (Tangzhong) or 1 (Yudane)." });
            return Results.Ok(CalculatorService.Roux(req));
        });

        return app;
    }
}

using BreadMaking.App.Server.Data;
using BreadMaking.App.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BreadMaking.App.Server.Api;

public static class GrainEndpoints
{
    public static IEndpointRouteBuilder MapGrainEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/grains/comparison", async (AppDbContext db) =>
        {
            var bakes = await db.Bakes
                .Include(b => b.Recipe).ThenInclude(r => r!.GrainProfile)
                .Include(b => b.Outcome)
                .Include(b => b.StepLogs).ThenInclude(l => l.RecipeStep)
                .Include(b => b.StepLogs).ThenInclude(l => l.Measurements)
                .Where(b => b.Outcome != null)
                .ToListAsync();

            var result = bakes
                .GroupBy(b => b.Recipe?.GrainProfile?.Name ?? "Unknown")
                .OrderBy(g => g.Key)
                .Select(g => new GrainComparisonDto
                {
                    GrainName               = g.Key,
                    BakeCount               = g.Count(),
                    AvgOvenSpringPct        = Avg(g.Select(b => b.Outcome!.OvenSpringPct)),
                    AvgCrumbOpenness        = Avg(g.Select(b =>
                        b.Outcome!.CrumbOpenness.HasValue
                            ? (double?)b.Outcome.CrumbOpenness.Value
                            : null)),
                    AvgTimeTo50PctRiseHours = Avg(g.Select(TimeTo50PctRise)),
                })
                .ToList();

            return Results.Ok(result);
        });

        return app;
    }

    private static double? Avg(IEnumerable<double?> values)
    {
        var list = values.Where(v => v.HasValue).Select(v => v!.Value).ToList();
        return list.Count > 0 ? list.Average() : null;
    }

    private static double? TimeTo50PctRise(Bake bake)
    {
        var bulk = bake.StepLogs
            .FirstOrDefault(l => l.RecipeStep?.Phase == "Bulk" && l.StartedAt.HasValue);
        if (bulk is null) return null;

        var first50 = bulk.Measurements
            .Where(m => m.Value >= 50)
            .OrderBy(m => m.RecordedAt)
            .FirstOrDefault();
        if (first50 is null) return null;

        return (first50.RecordedAt - bulk.StartedAt!.Value).TotalHours;
    }
}

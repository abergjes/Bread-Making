using BreadMaking.App.Server.Data;
using BreadMaking.App.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BreadMaking.App.Server.Api;

public static class AnalyticsEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /api/analytics/correlations?metric=ovenspring|crumb|taste|overall&factor=hydration|salt|kitchentemp|inoculation
        app.MapGet("/api/analytics/correlations", async (string? metric, string? factor, AppDbContext db) =>
        {
            var bakes = await db.Bakes
                .Include(b => b.Recipe).ThenInclude(r => r!.GrainProfile)
                .Include(b => b.Outcome)
                .Where(b => b.Outcome != null)
                .ToListAsync();

            Func<Bake, double?> xSelector = (factor ?? "hydration").ToLowerInvariant() switch
            {
                "salt"        => b => b.SaltPct,
                "kitchentemp" => b => b.AmbientTempC,
                "inoculation" => b => b.InoculationPct,
                _             => b => b.HydrationPct,
            };

            Func<BakeOutcome, double?> ySelector = (metric ?? "ovenspring").ToLowerInvariant() switch
            {
                "crumb"   => o => o.CrumbOpenness.HasValue  ? (double?)o.CrumbOpenness.Value  : null,
                "taste"   => o => o.TasteScore.HasValue     ? (double?)o.TasteScore.Value     : null,
                "overall" => o => o.OverallScore.HasValue   ? (double?)o.OverallScore.Value   : null,
                _         => o => o.OvenSpringPct,
            };

            var points = bakes
                .Select(b => new { b, x = xSelector(b), y = ySelector(b.Outcome!) })
                .Where(r => r.x.HasValue && r.y.HasValue)
                .Select(r => new CorrelationPointDto
                {
                    BakeId    = r.b.Id,
                    GrainName = r.b.Recipe?.GrainProfile?.Name ?? "Unknown",
                    Date      = r.b.StartedAt.ToString("d MMM yyyy"),
                    X         = r.x!.Value,
                    Y         = r.y!.Value,
                })
                .ToList();

            return Results.Ok(points);
        });

        // GET /api/analytics/personal-bests
        app.MapGet("/api/analytics/personal-bests", async (AppDbContext db) =>
        {
            var bakes = await db.Bakes
                .Include(b => b.Recipe).ThenInclude(r => r!.GrainProfile)
                .Include(b => b.Outcome)
                .Where(b => b.Outcome != null)
                .ToListAsync();

            var bests = bakes
                .GroupBy(b => b.Recipe?.GrainProfile?.Name ?? "Unknown")
                .Select(g =>
                {
                    var best = g
                        .OrderByDescending(b => b.Outcome!.OverallScore ?? 0)
                        .ThenByDescending(b => b.Outcome!.OvenSpringPct ?? 0)
                        .First();

                    return new PersonalBestDto
                    {
                        GrainName     = g.Key,
                        BakeId        = best.Id,
                        Date          = best.StartedAt.ToString("d MMM yyyy"),
                        OverallScore  = best.Outcome!.OverallScore,
                        OvenSpringPct = best.Outcome.OvenSpringPct,
                        CrumbOpenness = best.Outcome.CrumbOpenness.HasValue
                            ? (double?)best.Outcome.CrumbOpenness.Value : null,
                        Tags          = best.Outcome.Tags,
                        PhotoPath     = best.Outcome.PhotoPath,
                    };
                })
                .OrderBy(pb => pb.GrainName)
                .ToList();

            return Results.Ok(bests);
        });

        // GET /api/analytics/season-trend
        app.MapGet("/api/analytics/season-trend", async (AppDbContext db) =>
        {
            var bakes = await db.Bakes
                .Include(b => b.Outcome)
                .Where(b => b.Outcome != null)
                .ToListAsync();

            var trend = bakes
                .GroupBy(b => b.StartedAt.ToString("yyyy-MM"))
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var springs = g
                        .Where(b => b.Outcome!.OvenSpringPct.HasValue)
                        .Select(b => b.Outcome!.OvenSpringPct!.Value)
                        .ToList();
                    var crumbs = g
                        .Where(b => b.Outcome!.CrumbOpenness.HasValue)
                        .Select(b => (double)b.Outcome!.CrumbOpenness!.Value)
                        .ToList();

                    return new SeasonTrendDto
                    {
                        Month            = g.Key,
                        AvgOvenSpring    = springs.Count > 0 ? springs.Average() : null,
                        AvgCrumbOpenness = crumbs.Count  > 0 ? crumbs.Average()  : null,
                    };
                })
                .ToList();

            return Results.Ok(trend);
        });

        // GET /api/analytics/bake-diff?a={id1}&b={id2}
        app.MapGet("/api/analytics/bake-diff", async (int a, int b, AppDbContext db) =>
        {
            var bakes = await db.Bakes
                .Include(bk => bk.Recipe).ThenInclude(r => r!.GrainProfile)
                .Include(bk => bk.Outcome)
                .Where(bk => bk.Id == a || bk.Id == b)
                .ToListAsync();

            var bakeA = bakes.FirstOrDefault(bk => bk.Id == a);
            var bakeB = bakes.FirstOrDefault(bk => bk.Id == b);

            if (bakeA is null || bakeB is null)
                return Results.NotFound();

            return Results.Ok(new[] { ToBakeDiffSide(bakeA), ToBakeDiffSide(bakeB) });
        });

        return app;
    }

    private static BakeDiffSideDto ToBakeDiffSide(Bake b) => new()
    {
        BakeId          = b.Id,
        GrainName       = b.Recipe?.GrainProfile?.Name ?? "Unknown",
        Date            = b.StartedAt.ToString("d MMM yyyy"),
        Method          = b.Recipe?.Method.ToString() ?? "",
        HydrationPct    = b.HydrationPct,
        SaltPct         = b.SaltPct,
        InoculationPct  = b.InoculationPct,
        StarterActivity = b.StarterActivity,
        AmbientTempC    = b.AmbientTempC,
        OvenSpringPct   = b.Outcome?.OvenSpringPct,
        CrumbOpenness   = b.Outcome?.CrumbOpenness,
        OverallScore    = b.Outcome?.OverallScore,
        TasteScore      = b.Outcome?.TasteScore,
        CrustScore      = b.Outcome?.CrustScore,
        Tags            = b.Outcome?.Tags,
        CrumbNotes      = b.Outcome?.CrumbNotes,
        ProofingResult  = b.Outcome?.ProofingResult,
    };
}

using BreadMaking.App.Server.Services;
using BreadMaking.App.Shared.Dtos;

namespace BreadMaking.App.Server.Api;

public static class StarterEndpoints
{
    public static IEndpointRouteBuilder MapStarterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/starters");

        group.MapGet("/", async (IStarterService svc) =>
            Results.Ok(await svc.GetAllAsync()));

        group.MapPost("/", async (CreateStarterRequest req, IStarterService svc) =>
        {
            var dto = await svc.CreateAsync(req);
            return Results.Created($"/api/starters/{dto.Id}", dto);
        });

        group.MapGet("/{id:int}", async (int id, IStarterService svc) =>
        {
            var dto = await svc.GetAsync(id);
            return dto is null ? Results.NotFound() : Results.Ok(dto);
        });

        group.MapGet("/{id:int}/feeds", async (int id, IStarterService svc) =>
            Results.Ok(await svc.GetFeedsAsync(id)));

        group.MapPost("/{id:int}/feeds", async (int id, LogFeedRequest req, IStarterService svc) =>
        {
            try
            {
                var dto = await svc.LogFeedAsync(id, req);
                return Results.Created($"/api/starters/{id}/feeds/{dto.Id}", dto);
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        });

        // Returns the N most recent feed entries across all starters (for advisor selector)
        group.MapGet("/recent-feeds", async (IStarterService svc, int count = 5) =>
            Results.Ok(await svc.GetRecentFeedsAsync(count)));

        return app;
    }
}

using System.ComponentModel.DataAnnotations;
using BreadMaking.App.Server.Services;
using BreadMaking.App.Shared.Dtos;

namespace BreadMaking.App.Server.Api;

public static class StepLogEndpoints
{
    public static IEndpointRouteBuilder MapStepLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/steplogs");

        group.MapPost("/{id:int}/start", async (int id, ITimerService svc) =>
        {
            try   { return Results.Ok(await svc.StartAsync(id)); }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        });

        group.MapPost("/{id:int}/pause", async (int id, ITimerService svc) =>
        {
            try   { return Results.Ok(await svc.PauseAsync(id)); }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        });

        group.MapPost("/{id:int}/complete", async (int id, ITimerService svc) =>
        {
            try   { return Results.Ok(await svc.CompleteAsync(id)); }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        });

        // PATCH /api/steplogs/{id}/duration?deltaMin=15
        group.MapPatch("/{id:int}/duration", async (int id, int deltaMin, ITimerService svc) =>
        {
            try   { return Results.Ok(await svc.AdjustPlannedAsync(id, deltaMin)); }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        });

        // PATCH /api/steplogs/{id}/notes
        group.MapPatch("/{id:int}/notes", async (int id, UpdateNotesRequest req, IBakeSessionService svc) =>
            await svc.UpdateStepNotesAsync(id, req.Notes) ? Results.NoContent() : Results.NotFound());

        // POST /api/steplogs/{id}/measurements
        group.MapPost("/{id:int}/measurements",
            async (int id, AddMeasurementRequest req, IMeasurementService svc) =>
            {
                try
                {
                    var dto = await svc.AddAsync(id, req);
                    return Results.Created($"/api/steplogs/{id}/measurements/{dto.Id}", dto);
                }
                catch (KeyNotFoundException) { return Results.NotFound(); }
                catch (ValidationException ex)
                {
                    return Results.UnprocessableEntity(new { error = ex.Message });
                }
            });

        return app;
    }
}

using System.Text;
using System.Text.Json;
using BreadMaking.App.Server.Services;
using BreadMaking.App.Shared.Dtos;

namespace BreadMaking.App.Server.Api;

public static class BakeEndpoints
{
    private static readonly JsonSerializerOptions IndentedJson =
        new() { WriteIndented = true };

    private static readonly string[] MeasurementTypeNames =
        ["Dough temp", "Aliquot rise", "pH", "TTA", "Internal temp"];

    public static IEndpointRouteBuilder MapBakeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bakes");

        group.MapPost("/", async (StartBakeRequest req, IBakeSessionService svc) =>
        {
            var dto = await svc.CreateFromRequestAsync(req);
            return Results.Created($"/api/bakes/{dto.Id}", dto);
        });

        group.MapGet("/", async (IBakeSessionService svc) =>
            Results.Ok(await svc.GetBakeListAsync()));

        group.MapGet("/{id:int}", async (int id, IBakeSessionService svc) =>
        {
            var dto = await svc.GetBakeAsync(id);
            return dto is null ? Results.NotFound() : Results.Ok(dto);
        });

        group.MapGet("/{id:int}/inputs", async (int id, IBakeSessionService svc) =>
        {
            var req = await svc.GetBakeInputsAsync(id);
            return req is null ? Results.NotFound() : Results.Ok(req);
        });

        group.MapPatch("/{id:int}/notes", async (int id, UpdateNotesRequest req, IBakeSessionService svc) =>
        {
            var ok = await svc.UpdateNotesAsync(id, req.Notes);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        group.MapPut("/{id:int}/outcome", async (int id, BakeOutcomeDto dto, IBakeSessionService svc) =>
        {
            var ok = await svc.SaveOutcomeAsync(id, dto);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        group.MapPost("/{id:int}/outcome/photo", async (
            int id, IFormFile photo, IBakeSessionService svc, IWebHostEnvironment env) =>
        {
            var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
                return Results.BadRequest(new { error = "Only JPEG, PNG, and WebP images are accepted." });

            if (photo.Length > 10 * 1024 * 1024)
                return Results.BadRequest(new { error = "Photo must be under 10 MB." });

            var dir = Path.Combine(env.ContentRootPath, "uploads", "bake-photos");
            Directory.CreateDirectory(dir);

            var fileName  = $"bake-{id}{ext}";
            var filePath  = Path.Combine(dir, fileName);
            await using var stream = File.Create(filePath);
            await photo.CopyToAsync(stream);

            var relative = $"bake-photos/{fileName}";
            var ok = await svc.SavePhotoAsync(id, relative);
            return ok
                ? Results.Ok(new { url = $"/uploads/{relative}" })
                : Results.NotFound();
        }).DisableAntiforgery();

        group.MapGet("/{id:int}/export", async (int id, string format, IBakeSessionService svc) =>
        {
            var bake = await svc.GetBakeAsync(id);
            if (bake is null) return Results.NotFound();

            if (format.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                var json = JsonSerializer.Serialize(bake, IndentedJson);
                return Results.File(
                    Encoding.UTF8.GetBytes(json),
                    "application/json",
                    $"bake-{id}.json");
            }

            var csv = BuildCsv(bake);
            return Results.File(
                Encoding.UTF8.GetBytes(csv),
                "text/csv",
                $"bake-{id}.csv");
        });

        return app;
    }

    private static string BuildCsv(BakeDto bake)
    {
        var sb = new StringBuilder();

        // Header
        sb.Append("BakeId,GrainName,Method,BakeStartedAt,");
        sb.Append("StepOrder,StepName,Phase,PlannedMin,ActualMin,Status,");
        sb.AppendLine("DoughTemp_C,AliquotRise_Pct,pH,TTA_mL,InternalTemp_C");

        foreach (var step in bake.StepLogs.OrderBy(s => s.Order))
        {
            int? actualMin = step.StartedAt.HasValue && step.EndedAt.HasValue
                ? (int)(step.EndedAt.Value - step.StartedAt.Value).TotalMinutes
                : null;

            sb.Append(bake.Id); sb.Append(',');
            sb.Append(CsvEscape(bake.GrainName)); sb.Append(',');
            sb.Append(bake.Method); sb.Append(',');
            sb.Append(bake.StartedAt.ToString("O")); sb.Append(',');
            sb.Append(step.Order); sb.Append(',');
            sb.Append(CsvEscape(step.StepName)); sb.Append(',');
            sb.Append(step.Phase); sb.Append(',');
            sb.Append(step.PlannedDurationMin); sb.Append(',');
            sb.Append(actualMin.HasValue ? actualMin.Value.ToString() : ""); sb.Append(',');
            sb.Append(step.Status);

            foreach (var typeName in MeasurementTypeNames)
            {
                sb.Append(',');
                sb.Append(LastMeasurement(step, typeName));
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string LastMeasurement(BakeStepLogDto step, string typeName)
    {
        var m = step.Measurements
            .Where(m => m.TypeName == typeName)
            .OrderBy(m => m.RecordedAt)
            .LastOrDefault();
        return m is null ? "" : m.Value.ToString("G4");
    }

    private static string CsvEscape(string value) =>
        value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}

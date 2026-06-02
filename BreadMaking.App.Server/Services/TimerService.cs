using BreadMaking.App.Server.Data;
using BreadMaking.App.Shared;
using BreadMaking.App.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BreadMaking.App.Server.Services;

public class TimerService(AppDbContext db) : ITimerService
{
    public async Task<BakeStepLogDto> StartAsync(int id)
    {
        var log = await db.BakeStepLogs
            .Include(l => l.RecipeStep)
            .Include(l => l.Measurements).ThenInclude(m => m.MeasurementType)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"BakeStepLog {id} not found.");

        if (log.Status == StepStatus.Paused && log.StartedAt is not null && log.EndedAt is not null)
        {
            // Resume: shift StartedAt forward to preserve accumulated elapsed.
            // frozenElapsed = EndedAt - StartedAt (the time that was on the clock when paused)
            var frozenElapsed = log.EndedAt.Value - log.StartedAt.Value;
            log.StartedAt = DateTimeOffset.UtcNow - frozenElapsed;
            log.EndedAt = null;
        }
        else
        {
            log.StartedAt = DateTimeOffset.UtcNow;
            log.EndedAt = null;
        }

        log.Status = StepStatus.Running;
        await db.SaveChangesAsync();
        return DtoMapper.ToDto(log);
    }

    public async Task<BakeStepLogDto> PauseAsync(int id)
    {
        var log = await Load(id);
        log.EndedAt = DateTimeOffset.UtcNow;
        log.Status = StepStatus.Paused;
        await db.SaveChangesAsync();
        return DtoMapper.ToDto(log);
    }

    public async Task<BakeStepLogDto> CompleteAsync(int id)
    {
        var log = await Load(id);
        log.EndedAt = DateTimeOffset.UtcNow;
        log.Status = StepStatus.Completed;
        await db.SaveChangesAsync();
        return DtoMapper.ToDto(log);
    }

    public async Task<BakeStepLogDto> AdjustPlannedAsync(int id, int deltaMinutes)
    {
        var log = await db.BakeStepLogs
            .Include(l => l.RecipeStep)
            .Include(l => l.Measurements).ThenInclude(m => m.MeasurementType)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"BakeStepLog {id} not found.");

        // ± controls change only the target duration, never the clock.
        log.PlannedDurationMin = Math.Clamp(
            log.PlannedDurationMin + deltaMinutes,
            log.RecipeStep.MinDurationMin,
            log.RecipeStep.MaxDurationMin);

        await db.SaveChangesAsync();
        return DtoMapper.ToDto(log);
    }

    private async Task<BakeStepLog> Load(int id) =>
        await db.BakeStepLogs
            .Include(l => l.RecipeStep)
            .Include(l => l.Measurements).ThenInclude(m => m.MeasurementType)
            .FirstOrDefaultAsync(l => l.Id == id)
        ?? throw new KeyNotFoundException($"BakeStepLog {id} not found.");
}

using BreadMaking.App.Server.Data;
using BreadMaking.App.Shared;
using Microsoft.EntityFrameworkCore;

namespace BreadMaking.App.Server.Services;

public class FoldsReminderService(
    IServiceScopeFactory scopeFactory,
    INotificationService notifications) : BackgroundService
{
    // Tracks (stepLogId, 30-min multiple) already sent to avoid duplicates across polls.
    private readonly HashSet<(int stepLogId, int multiple)> _sent = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAndSendAsync();
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }

    private async Task CheckAndSendAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTimeOffset.UtcNow;

        var bulkSteps = await db.BakeStepLogs
            .Include(l => l.RecipeStep)
            .Where(l => l.Status == StepStatus.Running
                     && l.StartedAt != null
                     && l.RecipeStep!.Phase == "Bulk")
            .ToListAsync();

        foreach (var step in bulkSteps)
        {
            var elapsed  = step.Elapsed(now);
            var multiple = (int)(elapsed.TotalMinutes / 30);
            if (multiple < 1) continue;

            var key = (step.Id, multiple);
            if (_sent.Contains(key)) continue;

            _sent.Add(key);
            await notifications.SendFoldsReminderAsync(step.BakeId);
        }
    }
}

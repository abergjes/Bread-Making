using BreadMaking.App.Server.Data;
using BreadMaking.App.Server.Services;
using BreadMaking.App.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BreadMaking.App.Server.Tests;

/// <summary>
/// Unit tests for TimerService using an in-memory SQLite database.
/// Each test gets a fresh database seeded with one Bake and one BakeStepLog.
/// </summary>
public class TimerServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly int _logId;

    public TimerServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated(); // applies HasData seeds

        // Create one test bake using the first seeded recipe (Modern wheat Autolyse)
        var bake = new Bake { RecipeId = 1, StartedAt = DateTimeOffset.UtcNow };
        _db.Bakes.Add(bake);
        _db.SaveChanges();

        // Create one test step log (step 101 = "Mix flour + water", min=3, max=15, default=5)
        var log = new BakeStepLog
        {
            BakeId             = bake.Id,
            RecipeStepId       = 101,
            PlannedDurationMin = 5,
            Status             = StepStatus.NotStarted,
        };
        _db.BakeStepLogs.Add(log);
        _db.SaveChanges();
        _logId = log.Id;

        _db.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // ── Start from NotStarted ────────────────────────────────────────────────

    [Fact]
    public async Task Start_SetsStatus_Running()
    {
        var svc = new TimerService(_db);
        var dto = await svc.StartAsync(_logId);
        Assert.Equal(StepStatus.Running, dto.Status);
    }

    [Fact]
    public async Task Start_SetsStartedAt_NotNull()
    {
        var svc = new TimerService(_db);
        var dto = await svc.StartAsync(_logId);
        Assert.NotNull(dto.StartedAt);
    }

    [Fact]
    public async Task Start_ClearsEndedAt()
    {
        // Force EndedAt to be set first
        var log = await _db.BakeStepLogs.FindAsync(_logId);
        log!.EndedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        var svc = new TimerService(_db);
        var dto = await svc.StartAsync(_logId);
        Assert.Null(dto.EndedAt);
    }

    // ── Pause ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Pause_SetsStatus_Paused()
    {
        var svc = new TimerService(_db);
        await svc.StartAsync(_logId);
        var dto = await svc.PauseAsync(_logId);
        Assert.Equal(StepStatus.Paused, dto.Status);
    }

    [Fact]
    public async Task Pause_SetsEndedAt_NotNull()
    {
        var svc = new TimerService(_db);
        await svc.StartAsync(_logId);
        var dto = await svc.PauseAsync(_logId);
        Assert.NotNull(dto.EndedAt);
    }

    // ── Resume preserves accumulated elapsed ────────────────────────────────

    [Fact]
    public async Task Resume_PreservesAccumulatedElapsed()
    {
        // Set up a paused state manually:
        // started 5 min ago, paused 2 min ago → frozen elapsed = 3 min
        var now = DateTimeOffset.UtcNow;
        var log = await _db.BakeStepLogs.FindAsync(_logId);
        log!.StartedAt = now.AddMinutes(-5);
        log.EndedAt    = now.AddMinutes(-2);
        log.Status     = StepStatus.Paused;
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        var svc = new TimerService(_db);
        var dto = await svc.StartAsync(_logId);

        Assert.Equal(StepStatus.Running, dto.Status);
        Assert.Null(dto.EndedAt);

        // Elapsed should be ~3 minutes (frozen elapsed preserved on resume)
        var elapsed = (DateTimeOffset.UtcNow - dto.StartedAt!.Value).TotalMinutes;
        Assert.InRange(elapsed, 2.9, 3.1);
    }

    // ── Complete ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Complete_SetsStatus_Completed()
    {
        var svc = new TimerService(_db);
        await svc.StartAsync(_logId);
        var dto = await svc.CompleteAsync(_logId);
        Assert.Equal(StepStatus.Completed, dto.Status);
    }

    [Fact]
    public async Task Complete_SetsEndedAt_NotNull()
    {
        var svc = new TimerService(_db);
        await svc.StartAsync(_logId);
        var dto = await svc.CompleteAsync(_logId);
        Assert.NotNull(dto.EndedAt);
    }

    // ── AdjustPlanned ────────────────────────────────────────────────────────

    [Fact]
    public async Task AdjustPlanned_DoesNotChangeStartedAt_WhenRunning()
    {
        var svc = new TimerService(_db);
        await svc.StartAsync(_logId);

        var before = (await _db.BakeStepLogs.FindAsync(_logId))!.StartedAt;
        _db.ChangeTracker.Clear();

        await svc.AdjustPlannedAsync(_logId, deltaMinutes: 5);

        var after = (await _db.BakeStepLogs.FindAsync(_logId))!.StartedAt;
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task AdjustPlanned_DoesNotChangeEndedAt_WhenPaused()
    {
        var svc = new TimerService(_db);
        await svc.StartAsync(_logId);
        await svc.PauseAsync(_logId);

        var before = (await _db.BakeStepLogs.FindAsync(_logId))!.EndedAt;
        _db.ChangeTracker.Clear();

        await svc.AdjustPlannedAsync(_logId, deltaMinutes: 5);

        var after = (await _db.BakeStepLogs.FindAsync(_logId))!.EndedAt;
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task AdjustPlanned_ClampsToMaxDuration()
    {
        // Step 101 has MaxDurationMin=15, default=5
        var svc = new TimerService(_db);
        var dto = await svc.AdjustPlannedAsync(_logId, deltaMinutes: 999);
        Assert.Equal(15, dto.PlannedDurationMin); // clamped to max
    }

    [Fact]
    public async Task AdjustPlanned_ClampsToMinDuration()
    {
        // Step 101 has MinDurationMin=3, default=5
        var svc = new TimerService(_db);
        var dto = await svc.AdjustPlannedAsync(_logId, deltaMinutes: -999);
        Assert.Equal(3, dto.PlannedDurationMin); // clamped to min
    }

    [Fact]
    public async Task AdjustPlanned_IncrementsCorrectly()
    {
        var svc = new TimerService(_db);
        var dto = await svc.AdjustPlannedAsync(_logId, deltaMinutes: 5);
        Assert.Equal(10, dto.PlannedDurationMin); // 5 (default) + 5 = 10
    }

    // ── Not-found guard ──────────────────────────────────────────────────────

    [Fact]
    public async Task Start_ThrowsKeyNotFound_ForMissingId()
    {
        var svc = new TimerService(_db);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.StartAsync(99999));
    }
}

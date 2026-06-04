using BreadMaking.App.Server.Data;
using BreadMaking.App.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BreadMaking.App.Server.Services;

public class StarterService(AppDbContext db) : IStarterService
{
    public async Task<List<StarterDto>> GetAllAsync()
    {
        var starters = await db.Starters
            .Include(s => s.Feeds)
            .OrderByDescending(s => s.Id)
            .ToListAsync();
        return starters.Select(ToDto).ToList();
    }

    public async Task<StarterDto?> GetAsync(int id)
    {
        var starter = await db.Starters
            .Include(s => s.Feeds)
            .FirstOrDefaultAsync(s => s.Id == id);
        return starter is null ? null : ToDto(starter);
    }

    public async Task<StarterDto> CreateAsync(CreateStarterRequest req)
    {
        var starter = new Starter
        {
            Name         = req.Name.Trim(),
            HydrationPct = req.HydrationPct,
            FlourBlend   = req.FlourBlend?.Trim(),
            Notes        = req.Notes?.Trim(),
            CreatedAt    = DateTimeOffset.UtcNow,
        };
        db.Starters.Add(starter);
        await db.SaveChangesAsync();
        return ToDto(starter);
    }

    public async Task<StarterFeedLogDto> LogFeedAsync(int starterId, LogFeedRequest req)
    {
        var starter = await db.Starters.FindAsync(starterId)
            ?? throw new KeyNotFoundException($"Starter {starterId} not found.");

        var feed = new StarterFeedLog
        {
            StarterId        = starterId,
            FedAt            = DateTimeOffset.UtcNow,
            FlourGrams       = req.FlourGrams,
            WaterGrams       = req.WaterGrams,
            PrevStarterGrams = req.PrevStarterGrams,
            AmbientTempC     = req.AmbientTempC,
            PeakHours        = req.PeakHours,
            FloatTestPassed  = req.FloatTestPassed,
            FeedRatio        = req.FeedRatio?.Trim(),
        };
        db.StarterFeedLogs.Add(feed);
        await db.SaveChangesAsync();
        return ToDto(feed, starter.Name);
    }

    public async Task<List<StarterFeedLogDto>> GetFeedsAsync(int starterId)
    {
        var starter = await db.Starters.FindAsync(starterId);
        if (starter is null) return [];

        var feeds = await db.StarterFeedLogs
            .Where(f => f.StarterId == starterId)
            .OrderByDescending(f => f.Id)
            .ToListAsync();
        return feeds.Select(f => ToDto(f, starter.Name)).ToList();
    }

    public async Task<List<StarterFeedLogDto>> GetRecentFeedsAsync(int count = 5)
    {
        var feeds = await db.StarterFeedLogs
            .Include(f => f.Starter)
            .OrderByDescending(f => f.Id)
            .Take(count)
            .ToListAsync();
        return feeds.Select(f => ToDto(f, f.Starter.Name)).ToList();
    }

    private static StarterDto ToDto(Starter s) => new()
    {
        Id           = s.Id,
        Name         = s.Name,
        HydrationPct = s.HydrationPct,
        FlourBlend   = s.FlourBlend,
        CreatedAt    = s.CreatedAt,
        Notes        = s.Notes,
        Feeds        = s.Feeds.OrderByDescending(f => f.Id).Select(f => ToDto(f, s.Name)).ToList(),
    };

    private static StarterFeedLogDto ToDto(StarterFeedLog f, string starterName) => new()
    {
        Id               = f.Id,
        StarterId        = f.StarterId,
        StarterName      = starterName,
        FedAt            = f.FedAt,
        FlourGrams       = f.FlourGrams,
        WaterGrams       = f.WaterGrams,
        PrevStarterGrams = f.PrevStarterGrams,
        AmbientTempC     = f.AmbientTempC,
        PeakHours        = f.PeakHours,
        FloatTestPassed  = f.FloatTestPassed,
        FeedRatio        = f.FeedRatio,
    };
}

using BreadMaking.App.Server.Data;
using BreadMaking.App.Shared;
using BreadMaking.App.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BreadMaking.App.Server.Services;

public class BakeSessionService(AppDbContext db) : IBakeSessionService
{
    public async Task<BakeDto> CreateFromRequestAsync(StartBakeRequest request)
    {
        if (!Enum.TryParse<BakeMethod>(request.Method, ignoreCase: true, out var method))
            method = BakeMethod.Autolyse;

        var recipe = await FindRecipeAsync(request.GrainName, method);

        var bake = new Bake
        {
            RecipeId           = recipe.Id,
            StartedAt          = DateTimeOffset.UtcNow,
            AmbientTempC       = request.AmbientTempC,
            AmbientHumidityPct = request.AmbientHumidityPct,
            FlourBatch         = request.FlourBatch,
            Notes              = request.Notes,
            HydrationPct       = request.HydrationPct,
            StarterActivity    = request.StarterActivity,
            TotalFlourGrams    = request.TotalFlourGrams,
            SaltPct            = request.SaltPct,
            InoculationPct     = request.InoculationPct,
            StarterFeedLogId   = request.StarterFeedLogId,
            ButterPct          = request.ButterPct,
            EggPct             = request.EggPct,
            SugarPct           = request.SugarPct,
            MilkPct            = request.MilkPct,
            MilkPowderPct      = request.MilkPowderPct,
            IsPullmanTin       = request.IsPullmanTin,
            StepLogs           = recipe.Steps
                                       .OrderBy(s => s.Order)
                                       .Select(step => new BakeStepLog
                                       {
                                           RecipeStepId       = step.Id,
                                           PlannedDurationMin = step.DefaultDurationMin,
                                           Status             = StepStatus.NotStarted,
                                       })
                                       .ToList(),
        };

        db.Bakes.Add(bake);
        await db.SaveChangesAsync();

        // Re-load with all navigation properties for the DTO
        return await LoadBakeDtoAsync(bake.Id)
            ?? throw new InvalidOperationException("Bake was not saved.");
    }

    public async Task<BakeDto?> GetBakeAsync(int id) => await LoadBakeDtoAsync(id);

    public async Task<List<BakeListItemDto>> GetBakeListAsync()
    {
        var bakes = await db.Bakes
            .Include(b => b.Recipe).ThenInclude(r => r!.GrainProfile)
            .Include(b => b.Outcome)
            .Include(b => b.StepLogs)
            .Include(b => b.StarterFeed).ThenInclude(f => f!.Starter)
            .OrderByDescending(b => b.Id)
            .ToListAsync();

        return bakes.Select(b => new BakeListItemDto
        {
            Id            = b.Id,
            GrainName     = b.Recipe?.GrainProfile?.Name ?? "",
            Method        = b.Recipe?.Method ?? default,
            StartedAt     = b.StartedAt,
            EndedAt       = b.EndedAt,
            FlourBatch    = b.FlourBatch,
            HasOutcome    = b.Outcome is not null,
            OvenSpringPct = b.Outcome?.OvenSpringPct,
            CrumbOpenness = b.Outcome?.CrumbOpenness,
            HydrationPct  = b.HydrationPct,
            StarterName           = b.StarterFeed?.Starter?.Name,
            StarterFedHoursBefore = b.StarterFeedLogId.HasValue && b.StarterFeed is not null
                ? (b.StartedAt - b.StarterFeed.FedAt).TotalHours
                : null,
            OverallScore  = b.Outcome?.OverallScore,
            Tags          = b.Outcome?.Tags,
            IsBestLoaf    = b.Outcome?.IsBestLoaf ?? false,
            CrumbNotes    = b.Outcome?.CrumbNotes is { Length: > 80 } s
                                ? s[..80] + "…"
                                : b.Outcome?.CrumbNotes,
        }).ToList();
    }

    public async Task<StartBakeRequest?> GetBakeInputsAsync(int id)
    {
        var bake = await db.Bakes
            .Include(b => b.Recipe).ThenInclude(r => r!.GrainProfile)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (bake is null) return null;

        return new StartBakeRequest
        {
            GrainName          = bake.Recipe?.GrainProfile?.Name ?? "Modern wheat",
            Method             = bake.Recipe?.Method.ToString() ?? "Autolyse",
            AmbientTempC       = bake.AmbientTempC,
            AmbientHumidityPct = bake.AmbientHumidityPct,
            FlourBatch         = bake.FlourBatch,
            Notes              = bake.Notes,
            HydrationPct       = bake.HydrationPct,
            StarterActivity    = bake.StarterActivity,
            TotalFlourGrams    = bake.TotalFlourGrams,
            SaltPct            = bake.SaltPct,
            InoculationPct     = bake.InoculationPct,
            StarterFeedLogId   = bake.StarterFeedLogId,
            ButterPct          = bake.ButterPct,
            EggPct             = bake.EggPct,
            SugarPct           = bake.SugarPct,
            MilkPct            = bake.MilkPct,
            MilkPowderPct      = bake.MilkPowderPct,
            IsPullmanTin       = bake.IsPullmanTin,
        };
    }

    public async Task<bool> UpdateNotesAsync(int id, string? notes)
    {
        var bake = await db.Bakes.FindAsync(id);
        if (bake is null) return false;
        bake.Notes = notes;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStepNotesAsync(int stepLogId, string? notes)
    {
        var log = await db.BakeStepLogs.FindAsync(stepLogId);
        if (log is null) return false;
        log.Notes = notes;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SaveOutcomeAsync(int bakeId, BakeOutcomeDto dto)
    {
        var bake = await db.Bakes
            .Include(b => b.Outcome)
            .FirstOrDefaultAsync(b => b.Id == bakeId);
        if (bake is null) return false;

        if (bake.Outcome is null)
        {
            bake.Outcome = new BakeOutcome { BakeId = bakeId };
            db.BakeOutcomes.Add(bake.Outcome);
        }

        bake.Outcome.LoafHeightCm  = dto.LoafHeightCm;
        bake.Outcome.OvenSpringPct = dto.OvenSpringPct;
        bake.Outcome.InternalTempC = dto.InternalTempC;
        bake.Outcome.WeightLossPct = dto.WeightLossPct;
        bake.Outcome.CrumbOpenness = dto.CrumbOpenness;
        bake.Outcome.CrustScore    = dto.CrustScore;
        bake.Outcome.TasteScore    = dto.TasteScore;
        bake.Outcome.OverallScore  = dto.OverallScore;
        bake.Outcome.Tags          = dto.Tags;
        bake.Outcome.IsBestLoaf    = dto.IsBestLoaf;
        bake.Outcome.CrumbNotes    = dto.CrumbNotes;
        bake.Outcome.ProofingResult = dto.ProofingResult;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SavePhotoAsync(int bakeId, string relativePath)
    {
        var outcome = await db.BakeOutcomes.FirstOrDefaultAsync(o => o.BakeId == bakeId);
        if (outcome is null) return false;
        outcome.PhotoPath = relativePath;
        await db.SaveChangesAsync();
        return true;
    }

    private async Task<Recipe> FindRecipeAsync(string grainName, BakeMethod method)
    {
        // Exact match: grain name + method
        var recipe = await db.Recipes
            .Include(r => r.GrainProfile)
            .Include(r => r.Steps.OrderBy(s => s.Order))
            .FirstOrDefaultAsync(r =>
                r.GrainProfile != null &&
                r.GrainProfile.Name == grainName &&
                r.Method == method);

        // Fallback: modern wheat (GrainProfileId=1) + same method
        recipe ??= await db.Recipes
            .Include(r => r.GrainProfile)
            .Include(r => r.Steps.OrderBy(s => s.Order))
            .FirstOrDefaultAsync(r => r.GrainProfileId == 1 && r.Method == method);

        // Ultimate fallback: modern wheat autolyse
        recipe ??= await db.Recipes
            .Include(r => r.GrainProfile)
            .Include(r => r.Steps.OrderBy(s => s.Order))
            .FirstAsync(r => r.GrainProfileId == 1 && r.Method == BakeMethod.Autolyse);

        return recipe;
    }

    private async Task<BakeDto?> LoadBakeDtoAsync(int id)
    {
        var bake = await db.Bakes
            .Include(b => b.Recipe)
                .ThenInclude(r => r!.GrainProfile)
            .Include(b => b.StepLogs)
                .ThenInclude(l => l.RecipeStep)
            .Include(b => b.StepLogs)
                .ThenInclude(l => l.Measurements)
                    .ThenInclude(m => m.MeasurementType)
            .Include(b => b.Outcome)
            .Include(b => b.StarterFeed).ThenInclude(f => f!.Starter)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bake is null) return null;

        var types = await db.MeasurementTypes.ToListAsync();
        return DtoMapper.ToDto(bake, types);
    }
}

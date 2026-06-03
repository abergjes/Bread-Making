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
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bake is null) return null;

        var types = await db.MeasurementTypes.ToListAsync();
        return DtoMapper.ToDto(bake, types);
    }
}

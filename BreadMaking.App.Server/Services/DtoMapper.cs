using BreadMaking.App.Server.Data;
using BreadMaking.App.Shared.Dtos;

namespace BreadMaking.App.Server.Services;

internal static class DtoMapper
{
    internal static BakeDto ToDto(Bake bake, IEnumerable<MeasurementType> measurementTypes) => new()
    {
        Id             = bake.Id,
        RecipeId       = bake.RecipeId,
        RecipeName     = bake.Recipe?.Name ?? "",
        GrainName      = bake.Recipe?.GrainProfile?.Name ?? "",
        Method         = bake.Recipe?.Method ?? default,
        StartedAt      = bake.StartedAt,
        EndedAt        = bake.EndedAt,
        AmbientTempC   = bake.AmbientTempC,
        AmbientHumidityPct = bake.AmbientHumidityPct,
        FlourBatch     = bake.FlourBatch,
        Notes          = bake.Notes,
        StarterFeedLogId      = bake.StarterFeedLogId,
        StarterName           = bake.StarterFeed?.Starter?.Name,
        StarterFedHoursBefore = bake.StarterFeedLogId.HasValue && bake.StarterFeed is not null
            ? (bake.StartedAt - bake.StarterFeed.FedAt).TotalHours
            : null,
        HydrationPct    = bake.HydrationPct,
        StarterActivity = bake.StarterActivity,
        TotalFlourGrams = bake.TotalFlourGrams,
        SaltPct         = bake.SaltPct,
        InoculationPct  = bake.InoculationPct,
        ButterPct       = bake.ButterPct,
        EggPct          = bake.EggPct,
        SugarPct        = bake.SugarPct,
        MilkPct         = bake.MilkPct,
        MilkPowderPct   = bake.MilkPowderPct,
        IsPullmanTin    = bake.IsPullmanTin,
        StepLogs       = bake.StepLogs
                             .OrderBy(l => l.RecipeStep?.Order ?? 0)
                             .Select(ToDto)
                             .ToList(),
        MeasurementTypes = measurementTypes
                             .Select(t => new MeasurementTypeDto
                             {
                                 Id             = t.Id,
                                 Name           = t.Name,
                                 Unit           = t.Unit,
                                 Category       = t.Category,
                                 MinValid        = t.MinValid,
                                 MaxValid        = t.MaxValid,
                                 DefaultForPhase = t.DefaultForPhase,
                             })
                             .ToList(),
        Outcome = bake.Outcome is null ? null : new BakeOutcomeDto
        {
            LoafHeightCm  = bake.Outcome.LoafHeightCm,
            OvenSpringPct = bake.Outcome.OvenSpringPct,
            InternalTempC = bake.Outcome.InternalTempC,
            WeightLossPct = bake.Outcome.WeightLossPct,
            CrumbOpenness = bake.Outcome.CrumbOpenness,
            CrustScore    = bake.Outcome.CrustScore,
            TasteScore    = bake.Outcome.TasteScore,
            PhotoPath     = bake.Outcome.PhotoPath,
            OverallScore  = bake.Outcome.OverallScore,
            Tags          = bake.Outcome.Tags,
            IsBestLoaf    = bake.Outcome.IsBestLoaf,
        },
    };

    internal static BakeStepLogDto ToDto(BakeStepLog log) => new()
    {
        Id                 = log.Id,
        BakeId             = log.BakeId,
        Order              = log.RecipeStep?.Order ?? 0,
        StepName           = log.RecipeStep?.Name ?? "",
        Phase              = log.RecipeStep?.Phase ?? "",
        Description        = log.RecipeStep?.Description,
        PlannedDurationMin = log.PlannedDurationMin,
        DefaultDurationMin = log.RecipeStep?.DefaultDurationMin ?? log.PlannedDurationMin,
        MinDurationMin     = log.RecipeStep?.MinDurationMin ?? 0,
        MaxDurationMin     = log.RecipeStep?.MaxDurationMin ?? 1440,
        StepMin            = log.RecipeStep?.StepMin ?? 5,
        TargetTempC        = log.RecipeStep?.TargetTempC,
        StartedAt          = log.StartedAt,
        EndedAt            = log.EndedAt,
        Status             = log.Status,
        Notes              = log.Notes,
        Measurements       = log.Measurements
                               .OrderBy(m => m.RecordedAt)
                               .Select(m => new MeasurementDto
                               {
                                   Id                = m.Id,
                                   MeasurementTypeId = m.MeasurementTypeId,
                                   TypeName          = m.MeasurementType?.Name ?? "",
                                   Value             = m.Value,
                                   Unit              = m.Unit,
                                   RecordedAt        = m.RecordedAt,
                               })
                               .ToList(),
    };
}

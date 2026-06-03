namespace BreadMaking.App.Shared.Dtos;

public class BakeDto
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public string RecipeName { get; set; } = "";
    public string GrainName { get; set; } = "";
    public BakeMethod Method { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public double? AmbientTempC { get; set; }
    public double? AmbientHumidityPct { get; set; }
    public string? FlourBatch { get; set; }
    public string? Notes { get; set; }

    // Formula fields (M10)
    public double? HydrationPct    { get; set; }
    public int?    StarterActivity { get; set; }
    public double? TotalFlourGrams { get; set; }
    public double? SaltPct         { get; set; }
    public double? InoculationPct  { get; set; }

    public List<BakeStepLogDto> StepLogs { get; set; } = [];
    public List<MeasurementTypeDto> MeasurementTypes { get; set; } = [];
    public BakeOutcomeDto? Outcome { get; set; }
}

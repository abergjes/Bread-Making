namespace BreadMaking.App.Server.Data;

public class Bake
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public double? AmbientTempC { get; set; }
    public double? AmbientHumidityPct { get; set; }
    public string? FlourBatch { get; set; }
    public string? Notes { get; set; }

    // Starter journal link (M13)
    public int?            StarterFeedLogId { get; set; }
    public StarterFeedLog? StarterFeed      { get; set; }

    // Formula fields — persisted from advisor inputs (M10)
    public double? HydrationPct    { get; set; }
    public int?    StarterActivity { get; set; }   // int mirror of BreadInputs.StarterActivity enum
    public double? TotalFlourGrams { get; set; }
    public double? SaltPct         { get; set; }
    public double? InoculationPct  { get; set; }

    public ICollection<BakeStepLog> StepLogs { get; set; } = new List<BakeStepLog>();
    public BakeOutcome? Outcome { get; set; }
}

namespace BreadMaking.App.Shared.Dtos;

public class BakeListItemDto
{
    public int Id { get; set; }
    public string GrainName { get; set; } = "";
    public BakeMethod Method { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public string? FlourBatch { get; set; }
    public bool HasOutcome { get; set; }
    public double? OvenSpringPct { get; set; }
    public int? CrumbOpenness { get; set; }
    public double? HydrationPct { get; set; }

    // Starter journal link (M13)
    public string? StarterName           { get; set; }
    public double? StarterFedHoursBefore { get; set; }

    // Rating fields (M12)
    public int?    OverallScore { get; set; }
    public string? Tags         { get; set; }
    public bool    IsBestLoaf   { get; set; }
}

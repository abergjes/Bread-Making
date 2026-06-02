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
}

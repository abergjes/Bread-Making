namespace BreadMaking.App.Server.Data;

public class BakeOutcome
{
    public int Id { get; set; }
    public int BakeId { get; set; }
    public Bake Bake { get; set; } = null!;
    public double? LoafHeightCm { get; set; }
    public double? OvenSpringPct { get; set; }
    public double? InternalTempC { get; set; }
    public double? WeightLossPct { get; set; }
    public int? CrumbOpenness { get; set; }   // 0–10
    public int? CrustScore { get; set; }
    public int? TasteScore { get; set; }
    public string? PhotoPath { get; set; }
}

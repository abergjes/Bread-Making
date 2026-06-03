namespace BreadMaking.App.Shared.Dtos;

public class BakeOutcomeDto
{
    public double? LoafHeightCm   { get; set; }
    public double? OvenSpringPct  { get; set; }
    public double? InternalTempC  { get; set; }
    public double? WeightLossPct  { get; set; }
    public int?    CrumbOpenness  { get; set; }
    public int?    CrustScore     { get; set; }
    public int?    TasteScore     { get; set; }
}

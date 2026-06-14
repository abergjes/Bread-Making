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
    public string? PhotoPath      { get; set; }

    // Rating fields (M12)
    public int?    OverallScore  { get; set; }
    public string? Tags          { get; set; }
    public bool    IsBestLoaf    { get; set; }

    // M18 — crumb reading
    public string? CrumbNotes     { get; set; }
    public int?    ProofingResult { get; set; }   // 1=UnderProofed 2=ProperlyProofed 3=OverProofed
}

namespace BreadMaking.App.Shared.Dtos;

public class BakeDiffSideDto
{
    public int     BakeId          { get; set; }
    public string  GrainName       { get; set; } = "";
    public string  Date            { get; set; } = "";
    public string  Method          { get; set; } = "";
    public double? HydrationPct    { get; set; }
    public double? SaltPct         { get; set; }
    public double? InoculationPct  { get; set; }
    public int?    StarterActivity { get; set; }
    public double? AmbientTempC    { get; set; }
    public double? OvenSpringPct   { get; set; }
    public int?    CrumbOpenness   { get; set; }
    public int?    OverallScore    { get; set; }
    public int?    TasteScore      { get; set; }
    public int?    CrustScore      { get; set; }
    public string? Tags            { get; set; }
    public string? CrumbNotes      { get; set; }
    public int?    ProofingResult  { get; set; }
}

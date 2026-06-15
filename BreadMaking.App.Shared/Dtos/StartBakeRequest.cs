namespace BreadMaking.App.Shared.Dtos;

public class StartBakeRequest
{
    public string GrainName { get; set; } = "Modern wheat";
    public string Method { get; set; } = "Autolyse";
    public double? AmbientTempC { get; set; }
    public double? AmbientHumidityPct { get; set; }
    public string? FlourBatch { get; set; }
    public string? Notes { get; set; }

    // Starter journal link (M13)
    public int? StarterFeedLogId { get; set; }

    // Formula fields (M10)
    public double? HydrationPct    { get; set; }
    public int?    StarterActivity { get; set; }
    public double? TotalFlourGrams { get; set; }
    public double? SaltPct         { get; set; }
    public double? InoculationPct  { get; set; }

    // Enriched formula fields (M23)
    public double? ButterPct     { get; set; }
    public double? EggPct        { get; set; }
    public double? SugarPct      { get; set; }
    public double? MilkPct       { get; set; }
    public double? MilkPowderPct { get; set; }
    public bool    IsPullmanTin  { get; set; }
}

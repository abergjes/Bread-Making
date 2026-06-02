namespace BreadMaking.App.Shared.Dtos;

public class StartBakeRequest
{
    public string GrainName { get; set; } = "Modern wheat";
    public string Method { get; set; } = "Autolyse";
    public double? AmbientTempC { get; set; }
    public double? AmbientHumidityPct { get; set; }
    public string? FlourBatch { get; set; }
    public string? Notes { get; set; }
}

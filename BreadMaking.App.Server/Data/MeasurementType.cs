namespace BreadMaking.App.Server.Data;

public class MeasurementType
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "";
    public string Category { get; set; } = "";
    public double? MinValid { get; set; }
    public double? MaxValid { get; set; }
    public string? DefaultForPhase { get; set; }

    public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
}

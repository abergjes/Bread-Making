namespace BreadMaking.App.Shared.Dtos;

public class MeasurementTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "";
    public string Category { get; set; } = "";
    public double? MinValid { get; set; }
    public double? MaxValid { get; set; }
    public string? DefaultForPhase { get; set; }
}

namespace BreadMaking.App.Shared.Dtos;

public class MeasurementDto
{
    public int Id { get; set; }
    public int MeasurementTypeId { get; set; }
    public string TypeName { get; set; } = "";
    public double Value { get; set; }
    public string Unit { get; set; } = "";
    public DateTimeOffset RecordedAt { get; set; }
}

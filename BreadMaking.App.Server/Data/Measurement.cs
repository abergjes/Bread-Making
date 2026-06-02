namespace BreadMaking.App.Server.Data;

public class Measurement
{
    public int Id { get; set; }
    public int BakeStepLogId { get; set; }
    public BakeStepLog BakeStepLog { get; set; } = null!;
    public int MeasurementTypeId { get; set; }
    public MeasurementType MeasurementType { get; set; } = null!;
    public double Value { get; set; }
    public string Unit { get; set; } = "";
    public DateTimeOffset RecordedAt { get; set; }
}

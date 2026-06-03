namespace BreadMaking.App.Shared.Dtos;

public class BakeStepLogDto
{
    public int Id { get; set; }
    public int BakeId { get; set; }
    public int Order { get; set; }
    public string StepName { get; set; } = "";
    public string Phase { get; set; } = "";
    public string? Description { get; set; }
    public int PlannedDurationMin { get; set; }
    public int DefaultDurationMin { get; set; }
    public int MinDurationMin { get; set; }
    public int MaxDurationMin { get; set; }
    public int StepMin { get; set; }
    public double? TargetTempC { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public StepStatus Status { get; set; }
    public string? Notes { get; set; }
    public List<MeasurementDto> Measurements { get; set; } = [];
}

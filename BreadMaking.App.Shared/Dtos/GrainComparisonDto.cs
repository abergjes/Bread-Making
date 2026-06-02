namespace BreadMaking.App.Shared.Dtos;

public class GrainComparisonDto
{
    public string GrainName { get; set; } = "";
    public int BakeCount { get; set; }
    public double? AvgOvenSpringPct { get; set; }
    public double? AvgCrumbOpenness { get; set; }
    public double? AvgTimeTo50PctRiseHours { get; set; }
}

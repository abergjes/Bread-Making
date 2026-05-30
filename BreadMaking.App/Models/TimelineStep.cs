namespace BreadMaking.App.Models;

public class TimelineStep
{
    public string Phase { get; set; } = "";
    public string DurationLabel { get; set; } = "";
    public string TempLabel { get; set; } = "";
    public string Notes { get; set; } = "";
    public bool IsRestPhase { get; set; }
    public RestMethod? RestMethod { get; set; }
}

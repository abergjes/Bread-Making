namespace BreadMaking.App.Models;

public class BreadRecommendation
{
    public RestMethod Method { get; set; }
    public int RestDurationMin { get; set; }
    public string Headline { get; set; } = "";
    public string Reason { get; set; } = "";
    public string RiskLevel { get; set; } = "";
    public List<string> Tips { get; set; } = new();
    public List<TimelineStep> Timeline { get; set; } = new();
}

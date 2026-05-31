namespace BreadMaking.App.Models;

public class BreadRecommendation
{
    public RestMethod Method { get; set; }
    public int RestDurationMin { get; set; }
    public int RestDurationSweetSpot { get; set; }
    public int RestDurationMax { get; set; }
    public string Headline { get; set; } = "";
    public string Reason { get; set; } = "";
    public string RiskLevel { get; set; } = "";
    public List<string> Pros { get; set; } = new();
    public List<string> Cons { get; set; } = new();
    public string BlendNote { get; set; } = "";
    public List<string> Tips { get; set; } = new();
    public List<TimelineStep> Timeline { get; set; } = new();
}

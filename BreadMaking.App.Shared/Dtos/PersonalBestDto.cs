namespace BreadMaking.App.Shared.Dtos;

public class PersonalBestDto
{
    public string  GrainName     { get; set; } = "";
    public int     BakeId        { get; set; }
    public string  Date          { get; set; } = "";
    public int?    OverallScore  { get; set; }
    public double? OvenSpringPct { get; set; }
    public double? CrumbOpenness { get; set; }
    public string? Tags          { get; set; }
    public string? PhotoPath     { get; set; }
}

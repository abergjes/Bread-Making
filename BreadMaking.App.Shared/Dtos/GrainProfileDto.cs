namespace BreadMaking.App.Shared.Dtos;

public class GrainProfileDto
{
    public int     Id                  { get; set; }
    public string  Name                { get; set; } = "";
    public string? Ploidy              { get; set; }
    public string  GlutenStrength      { get; set; } = "";
    public bool    NeedsBinder         { get; set; }
    public string? FlavorNotes         { get; set; }
    public string? NutritionHighlights { get; set; }
    public string? UsageNotes          { get; set; }
    public string? HistoricalOrigin    { get; set; }
}

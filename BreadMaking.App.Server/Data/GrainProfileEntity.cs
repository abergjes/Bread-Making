namespace BreadMaking.App.Server.Data;

public class GrainProfileEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Ploidy { get; set; }
    public string GlutenStrength { get; set; } = "";
    public double HydrationAdjustPct { get; set; }
    public int MaxAutolyseMinutes { get; set; }
    public bool NeedsBinder { get; set; }

    // M17 — grain encyclopedia
    public string? FlavorNotes         { get; set; }
    public string? NutritionHighlights { get; set; }
    public string? UsageNotes          { get; set; }
    public string? HistoricalOrigin    { get; set; }

    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}

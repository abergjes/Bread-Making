namespace BreadMaking.App.Server.Data;

public class RecipeFormula
{
    public int     Id           { get; set; }
    public int     RecipeId     { get; set; }
    public Recipe  Recipe       { get; set; } = null!;
    public double  FlourWeightG { get; set; }
    public double  WaterPct     { get; set; }
    public double  SaltPct      { get; set; }
    public double  StarterPct   { get; set; }
    public string? Notes        { get; set; }
}

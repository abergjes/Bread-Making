using BreadMaking.App.Shared;

namespace BreadMaking.App.Server.Data;

public class Recipe
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public BakeMethod Method { get; set; }
    public int? GrainProfileId { get; set; }
    public GrainProfileEntity? GrainProfile { get; set; }
    public double TargetHydrationPct { get; set; }
    public double TargetDoughTempC { get; set; }
    public double FrictionFactorC { get; set; }

    // M14 — user-defined recipes
    public bool    IsUserDefined  { get; set; }
    public string? CreatedByLabel { get; set; }

    public ICollection<RecipeStep>  Steps   { get; set; } = new List<RecipeStep>();
    public ICollection<Bake>        Bakes   { get; set; } = new List<Bake>();
    public RecipeFormula?           Formula { get; set; }
}

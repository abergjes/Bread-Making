namespace BreadMaking.App.Models;

public class BreadInputs
{
    public double KitchenTemperatureC { get; set; } = 22;
    public FlourType FlourType { get; set; } = FlourType.White;
    public int HydrationPercent { get; set; } = 72;
    public StarterActivity StarterActivity { get; set; } = StarterActivity.NotAvailable;
    public FlavourGoal FlavourGoal { get; set; } = FlavourGoal.MildOpenCrumb;
    public BakerExperience Experience { get; set; } = BakerExperience.Experienced;

    // Grain blend support
    public FlourType? SecondaryFlourType { get; set; }
    public int PrimaryFlourPercent { get; set; } = 100;
    public int SecondaryFlourPercent => 100 - PrimaryFlourPercent;
    public bool IsBlend => SecondaryFlourType.HasValue;

    // The flour with the higher share; used for flour-specific decision branches
    public FlourType DominantFlourType =>
        IsBlend && SecondaryFlourPercent > PrimaryFlourPercent
            ? SecondaryFlourType!.Value
            : FlourType;

    // True when either grain is bran-heavy with a meaningful share (≥30%)
    public bool HasMeaningfulBranContent =>
        (FlourType is FlourType.Rye or FlourType.WholeGrain && PrimaryFlourPercent >= 30)
        || (IsBlend && SecondaryFlourType is FlourType.Rye or FlourType.WholeGrain && SecondaryFlourPercent >= 30);

    public bool HasSourdoughStarter => StarterActivity != StarterActivity.NotAvailable;

    // Formula fields (M10)
    public int    TotalFlourGrams { get; set; } = 900;
    public double SaltPct         { get; set; } = 2.0;
    public double InoculationPct  { get; set; } = 20.0;

    // Yeast type (M25)
    public YeastType YeastType { get; set; } = YeastType.Sourdough;

    // Enriched dough fields (M23) — only sent when FlourType == Enriched
    public double ButterPct     { get; set; } = 10;
    public double EggPct        { get; set; } = 10;
    public double SugarPct      { get; set; } = 10;
    public double MilkPct       { get; set; } = 60;
    public double MilkPowderPct { get; set; } = 3;
    public bool   UseTangzhong  { get; set; } = true;
    public bool   IsPullmanTin  { get; set; } = false;
}

namespace BreadMaking.App.Shared.Dtos;

public class UserRecipeDto
{
    public int     Id           { get; set; }
    public string  Name         { get; set; } = "";
    public string  GrainName    { get; set; } = "";
    public string  Method       { get; set; } = "";
    public double  HydrationPct { get; set; }
    public double  FlourWeightG { get; set; }
    public double  SaltPct      { get; set; }
    public double  StarterPct   { get; set; }
    public string? Notes        { get; set; }
}

public class SaveRecipeRequest
{
    public string  Name         { get; set; } = "";
    public string  GrainName    { get; set; } = "";
    public string  Method       { get; set; } = "";
    public double  HydrationPct { get; set; }
    public double  FlourWeightG { get; set; }
    public double  SaltPct      { get; set; }
    public double  StarterPct   { get; set; }
    public string? Notes        { get; set; }
}

public class UpdateRecipeRequest
{
    public string  Name         { get; set; } = "";
    public double  HydrationPct { get; set; }
    public double  FlourWeightG { get; set; }
    public double  SaltPct      { get; set; }
    public double  StarterPct   { get; set; }
    public string? Notes        { get; set; }
}

namespace BreadMaking.App.Server.Data;

public class StarterFeedLog
{
    public int     Id               { get; set; }
    public int     StarterId        { get; set; }
    public Starter Starter          { get; set; } = null!;
    public DateTimeOffset FedAt     { get; set; }
    public double  FlourGrams       { get; set; }
    public double  WaterGrams       { get; set; }
    public double  PrevStarterGrams { get; set; }
    public double? AmbientTempC     { get; set; }
    public double? PeakHours        { get; set; }
    public bool?   FloatTestPassed  { get; set; }
    public string? FeedRatio        { get; set; }
}

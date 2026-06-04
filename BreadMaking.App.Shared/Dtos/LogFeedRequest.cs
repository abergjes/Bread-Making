namespace BreadMaking.App.Shared.Dtos;

public class LogFeedRequest
{
    public double  FlourGrams       { get; set; } = 50;
    public double  WaterGrams       { get; set; } = 50;
    public double  PrevStarterGrams { get; set; } = 10;
    public double? AmbientTempC     { get; set; }
    public double? PeakHours        { get; set; }
    public bool?   FloatTestPassed  { get; set; }
    public string? FeedRatio        { get; set; }
}

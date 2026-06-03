namespace BreadMaking.App.Shared.Dtos;

public class StarterFeedLogDto
{
    public int    Id               { get; set; }
    public int    StarterId        { get; set; }
    public string StarterName      { get; set; } = "";
    public DateTimeOffset FedAt    { get; set; }
    public double FlourGrams       { get; set; }
    public double WaterGrams       { get; set; }
    public double PrevStarterGrams { get; set; }
    public double? AmbientTempC    { get; set; }
    public double? PeakHours       { get; set; }
    public bool?   FloatTestPassed { get; set; }
}

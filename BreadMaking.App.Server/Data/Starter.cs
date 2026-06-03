namespace BreadMaking.App.Server.Data;

public class Starter
{
    public int    Id           { get; set; }
    public string Name         { get; set; } = "";
    public double HydrationPct { get; set; } = 100;
    public string? FlourBlend  { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? Notes       { get; set; }

    public ICollection<StarterFeedLog> Feeds { get; set; } = new List<StarterFeedLog>();
}

namespace BreadMaking.App.Shared.Dtos;

public class StarterDto
{
    public int    Id           { get; set; }
    public string Name         { get; set; } = "";
    public double HydrationPct { get; set; }
    public string? FlourBlend  { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? Notes       { get; set; }
    public List<StarterFeedLogDto> Feeds { get; set; } = [];
}

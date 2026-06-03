namespace BreadMaking.App.Shared.Dtos;

public class CreateStarterRequest
{
    public string  Name         { get; set; } = "";
    public double  HydrationPct { get; set; } = 100;
    public string? FlourBlend   { get; set; }
    public string? Notes        { get; set; }
}

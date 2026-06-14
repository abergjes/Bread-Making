namespace BreadMaking.App.Shared.Dtos;

public class CorrelationPointDto
{
    public int    BakeId    { get; set; }
    public string GrainName { get; set; } = "";
    public string Date      { get; set; } = "";
    public double X         { get; set; }
    public double Y         { get; set; }
}

namespace BreadMaking.App.Server.Data;

public class RecipeStep
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public int Order { get; set; }
    public string Name { get; set; } = "";
    public string? Phase { get; set; }
    public int DefaultDurationMin { get; set; }
    public int MinDurationMin { get; set; }
    public int MaxDurationMin { get; set; }
    public int StepMin { get; set; } = 5;
    public double? TargetTempC { get; set; }
    public string? Description { get; set; }

    public ICollection<BakeStepLog> StepLogs { get; set; } = new List<BakeStepLog>();
}

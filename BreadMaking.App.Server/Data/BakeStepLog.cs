using BreadMaking.App.Shared;

namespace BreadMaking.App.Server.Data;

public class BakeStepLog
{
    public int Id { get; set; }
    public int BakeId { get; set; }
    public Bake Bake { get; set; } = null!;
    public int RecipeStepId { get; set; }
    public RecipeStep RecipeStep { get; set; } = null!;
    public int PlannedDurationMin { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public StepStatus Status { get; set; } = StepStatus.NotStarted;
    public string? Notes { get; set; }

    public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();

    // Elapsed is derived — never stored as a ticking counter.
    // On resume from Paused, StartedAt is shifted forward to preserve accumulated elapsed.
    public TimeSpan Elapsed(DateTimeOffset now) =>
        StartedAt is null ? TimeSpan.Zero
        : (EndedAt ?? now) - StartedAt.Value;
}

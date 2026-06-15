namespace BreadMaking.App.Server.Data;

public class BreadFormula
{
    public int    Id        { get; set; }
    public string Name      { get; set; } = "";
    public string? Notes    { get; set; }
    public string? Tags     { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<FormulaIngredient> Ingredients { get; set; } = new List<FormulaIngredient>();
}

public class FormulaIngredient
{
    public int     Id        { get; set; }
    public int     FormulaId { get; set; }
    public string  Name      { get; set; } = "";
    public decimal Percent   { get; set; }
    public int     SortOrder { get; set; }

    public BreadFormula Formula { get; set; } = null!;
}

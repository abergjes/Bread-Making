namespace BreadMaking.App.Shared.Dtos;

public class FormulaIngredientDto
{
    public int     Id        { get; set; }
    public string  Name      { get; set; } = "";
    public decimal Percent   { get; set; }
    public int     SortOrder { get; set; }
}

public class FormulaDto
{
    public int    Id        { get; set; }
    public string Name      { get; set; } = "";
    public string? Notes    { get; set; }
    public string? Tags     { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<FormulaIngredientDto> Ingredients { get; set; } = [];
}

public class SaveFormulaRequest
{
    public string  Name      { get; set; } = "";
    public string? Notes     { get; set; }
    public string? Tags      { get; set; }
    public List<FormulaIngredientDto> Ingredients { get; set; } = [];
}

namespace BreadMaking.App.Shared.Dtos;

// ── Ingredient row — class so Blazor @bind works on editable lists ─────────────
public class IngredientPct
{
    public string  Name    { get; set; } = "";
    public decimal Percent { get; set; }
}

// ── 1. Baker's-% scaling (§48.1) ──────────────────────────────────────────────
public record ScaleRequest(List<IngredientPct> Formula, decimal TargetDoughGrams);
public record ScaleResultRow(string Name, decimal Percent, decimal Grams);
public record ScaleResult(List<ScaleResultRow> Rows, decimal TotalFormulaPct, decimal TotalGrams);

// ── 2. Batch scaling with yield / loss (§48.2) ────────────────────────────────
public record BatchRequest(
    int     Loaves,
    decimal BakedWeightG,
    decimal BakeLossPct,
    decimal ScaleLossPct,
    List<IngredientPct> Formula);

public record BatchResult(decimal DoughPerLoafG, decimal BatchDoughG, ScaleResult Scaling);

// ── 3. DDT water temperature (§48.3) ──────────────────────────────────────────
public record DdtRequest(
    decimal  Ddt,
    decimal  FlourC,
    decimal  RoomC,
    decimal  FrictionC,
    decimal? PrefermentC);

public record DdtResult(decimal WaterTempC, int FactorCount);

// ── 4. Levain split & true hydration (§48.4) ──────────────────────────────────
public record HydrationRequest(
    decimal TotalFlourG,
    decimal TargetHydrationPct,
    decimal LevainGrams,
    decimal LevainHydrationPct);

public record HydrationResult(
    decimal LevainFlour,
    decimal LevainWater,
    decimal FinalDoughFlour,
    decimal FinalDoughWater,
    decimal OverallHydrationPct);

// ── 5. Cost per loaf (§48.5) ──────────────────────────────────────────────────
public class CostIngredient
{
    public string  Name         { get; set; } = "";
    public decimal Grams        { get; set; }
    public decimal PricePer100g { get; set; }
}

public record CostRequest(
    List<CostIngredient> Ingredients,
    decimal EnergyCost,
    decimal LabourCost,
    decimal PackagingCost,
    decimal OverheadCost,
    int     SaleableLoaves);

public record CostResult(decimal IngredientCost, decimal TotalBatchCost, decimal CostPerLoaf);

// ── 6. Water-roux fold (§54.3) ────────────────────────────────────────────────
public record RouxRequest(
    decimal TotalFlour,
    decimal HydrationPct,
    decimal RouxFlourSharePct,
    decimal RouxRatio);       // 5.0 = Tangzhong 1:5  ·  1.0 = Yudane 1:1

public record RouxResult(
    decimal RouxFlour,
    decimal RouxLiquid,
    decimal DoughFlour,
    decimal DoughLiquid,
    decimal CheckTotalFlour,
    decimal CheckTotalLiquid,
    decimal CheckHydrationPct);

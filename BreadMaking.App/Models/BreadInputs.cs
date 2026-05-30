namespace BreadMaking.App.Models;

public class BreadInputs
{
    public double KitchenTemperatureC { get; set; } = 22;
    public FlourType FlourType { get; set; } = FlourType.White;
    public int HydrationPercent { get; set; } = 72;
    public StarterActivity StarterActivity { get; set; } = StarterActivity.NotAvailable;
    public FlavourGoal FlavourGoal { get; set; } = FlavourGoal.MildOpenCrumb;
    public BakerExperience Experience { get; set; } = BakerExperience.Experienced;

    public bool HasSourdoughStarter => StarterActivity != StarterActivity.NotAvailable;
}

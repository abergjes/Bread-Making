namespace BreadMaking.App.Server.Services;

public interface INotificationService
{
    Task SendFoldsReminderAsync(int bakeId);
    Task SendBulk50CrossedAsync(int bakeId);
    Task SendStepCompletedAsync(int bakeId, string stepName);
}

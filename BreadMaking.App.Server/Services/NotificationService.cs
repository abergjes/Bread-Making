using BreadMaking.App.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BreadMaking.App.Server.Services;

public class NotificationService(IHubContext<BakeHub> hub) : INotificationService
{
    public Task SendFoldsReminderAsync(int bakeId) =>
        hub.Clients.Group(BakeHub.GroupName(bakeId))
           .SendAsync("FoldsReminder", "Time for your next set of stretch and folds.");

    public Task SendBulk50CrossedAsync(int bakeId) =>
        hub.Clients.Group(BakeHub.GroupName(bakeId))
           .SendAsync("Bulk50Crossed", "Dough has crossed 50% rise — shaping window is approaching.");

    public Task SendStepCompletedAsync(int bakeId, string stepName) =>
        hub.Clients.Group(BakeHub.GroupName(bakeId))
           .SendAsync("StepCompleted", $"{stepName} complete.");
}

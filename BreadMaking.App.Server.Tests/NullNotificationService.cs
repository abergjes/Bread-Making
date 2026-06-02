using BreadMaking.App.Server.Services;

namespace BreadMaking.App.Server.Tests;

internal sealed class NullNotificationService : INotificationService
{
    public Task SendFoldsReminderAsync(int bakeId)              => Task.CompletedTask;
    public Task SendBulk50CrossedAsync(int bakeId)              => Task.CompletedTask;
    public Task SendStepCompletedAsync(int bakeId, string step) => Task.CompletedTask;
}

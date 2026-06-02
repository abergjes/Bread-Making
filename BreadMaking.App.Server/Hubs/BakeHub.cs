using Microsoft.AspNetCore.SignalR;

namespace BreadMaking.App.Server.Hubs;

public class BakeHub : Hub
{
    public Task JoinBake(int bakeId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupName(bakeId));

    public Task LeaveBake(int bakeId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(bakeId));

    public static string GroupName(int bakeId) => $"bake-{bakeId}";
}

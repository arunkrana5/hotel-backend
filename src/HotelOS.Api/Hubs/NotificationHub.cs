using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HotelOS.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public async Task JoinProperty(string propertyId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Property_{propertyId}");
    }

    public async Task LeaveProperty(string propertyId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Property_{propertyId}");
    }

    public override async Task OnConnectedAsync()
    {
        // Add to general notifications group
        await Groups.AddToGroupAsync(Context.ConnectionId, "AllUsers");
        await base.OnConnectedAsync();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

[Authorize]
public class NotificationHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new ConcurrentDictionary<string, string>();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.Identity?.Name;

        if (string.IsNullOrEmpty(userId))
        {
            Console.WriteLine("사용자 ID를 가져올 수 없음.");
        }
        else
        {
            ConnectedUsers[userId] = Context.ConnectionId;
            Console.WriteLine($"사용자 {userId} 접속됨.");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.Identity?.Name;

        if (!string.IsNullOrEmpty(userId))
        {
            await Task.Delay(5000);

            if (ConnectedUsers.ContainsKey(userId) && ConnectedUsers[userId] == Context.ConnectionId)
            {
                Console.WriteLine($"사용자 {userId} 접속 종료.");
                ConnectedUsers.TryRemove(userId, out _);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendNotification(string userId, string message)
    {
        if (!string.IsNullOrEmpty(userId) && ConnectedUsers.TryGetValue(userId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveNotification", message);
        }
    }

    public static bool IsUserOnline(string userId)
    {
        return !string.IsNullOrEmpty(userId) && ConnectedUsers.ContainsKey(userId);
    }
}

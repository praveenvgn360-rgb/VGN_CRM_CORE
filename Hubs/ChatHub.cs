using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace VGN_CRM_CORE.Hubs
{
    public class ChatHub : Hub
    {
        // Thread-safe dictionary to keep track of connection count per user
        public static ConcurrentDictionary<string, int> ActiveUsers = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext().Request.Query["userId"].ToString();
            if (!string.IsNullOrEmpty(userId))
            {
                ActiveUsers.AddOrUpdate(userId, 1, (key, count) => count + 1);
                await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToUpper());
                // Broadcast to all that this user is online
                await Clients.All.SendAsync("UserOnline", userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.GetHttpContext().Request.Query["userId"].ToString();
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToUpper());
                
                if (ActiveUsers.TryGetValue(userId, out int count))
                {
                    if (count > 1)
                    {
                        ActiveUsers[userId] = count - 1;
                    }
                    else
                    {
                        ActiveUsers.TryRemove(userId, out _);
                        await Clients.All.SendAsync("UserOffline", userId);
                    }
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string senderId, string receiverId, string message)
        {
            // The actual DB saving is done via an API endpoint
            await Clients.Group(receiverId.ToUpper()).SendAsync("ReceiveMessage", senderId, message, DateTime.Now.ToString("o"));
        }
    }
}

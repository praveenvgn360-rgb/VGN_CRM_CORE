using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace VGN_CRM_CORE.Hubs
{
    public class ChatHub : Hub
    {
        // Thread-safe dictionary to keep track of user connections
        // Map: UserId -> ConnectionId
        public static ConcurrentDictionary<string, string> ActiveUsers = new ConcurrentDictionary<string, string>();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext().Request.Query["userId"].ToString();
            if (!string.IsNullOrEmpty(userId))
            {
                ActiveUsers[userId] = Context.ConnectionId;
                // Broadcast to all that this user is online
                await Clients.All.SendAsync("UserOnline", userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = ActiveUsers.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (!string.IsNullOrEmpty(userId))
            {
                ActiveUsers.TryRemove(userId, out _);
                await Clients.All.SendAsync("UserOffline", userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string senderId, string receiverId, string message)
        {
            // The actual DB saving is done via an API endpoint or we can do it here if we pass connstring.
            // Let's rely on the API to save it to DB, and just use Hub to broadcast.
            // Actually, we can just broadcast. The client will call API to save, then call Hub to broadcast.
            // Or the client calls Hub, Hub saves to DB. 
            // We'll just pass the message object here.
            
            if (ActiveUsers.TryGetValue(receiverId, out string receiverConnectionId))
            {
                await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", senderId, message, DateTime.Now.ToString("o"));
            }
        }
    }
}

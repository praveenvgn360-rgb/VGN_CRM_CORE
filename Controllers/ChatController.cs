using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.DAL;
using VGN_CRM_CORE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using VGN_CRM_CORE.Hubs;
using System.Threading.Tasks;

namespace VGN_CRM_CORE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IConfiguration config, IHubContext<ChatHub> hubContext)
        {
            _config = config;
            _hubContext = hubContext;
            
            // Initialize DB on first access
            ChatDAL.InitDb(_config);
        }

        private string Conn => _config.GetActiveConnectionString("ConnLI");

        [HttpGet("onlineUsers")]
        public IActionResult GetOnlineUsers()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return Unauthorized();

            var users = ChatDAL.GetOnlineUsers(Conn, user.UserId);
            // Enhance with actual SignalR presence
            foreach(var u in users)
            {
                // If they are in ActiveUsers dict, they are truly online right now
                if(ChatHub.ActiveUsers.ContainsKey(u.UserId))
                {
                    // they are online
                }
            }
            return Ok(users);
        }

        [HttpGet("messages/{otherUserId}")]
        public IActionResult GetMessages(string otherUserId)
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return Unauthorized();

            var messages = ChatDAL.GetMessages(Conn, user.UserId, otherUserId);
            
            // Auto mark as read when fetching
            ChatDAL.MarkMessagesAsRead(Conn, otherUserId, user.UserId);
            
            return Ok(messages);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage msg)
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return Unauthorized();

            msg.SenderId = user.UserId;
            msg.SentAt = DateTime.Now;
            msg.IsRead = false;

            ChatDAL.SaveMessage(Conn, msg);

            // Broadcast via Hub to receiver if online (including MessageId)
            await _hubContext.Clients.Group(msg.ReceiverId.ToUpper()).SendAsync("ReceiveMessage", msg.SenderId, msg.Message, msg.SentAt.ToString("o"), msg.MessageId);

            return Ok(msg);
        }

        public class EditMessageRequest
        {
            public string ReceiverId { get; set; }
            public string Message { get; set; }
        }

        [HttpPut("edit/{messageId}")]
        public async Task<IActionResult> EditMessage(int messageId, [FromBody] EditMessageRequest req)
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return Unauthorized();

            ChatDAL.EditMessage(Conn, messageId, user.UserId, req.Message);

            await _hubContext.Clients.Group(req.ReceiverId.ToUpper()).SendAsync("MessageEdited", messageId, req.Message);
            return Ok();
        }

        public class DeleteMessageRequest
        {
            public string ReceiverId { get; set; }
        }

        [HttpDelete("delete/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId, [FromBody] DeleteMessageRequest req)
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return Unauthorized();

            ChatDAL.DeleteMessage(Conn, messageId, user.UserId);

            await _hubContext.Clients.Group(req.ReceiverId.ToUpper()).SendAsync("MessageDeleted", messageId);
            return Ok();
        }
        
        [HttpPost("markRead/{senderId}")]
        public IActionResult MarkAsRead(string senderId)
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return Unauthorized();

            ChatDAL.MarkMessagesAsRead(Conn, senderId, user.UserId);
            return Ok();
        }
    }
}

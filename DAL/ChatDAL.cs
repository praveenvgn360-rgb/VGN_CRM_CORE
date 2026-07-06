using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Models;

namespace VGN_CRM_CORE.Models
{
    public class ChatMessage
    {
        public int MessageId { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class OnlineUser
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string Department { get; set; }
        public int UnreadCount { get; set; }
    }
}

namespace VGN_CRM_CORE.DAL
{
    public static class ChatDAL
    {

        public static void InitDb(IConfiguration config)
        {
            string connString = config.GetActiveConnectionString("ConnLI");
            try
            {
                using (var con = new SqlConnection(connString))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("usp_InitChatDatabase", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChatDAL InitDb] {ex.Message}");
            }
        }

        public static void SaveMessage(string connString, ChatMessage msg)
        {
            try
            {
                using (var con = new SqlConnection(connString))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("usp_SaveChatMessage", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SenderId", msg.SenderId);
                        cmd.Parameters.AddWithValue("@ReceiverId", msg.ReceiverId);
                        cmd.Parameters.AddWithValue("@Message", msg.Message);
                        cmd.Parameters.AddWithValue("@SentAt", msg.SentAt);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChatDAL SaveMessage] {ex.Message}");
            }
        }

        public static void MarkMessagesAsRead(string connString, string senderId, string receiverId)
        {
            try
            {
                using (var con = new SqlConnection(connString))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("usp_MarkMessagesAsRead", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SenderId", senderId);
                        cmd.Parameters.AddWithValue("@ReceiverId", receiverId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChatDAL MarkMessagesAsRead] {ex.Message}");
            }
        }

        public static List<ChatMessage> GetMessages(string connString, string userId1, string userId2)
        {
            var list = new List<ChatMessage>();
            try
            {
                using (var con = new SqlConnection(connString))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("usp_GetChatMessages", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserId1", userId1);
                        cmd.Parameters.AddWithValue("@UserId2", userId2);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new ChatMessage
                                {
                                    MessageId = Convert.ToInt32(reader["MessageId"]),
                                    SenderId = reader["SenderId"].ToString(),
                                    ReceiverId = reader["ReceiverId"].ToString(),
                                    Message = reader["Message"].ToString(),
                                    SentAt = Convert.ToDateTime(reader["SentAt"]),
                                    IsRead = Convert.ToBoolean(reader["IsRead"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChatDAL GetMessages] {ex.Message}");
            }
            return list;
        }

        public static List<OnlineUser> GetOnlineUsers(string connString, string currentUserId)
        {
            var list = new List<OnlineUser>();
            try
            {
                using (var con = new SqlConnection(connString))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("usp_GetOnlineUsersForChat", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CurrentUserId", currentUserId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new OnlineUser
                                {
                                    UserId = reader["UserId"].ToString(),
                                    UserName = reader["UserName"].ToString(),
                                    UnreadCount = Convert.ToInt32(reader["UnreadCount"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChatDAL GetOnlineUsers] {ex.Message}");
            }
            return list;
        }
    }
}

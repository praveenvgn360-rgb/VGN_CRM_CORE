using System;
using System.Collections.Generic;
using System.Net;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using VGN_CRM_CORE.Models;

namespace VGN_CRM_CORE.CommonFunctions
{
    // ══════════════════════════════════════════════════════════
    // SESSION HELPER  — uses ISession (ASP.NET Core)
    // ══════════════════════════════════════════════════════════
    public static class SessionHelper
    {
        private const string SESSION_KEY = "VGN_CurrentUser";

        public static void SetUserSession(ISession session, UserSession user)
        {
            session.SetString(SESSION_KEY, JsonConvert.SerializeObject(user));
        }

        public static UserSession GetUserSession(ISession session)
        {
            var raw = session.GetString(SESSION_KEY);
            return string.IsNullOrEmpty(raw) ? null : JsonConvert.DeserializeObject<UserSession>(raw);
        }

        public static void ClearSession(ISession session)
        {
            session.Remove(SESSION_KEY);
            session.Clear();
        }

        public static bool IsLoggedIn(ISession session)
        {
            return !string.IsNullOrEmpty(session.GetString(SESSION_KEY));
        }

        // ── Menu list helpers ────────────────────────────────
        private const string MENU_KEY = "VGN_MenuList";

        public static void SetMenuList(ISession session, List<Models.MenuModel> menus)
        {
            session.SetString(MENU_KEY, JsonConvert.SerializeObject(menus));
        }

        public static List<Models.MenuModel> GetMenuList(ISession session)
        {
            var raw = session.GetString(MENU_KEY);
            return string.IsNullOrEmpty(raw)
                ? null
                : JsonConvert.DeserializeObject<List<Models.MenuModel>>(raw);
        }

        // ── Network helpers ──────────────────────────────────
        public static string GetClientIPAddress(HttpRequest request)
        {
            // Check forwarded header first (proxy/load-balancer)
            var forwarded = request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwarded))
            {
                var parts = forwarded.Split(',');
                return parts[0].Trim();
            }
            return request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        public static string GetClientHostName(string ipAddress)
        {
            try
            {
                if (ipAddress == "::1" || ipAddress == "127.0.0.1")
                    return Dns.GetHostName();

                var entry = Dns.GetHostEntry(ipAddress);
                return entry.HostName;
            }
            catch { return ipAddress; }
        }

        public static string GetBrowserInfo(HttpRequest request)
        {
            var ua = request.Headers["User-Agent"].ToString();
            return string.IsNullOrEmpty(ua) ? "Unknown" : ua;
        }
    }

    // ══════════════════════════════════════════════════════════
    // SESSION CHECK RESULT  — three-way session state
    // ══════════════════════════════════════════════════════════
    public enum SessionCheckResult
    {
        /// <summary>Row found in tbl_UserSessionTracker and IsActive = 1.</summary>
        Valid,
        /// <summary>Row found but IsActive = 0 (force-logout or server-side timeout).</summary>
        Invalidated,
        /// <summary>No row at all — DB restart, missing entry, or user has never logged in via the tracker.</summary>
        NotFound
    }

    // ══════════════════════════════════════════════════════════
    // AUDIT LOGGER  — all DB writes go here
    // Uses IConfiguration seeded once from Startup.cs
    // ══════════════════════════════════════════════════════════
    public static class AuditLogger
    {
        private static string _erpConn;

        public static void Initialize(IConfiguration config)
        {
            _erpConn = config.GetActiveConnectionString("ConnLI");
        }

        private static string Conn => _erpConn
            ?? throw new InvalidOperationException("AuditLogger not initialized. Call AuditLogger.Initialize() in Startup.");

        /// <summary>Insert a login record capturing IP, hostname, browser, session.</summary>
        public static void LogLogin(UserLoginAudit audit)
        {
            Try(() =>
            {
                using (var con = new SqlConnection(Conn))
                using (var cmd = new SqlCommand("usp_ManageUserSession", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "LOGIN");
                    cmd.Parameters.AddWithValue("@SessionId", audit.SessionId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserId", audit.UserId);
                    cmd.Parameters.AddWithValue("@UserName", audit.UserName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IPAddress", audit.IPAddress ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@HostName", audit.HostName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BrowserInfo", audit.BrowserInfo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TimeoutMinutes", 20);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            });
        }

        /// <summary>Stamp the logout time and mark session inactive.</summary>
        public static void LogLogout(string sessionId)
        {
            Try(() =>
            {
                using (var con = new SqlConnection(Conn))
                using (var cmd = new SqlCommand("usp_ManageUserSession", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "LOGOUT");
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            });
        }

        /// <summary>Record every page the user navigates to.</summary>
        public static void LogPageVisit(PageVisitLog log)
        {
            Try(() =>
            {
                using (var con = new SqlConnection(Conn))
                using (var cmd = new SqlCommand("usp_ManageUserSession", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "LOG_SCREEN");
                    cmd.Parameters.AddWithValue("@SessionId", log.SessionId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserId", log.UserId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ScreenName", log.PageTitle ?? log.PageUrl ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TimeoutMinutes", 20);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            });
        }

        /// <summary>Save or update user theme/layout preferences.</summary>
        public static void SavePreferences(UserPreferences pref)
        {
            Try(() =>
            {
                const string sql = @"
                    MERGE tbl_UserPreferences AS target
                    USING (SELECT @UserId AS UserId) AS source ON target.UserId = source.UserId
                    WHEN MATCHED THEN
                        UPDATE SET ThemeMode=@ThemeMode, ThemeColor=@ThemeColor,
                                   SidebarType=@SidebarType, NavLayout=@NavLayout,
                                   Contrast=@Contrast, UpdatedOn=GETDATE()
                    WHEN NOT MATCHED THEN
                        INSERT (UserId, ThemeMode, ThemeColor, SidebarType, NavLayout, Contrast)
                        VALUES (@UserId, @ThemeMode, @ThemeColor, @SidebarType, @NavLayout, @Contrast);";

                using (var con = new SqlConnection(Conn))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@UserId",      pref.UserId);
                    cmd.Parameters.AddWithValue("@ThemeMode",   pref.ThemeMode);
                    cmd.Parameters.AddWithValue("@ThemeColor",  pref.ThemeColor);
                    cmd.Parameters.AddWithValue("@SidebarType", pref.SidebarType);
                    cmd.Parameters.AddWithValue("@NavLayout",   pref.NavLayout);
                    cmd.Parameters.AddWithValue("@Contrast",    pref.Contrast);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            });
        }

        public static UserPreferences GetPreferences(string userId)
        {
            var pref = new UserPreferences { UserId = userId };
            Try(() =>
            {
                const string sql = "SELECT * FROM tbl_UserPreferences WHERE UserId=@UserId";
                using (var con = new SqlConnection(Conn))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            pref.ThemeMode   = rdr["ThemeMode"].ToString();
                            pref.ThemeColor  = rdr["ThemeColor"].ToString();
                            pref.SidebarType = rdr["SidebarType"].ToString();
                            pref.NavLayout   = rdr["NavLayout"].ToString();
                            pref.Contrast    = rdr["Contrast"].ToString();
                        }
                    }
                }
            });
            return pref;
        }

        public static bool IsUserAlreadyLoggedIn(string userId)
        {
            bool loggedIn = false;
            Try(() =>
            {
                using (var con = new SqlConnection(Conn))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("usp_ManageUserSession", con))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Action", "CHECK_CONCURRENT");
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@TimeoutMinutes", 20);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        loggedIn = count > 0;
                    }
                }
            });
            return loggedIn;
        }

        public static void ForceLogoutUser(string userId)
        {
            Try(() =>
            {
                using (var con = new SqlConnection(Conn))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("UPDATE tbl_UserSessionTracker SET IsActive = 0, LogoutTime = GETDATE() WHERE UserId = @UserId AND IsActive = 1", con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
            });
        }

        /// <summary>
        /// Kept for backward-compatibility. Prefer CheckSessionStatus for fine-grained control.
        /// Returns true only when an active DB record exists for this session.
        /// </summary>
        public static bool IsSessionValid(string sessionId)
            => CheckSessionStatus(sessionId) == SessionCheckResult.Valid;

        /// <summary>
        /// Queries the DB to determine the exact state of a session:
        ///   Valid       – row exists and IsActive = 1 (and not timed out)
        ///   Invalidated – row exists but IsActive = 0  (force-logout / timeout)
        ///   NotFound    – no row at all (DB restart, missing record, user never logged in via this SP)
        /// On any DB error we return Valid so we never wrongly lock out a user due to connectivity.
        /// </summary>
        public static SessionCheckResult CheckSessionStatus(string sessionId)
        {
            SessionCheckResult result = SessionCheckResult.Valid; // safe default on DB error
            Try(() =>
            {
                const string sql = @"
                    SELECT
                        CASE
                            WHEN COUNT(*) = 0                          THEN 0  -- NotFound
                            WHEN MAX(CAST(IsActive AS INT)) = 1        THEN 1  -- Valid
                            ELSE                                            2  -- Invalidated (IsActive=0)
                        END
                    FROM tbl_UserSessionTracker
                    WHERE SessionId = @SessionId";

                using (var con = new SqlConnection(Conn))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    con.Open();
                    int code = Convert.ToInt32(cmd.ExecuteScalar());
                    result = code == 1 ? SessionCheckResult.Valid
                           : code == 2 ? SessionCheckResult.Invalidated
                           :             SessionCheckResult.NotFound;
                }
            });
            return result;
        }

        // ── private helper ───────────────────────────────────
        private static void Try(Action action)
        {
            try { action(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuditLogger] {ex.Message}");
            }
        }
    }

    // ══════════════════════════════════════════════════════════
    // PASSWORD HELPER  (simple SHA-256; swap for BCrypt in prod)
    // ══════════════════════════════════════════════════════════
    public static class PasswordHelper
    {
        public static string Hash(string password)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash  = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public static bool Verify(string password, string hash) =>
            Hash(password) == hash;
    }

    // ══════════════════════════════════════════════════════════
    // CONFIGURATION EXTENSION (Automatic Network Failover)
    // ══════════════════════════════════════════════════════════
    public static class ConfigurationExtensions
    {
        private static bool _useFallback = false;
        private static DateTime _lastCheck = DateTime.MinValue;
        private static readonly object _lock = new object();

        public static string GetActiveConnectionString(this IConfiguration config, string name)
        {
            if ((DateTime.Now - _lastCheck).TotalSeconds > 30)
            {
                lock (_lock)
                {
                    if ((DateTime.Now - _lastCheck).TotalSeconds > 30)
                    {
                        string primary = config.GetSection("ConnectionStrings")[name];
                        if (primary != null)
                        {
                            bool primaryWorks = TestConnection(primary);
                            if (primaryWorks)
                            {
                                _useFallback = false;
                            }
                            else
                            {
                                string fallback = config.GetSection("ConnectionStringsFallback")[name];
                                if (fallback != null && TestConnection(fallback))
                                {
                                    _useFallback = true;
                                }
                            }
                            _lastCheck = DateTime.Now;
                        }
                    }
                }
            }

            if (_useFallback)
            {
                string fallback = config.GetSection("ConnectionStringsFallback")[name];
                return fallback ?? config.GetSection("ConnectionStrings")[name];
            }

            return config.GetSection("ConnectionStrings")[name];
        }

        private static bool TestConnection(string connectionString)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                string server = builder.DataSource;
                if (server.Contains(","))
                {
                    server = server.Split(',')[0];
                }
                
                using (var tcp = new System.Net.Sockets.TcpClient())
                {
                    var result = tcp.BeginConnect(server, 1433, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                    if (!success)
                    {
                        return false;
                    }
                    tcp.EndConnect(result);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}

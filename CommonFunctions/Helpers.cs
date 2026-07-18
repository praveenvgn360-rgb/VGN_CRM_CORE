using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using VGN_CRM_CORE.Models;

namespace VGN_CRM_CORE.CommonFunctions
{
    // ══════════════════════════════════════════════════════════
    // JWT HELPER  — stores user identity in an HttpOnly cookie
    //               containing a signed JWT token.
    //
    // SessionId = Guid.NewGuid() generated at LOGIN time.
    //             It is stored BOTH inside the JWT payload and
    //             in the DB (tbl_UserSessionTracker).
    //             It has nothing to do with ASP.NET Session.Id,
    //             so it remains stable across browser restarts
    //             and is unique per login, not per browser.
    // ══════════════════════════════════════════════════════════
    public static class SessionHelper
    {
        public static IHttpContextAccessor HttpContextAccessor { get; set; }

        // Cookie names
        private const string JWT_COOKIE  = "vgn_jwt";
        private const string MENU_COOKIE = "vgn_menu";

        // ── JWT secret and expiry timings read from configuration ────────────
        private static string _jwtSecret;
        private static int    _expiryMinutes      = 20;  // fallback
        private static int    _idleTimeoutMinutes = 20;  // fallback
        private static int    _idleWarningMinutes = 3;   // fallback

        /// <summary>How long (minutes) the JWT lives after the last activity ping.</summary>
        public static int ExpiryMinutes      => _expiryMinutes;
        /// <summary>Minutes of inactivity before the client auto-logs out.</summary>
        public static int IdleTimeoutMinutes => _idleTimeoutMinutes;
        /// <summary>Minutes before idle-logout at which to show a warning.</summary>
        public static int IdleWarningMinutes => _idleWarningMinutes;

        public static void Initialize(IConfiguration config)
        {
            _jwtSecret = config["Jwt:Secret"]
                ?? "VGN360_SuperSecret_JWT_Key_2024!@#$%^&*_MustBe256BitsLong_RandomString";

            if (int.TryParse(config["Jwt:ExpiryMinutes"],      out int exp))  _expiryMinutes      = exp;
            if (int.TryParse(config["Jwt:IdleTimeoutMinutes"], out int idle)) _idleTimeoutMinutes = idle;
            if (int.TryParse(config["Jwt:IdleWarningMinutes"], out int warn)) _idleWarningMinutes = warn;
        }

        private static string JwtSecret => _jwtSecret
            ?? "VGN360_SuperSecret_JWT_Key_2024!@#$%^&*_MustBe256BitsLong_RandomString";

        // ─────────────────────────────────────────────────────
        //  Write JWT cookie (called on login)
        // ─────────────────────────────────────────────────────
        public static void SetUserSession(ISession session, UserSession user)
        {
            var context = HttpContextAccessor?.HttpContext;
            if (context == null) return;

            var jwt = BuildJwt(user);
            WriteJwtCookie(context, jwt);
        }

        // ─────────────────────────────────────────────────────
        //  Read & validate JWT cookie
        // ─────────────────────────────────────────────────────
        public static UserSession GetUserSession(ISession session)
        {
            var context = HttpContextAccessor?.HttpContext;
            if (context == null) return null;

            return ValidateJwtCookie(context);
        }

        // ─────────────────────────────────────────────────────
        //  Silently renew the JWT cookie expiry
        //  (call from filter on every authenticated request)
        // ─────────────────────────────────────────────────────
        public static void RefreshJwt(UserSession user)
        {
            var context = HttpContextAccessor?.HttpContext;
            if (context == null || user == null) return;

            // Re-issue a fresh token with the SAME SessionId so the DB
            // record is still matched, but with a new expiry timestamp.
            var jwt = BuildJwt(user);
            WriteJwtCookie(context, jwt);
        }

        // ─────────────────────────────────────────────────────
        //  Clear JWT cookie (called on logout)
        // ─────────────────────────────────────────────────────
        public static void ClearSession(ISession session)
        {
            var context = HttpContextAccessor?.HttpContext;
            if (context == null) return;

            context.Response.Cookies.Delete(JWT_COOKIE,  new CookieOptions { Path = "/" });
            // Also delete the old cookie if it exists in the client's browser
            context.Response.Cookies.Delete(MENU_COOKIE, new CookieOptions { Path = "/" });
            session?.Remove(MENU_COOKIE);
        }

        // ─────────────────────────────────────────────────────
        //  Quick auth check
        // ─────────────────────────────────────────────────────
        public static bool IsLoggedIn(ISession session)
            => GetUserSession(session) != null;

        // ── Menu list helpers ─────────────────────────────────

        public static void SetMenuList(ISession session, List<Models.MenuModel> menus)
        {
            if (session == null || menus == null) return;
            session.SetString(MENU_COOKIE, JsonConvert.SerializeObject(menus));
        }

        public static List<Models.MenuModel> GetMenuList(ISession session)
        {
            if (session == null) return null;
            var raw = session.GetString(MENU_COOKIE);
            return string.IsNullOrEmpty(raw)
                ? null
                : JsonConvert.DeserializeObject<List<Models.MenuModel>>(raw);
        }

        // ── Network helpers ───────────────────────────────────

        public static string GetClientIPAddress(HttpRequest request)
        {
            var forwarded = request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwarded))
                return forwarded.Split(',')[0].Trim();
            return request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        public static string GetClientHostName(string ipAddress)
        {
            try
            {
                if (ipAddress == "::1" || ipAddress == "127.0.0.1")
                    return Dns.GetHostName();
                return Dns.GetHostEntry(ipAddress).HostName;
            }
            catch { return ipAddress; }
        }

        public static string GetBrowserInfo(HttpRequest request)
        {
            var ua = request.Headers["User-Agent"].ToString();
            return string.IsNullOrEmpty(ua) ? "Unknown" : ua;
        }

        // ── Private helpers ────────────────────────────────────

        private static string BuildJwt(UserSession user)
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key     = System.Text.Encoding.ASCII.GetBytes(JwtSecret);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim("jti",      user.SessionId),
                    new System.Security.Claims.Claim("UserData", JsonConvert.SerializeObject(user))
                }),
                Expires            = DateTime.UtcNow.AddMinutes(_expiryMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }

        private static void WriteJwtCookie(HttpContext context, string jwt)
        {
            context.Response.Cookies.Append(JWT_COOKIE, jwt, new CookieOptions
            {
                HttpOnly = true,
                Secure   = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires  = DateTime.UtcNow.AddMinutes(_expiryMinutes)
            });
        }

        private static UserSession ValidateJwtCookie(HttpContext context)
        {
            var jwt = context.Request.Cookies[JWT_COOKIE];
            if (string.IsNullOrEmpty(jwt)) return null;

            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var key     = System.Text.Encoding.ASCII.GetBytes(JwtSecret);

                handler.ValidateToken(jwt,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey         = new SymmetricSecurityKey(key),
                        ValidateIssuer           = false,
                        ValidateAudience         = false,
                        ClockSkew                = TimeSpan.Zero
                    },
                    out SecurityToken validated);

                var jwtToken = (System.IdentityModel.Tokens.Jwt.JwtSecurityToken)validated;
                var userData = jwtToken.Claims.First(x => x.Type == "UserData").Value;
                return JsonConvert.DeserializeObject<UserSession>(userData);
            }
            catch
            {
                return null;
            }
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
                    cmd.Parameters.AddWithValue("@Action",      "LOGIN");
                    cmd.Parameters.AddWithValue("@SessionId",   audit.SessionId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserId",      audit.UserId);
                    cmd.Parameters.AddWithValue("@UserName",    audit.UserName   ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IPAddress",   audit.IPAddress  ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@HostName",    audit.HostName   ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BrowserInfo", audit.BrowserInfo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TimeoutMinutes", 480);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            });
        }

        /// <summary>Stamp the logout time and mark session inactive.</summary>
        public static void LogLogout(string sessionId, string reason = "User Logged Out")
        {
            Try(() =>
            {
                using (var con = new SqlConnection(Conn))
                using (var cmd = new SqlCommand("usp_ManageUserSession", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action",    "LOGOUT");
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    cmd.Parameters.AddWithValue("@LogoutReason", reason);
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
                    cmd.Parameters.AddWithValue("@Action",         "LOG_SCREEN");
                    cmd.Parameters.AddWithValue("@SessionId",      log.SessionId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserId",         log.UserId    ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ScreenName",     log.PageTitle ?? log.PageUrl ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TimeoutMinutes", 480);
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
                using (var con = new SqlConnection(Conn))
                using (var cmd = new SqlCommand("usp_ManageUserPreferences", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action",      "SAVE");
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
                using (var con = new SqlConnection(Conn))
                using (var cmd = new SqlCommand("usp_ManageUserPreferences", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GET");
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

        public static void ResetPreferences(string userId)
        {
            Try(() =>
            {
                using (var con = new SqlConnection(Conn))
                using (var cmd = new SqlCommand("usp_ManageUserPreferences", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "RESET");
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            });
        }

        /// <summary>
        /// Returns true if there is already an ACTIVE session for this UserId in the DB.
        /// This is the single-session enforcement check used at login time.
        /// </summary>
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
                        cmd.Parameters.AddWithValue("@Action",         "CHECK_CONCURRENT");
                        cmd.Parameters.AddWithValue("@UserId",         userId);
                        cmd.Parameters.AddWithValue("@TimeoutMinutes", 480);
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
                using (var cmd = new SqlCommand("usp_ManageUserSession", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "FORCE_LOGOUT_OTHER");
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            });
        }

        public static void TimeoutLogoutUser(string sessionId)
        {
            Try(() =>
            {
                using (var con = new SqlConnection(Conn))
                using (var cmd = new SqlCommand("usp_ManageUserSession", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "TIMEOUT_LOGOUT");
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    con.Open();
                    cmd.ExecuteNonQuery();
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
        ///   NotFound    – no row at all (DB restart, missing record)
        /// On any DB error we return Valid so we never wrongly lock out a user.
        /// </summary>
        public static SessionCheckResult CheckSessionStatus(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId)) return SessionCheckResult.NotFound;

            SessionCheckResult result = SessionCheckResult.Valid; // safe default on DB error
            Try(() =>
            {
                const string sql = @"
                    SELECT
                        CASE
                            WHEN COUNT(*) = 0                   THEN 0  -- NotFound
                            WHEN MAX(CAST(IsActive AS INT)) = 1 THEN 1  -- Valid
                            ELSE                                     2  -- Invalidated (IsActive=0)
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

        // ── private helper ────────────────────────────────────
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
    // PASSWORD HELPER  (simple SHA-256)
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
                                    _useFallback = true;
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
                if (server.Contains(",")) server = server.Split(',')[0];

                using (var tcp = new System.Net.Sockets.TcpClient())
                {
                    var result  = tcp.BeginConnect(server, 1433, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                    if (!success) return false;
                    tcp.EndConnect(result);
                    return true;
                }
            }
            catch { return false; }
        }
    }
}

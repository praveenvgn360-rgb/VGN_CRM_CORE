using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Models;

namespace VGN_CRM_CORE.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _config;

        public AccountController(IConfiguration config)
        {
            _config = config;
        }

        // ── GET /Account/Login ────────────────────────────────
        [HttpGet]
        public IActionResult Login(string ReturnUrl = "")
        {
            // Check if the filter set a "session expired" signal via TempData.
            bool sessionExpired = TempData.ContainsKey("SessionExpired") &&
                                  TempData["SessionExpired"]?.ToString() == "true";

            if (sessionExpired)
            {
                SessionHelper.ClearSession(HttpContext.Session);
                ViewBag.KickedOut = true;
            }
            else if (SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                // Validate the JWT session against DB before redirecting.
                var activeUser = SessionHelper.GetUserSession(HttpContext.Session);
                var dbState    = activeUser != null
                    ? AuditLogger.CheckSessionStatus(activeUser.SessionId)
                    : SessionCheckResult.NotFound;

                if (dbState == SessionCheckResult.Valid)
                    return RedirectToAction("Index", "GeneralDashboard");

                // Stale cookie — clear and let user log in fresh
                SessionHelper.ClearSession(HttpContext.Session);
            }

            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        // ── POST /Account/Login ───────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1. Validate credentials against DB
            var dbUser = GetUserFromDB(model.UserId, model.Password, model.LoginType);

            if (dbUser == null)
            {
                ModelState.AddModelError("", "Invalid User ID or Password.");
                return View(model);
            }

            if (!dbUser.IsActive)
            {
                ModelState.AddModelError("", "Your account is inactive. Contact admin.");
                return View(model);
            }

            // 2. Single-session enforcement: if this UserId already has an
            //    active session in the DB, forcefully log it out so this new login succeeds.
            if (AuditLogger.IsUserAlreadyLoggedIn(model.UserId))
            {
                AuditLogger.ForceLogoutUser(model.UserId);
            }

            // 3. Generate a unique SessionId (Guid) — NOT tied to ASP.NET Session.Id.
            //    This Guid is stored in the JWT and in tbl_UserSessionTracker.
            //    It is stable across all browsers and survives cookie-only scenarios.
            string sessionId = Guid.NewGuid().ToString();

            // 4. Collect client info
            string ip       = SessionHelper.GetClientIPAddress(Request);
            string hostname = SessionHelper.GetClientHostName(ip);
            string browser  = SessionHelper.GetBrowserInfo(Request);

            // 5. Build session object — embed the Guid SessionId
            var userSession = new UserSession
            {
                UserId             = dbUser.UserId,
                UserName           = dbUser.UserName,
                Email              = dbUser.Email,
                MobileNo           = dbUser.MobileNo,
                Role               = dbUser.Role,
                Designation        = dbUser.Designation,
                DepartmentId       = dbUser.DepartmentId,
                Department         = dbUser.Department,
                RptEmpName         = dbUser.RptEmpName,
                ReprotingMailId    = dbUser.ReprotingMailId,
                ReportingMobileNum = dbUser.ReportingMobileNum,
                LoginType          = string.IsNullOrEmpty(model.LoginType) ? "CORPORATE" : model.LoginType.ToUpper(),
                SessionId          = sessionId,          // ← Guid, NOT HttpContext.Session.Id
                LoginTime          = DateTime.Now,
                IPAddress          = ip,
                HostName           = hostname
            };

            // 6. Write the JWT cookie
            SessionHelper.SetUserSession(HttpContext.Session, userSession);

            // 7. Write audit record to DB (uses the same Guid SessionId)
            AuditLogger.LogLogin(new UserLoginAudit
            {
                UserId      = dbUser.UserId,
                UserName    = dbUser.UserName,
                IPAddress   = ip,
                HostName    = hostname,
                BrowserInfo = browser,
                LoginTime   = DateTime.Now,
                SessionId   = sessionId                  // ← same Guid stored in DB
            });

            // 8. Redirect
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "GeneralDashboard");
        }

        // ── GET /Account/Logout ───────────────────────────────
        [HttpGet]
        public IActionResult Logout(string reason = "User Logged Out")
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user != null)
                AuditLogger.LogLogout(user.SessionId, reason);

            SessionHelper.ClearSession(HttpContext.Session);
            return RedirectToAction("Login", "Account");
        }

        // ── GET /Account/CheckSession (AJAX heartbeat / activity ping) ──
        // Called by the client in two scenarios:
        //   1. active=true  → user just moved/clicked/typed. Renew JWT (resets idle clock).
        //   2. active=false → idle poll. Only validate; do NOT renew. JWT will expire
        //                     naturally after 20 min of no activity, kicking the user out.
        [HttpGet]
        public IActionResult CheckSession(bool active = false)
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null)
                return Json(new { valid = false, reason = "no_cookie" });

            var status = AuditLogger.CheckSessionStatus(user.SessionId);

            if (status == SessionCheckResult.Valid)
            {
                // Only re-issue the cookie (extend idle clock) when the user is active.
                // If just validating (idle poll), leave the JWT expiry unchanged.
                if (active)
                    SessionHelper.RefreshJwt(user);

                return Json(new { valid = true });
            }

            if (status == SessionCheckResult.Invalidated)
                return Json(new { valid = false, reason = "invalidated" });

            // NotFound — stale cookie
            SessionHelper.ClearSession(HttpContext.Session);
            return Json(new { valid = false, reason = "not_found" });
        }

        // ── DB helper ─────────────────────────────────────────
        private AppUser GetUserFromDB(string userId, string password, string loginType)
        {
            try
            {
                string conn = _config.GetActiveConnectionString("ConnDB");
                using (var con = new SqlConnection(conn))
                using (var cmd = new SqlCommand("Web_UserLogin", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmpId",    userId);
                    cmd.Parameters.AddWithValue("@Password", password);

                    string flag = (loginType == "CHANNELPARTNER") ? "CPlogin" : "Login";
                    cmd.Parameters.AddWithValue("@Flag",   flag);
                    cmd.Parameters.AddWithValue("@Outval", "");

                    var ds = new DataSet();
                    using (var da = new SqlDataAdapter(cmd)) { da.Fill(ds); }

                    if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        var row = ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1];

                        string userType   = Col(row, "UserType");
                        string role       = !string.IsNullOrEmpty(Col(row, "Role")) ? Col(row, "Role") : userType;
                        string designation = Col(row, "Designation");

                        return new AppUser
                        {
                            UserId             = !string.IsNullOrEmpty(Col(row, "UserId"))   ? Col(row, "UserId")   : userId,
                            UserName           = !string.IsNullOrEmpty(Col(row, "UserName")) ? Col(row, "UserName") : userId,
                            Email              = Col(row, "EmailId"),
                            MobileNo           = Col(row, "MobileNo"),
                            Password           = password,
                            Role               = role,
                            Designation        = designation,
                            DepartmentId       = !string.IsNullOrEmpty(Col(row, "DepartmentID")) ? Col(row, "DepartmentID") : Col(row, "DeptId"),
                            Department         = Col(row, "Department"),
                            RptEmpName         = Col(row, "RptEmpName"),
                            ReprotingMailId    = Col(row, "ReprotingMailId"),
                            ReportingMobileNum = Col(row, "ReportingMobileNum"),
                            IsActive           = true
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AccountController] DB Error: {ex.Message}");
            }
            return null;
        }

        // Safe DataRow column reader
        private static string Col(DataRow row, string col)
        {
            return row.Table.Columns.Contains(col) && row[col] != DBNull.Value
                ? row[col].ToString()
                : "";
        }
    }
}

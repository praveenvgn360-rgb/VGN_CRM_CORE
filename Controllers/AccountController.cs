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

        // GET /Account/Login
        [HttpGet]
        public IActionResult Login(string ReturnUrl = "")
        {
            // Check if the filter set a "session expired" signal via TempData.
            // TempData is one-time-read — it cannot be triggered by a bookmarked
            // URL, a page refresh, or a manually typed query string.
            bool sessionExpired = TempData.ContainsKey("SessionExpired") &&
                                  TempData["SessionExpired"]?.ToString() == "true";

            if (sessionExpired)
            {
                // Clear any stale in-memory session data so it cannot redirect to dashboard
                SessionHelper.ClearSession(HttpContext.Session);
                ModelState.AddModelError("", "Your session has expired due to inactivity or login from another device. Please log in again.");
            }
            else if (SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                // Validate the session is still genuinely active in the DB.
                // If the DB record is gone or invalidated, clear the stale cookie
                // instead of bouncing the user to the dashboard.
                var activeUser = SessionHelper.GetUserSession(HttpContext.Session);
                var dbState = activeUser != null
                    ? AuditLogger.CheckSessionStatus(activeUser.SessionId)
                    : SessionCheckResult.NotFound;

                if (dbState == SessionCheckResult.Valid)
                {
                    return RedirectToAction("Index", "GeneralDashboard");
                }

                // Stale cookie — clear it and let the user log in fresh
                SessionHelper.ClearSession(HttpContext.Session);
            }

            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        // POST /Account/Login
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

            // ── Step 1: Clean up any previous active DB sessions for this userId
            // This handles: crash recovery, tab close without logout, cross-device login
            AuditLogger.ForceLogoutUser(model.UserId);

            // ── Step 2: Clear old session data.
            // We do NOT delete the session cookie here. Deleting it would instruct the 
            // browser to discard the session we are about to establish, resulting in 
            // an immediate redirection back to the login page on the next request.
            HttpContext.Session.Clear();
            string sessionId = HttpContext.Session.Id;

            // 2. Collect client info
            string ip        = SessionHelper.GetClientIPAddress(Request);
            string hostname  = SessionHelper.GetClientHostName(ip);
            string browser   = SessionHelper.GetBrowserInfo(Request);

            // 3. Build session object
            var userSession = new UserSession
            {
                UserId      = dbUser.UserId,
                UserName    = dbUser.UserName,
                Email       = dbUser.Email,
                MobileNo    = dbUser.MobileNo,
                Role        = dbUser.Role,
                Designation = dbUser.Designation,
                DepartmentId = dbUser.DepartmentId,
                Department = dbUser.Department,
                RptEmpName = dbUser.RptEmpName,
                ReprotingMailId = dbUser.ReprotingMailId,
                ReportingMobileNum = dbUser.ReportingMobileNum,
                LoginType   = string.IsNullOrEmpty(model.LoginType) ? "CORPORATE" : model.LoginType.ToUpper(),
                SessionId   = sessionId,
                LoginTime   = DateTime.Now,
                IPAddress   = ip,
                HostName    = hostname
            };

            SessionHelper.SetUserSession(HttpContext.Session, userSession);

            // 4. Write audit record to DB
            AuditLogger.LogLogin(new UserLoginAudit
            {
                UserId      = dbUser.UserId,
                UserName    = dbUser.UserName,
                IPAddress   = ip,
                HostName    = hostname,
                BrowserInfo = browser,
                LoginTime   = DateTime.Now,
                SessionId   = sessionId
            });

            // 5. Redirect
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "GeneralDashboard");
        }

        // GET /Account/Logout
        public IActionResult Logout()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user != null)
                AuditLogger.LogLogout(user.SessionId);

            // 1. Clear all session data from the server-side store.
            HttpContext.Session.Clear();

            // 2. Explicitly delete the session cookie from the browser.
            //    session.Clear() alone does NOT remove the cookie — the browser keeps
            //    sending the same SessionId on the next request, which maps to the
            //    DB record with IsActive=0, causing a false "already logged in" block.
            HttpContext.Response.Cookies.Delete(".AspNetCore.Session");

            return RedirectToAction("Login", "Account");
        }

        // ── DB helper ────────────────────────────────────────────
        private AppUser GetUserFromDB(string userId, string password, string loginType)
        {
            try
            {
                string conn = _config.GetActiveConnectionString("ConnDB");
                using (var con = new SqlConnection(conn))
                using (var cmd = new SqlCommand("Web_UserLogin", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmpId", userId);
                    cmd.Parameters.AddWithValue("@Password", password);
                    
                    string flag = (loginType == "CHANNELPARTNER") ? "CPlogin" : "Login";
                    cmd.Parameters.AddWithValue("@Flag", flag);
                    cmd.Parameters.AddWithValue("@Outval", "");

                    var ds = new DataSet();
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }

                    if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        var lastRow = ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1];
                        
                        string userType = lastRow.Table.Columns.Contains("UserType") && lastRow["UserType"] != DBNull.Value ? lastRow["UserType"].ToString() : "";
                        string role = lastRow.Table.Columns.Contains("Role") && lastRow["Role"] != DBNull.Value ? lastRow["Role"].ToString() : userType;
                        string designation = lastRow.Table.Columns.Contains("Designation") && lastRow["Designation"] != DBNull.Value ? lastRow["Designation"].ToString() : "";

                        return new AppUser
                        {
                            UserId       = lastRow.Table.Columns.Contains("UserId")   && lastRow["UserId"]   != DBNull.Value ? lastRow["UserId"].ToString()   : userId,
                            UserName     = lastRow.Table.Columns.Contains("UserName") && lastRow["UserName"] != DBNull.Value ? lastRow["UserName"].ToString() : userId,
                            Email        = lastRow.Table.Columns.Contains("EmailId")  && lastRow["EmailId"]  != DBNull.Value ? lastRow["EmailId"].ToString()  : "",
                            MobileNo     = lastRow.Table.Columns.Contains("MobileNo") && lastRow["MobileNo"] != DBNull.Value ? lastRow["MobileNo"].ToString() : "",
                            Password     = password,
                            Role         = role,
                            Designation  = designation,
                            DepartmentId = lastRow.Table.Columns.Contains("DepartmentID") && lastRow["DepartmentID"] != DBNull.Value ? lastRow["DepartmentID"].ToString() : (lastRow.Table.Columns.Contains("DeptId") && lastRow["DeptId"] != DBNull.Value ? lastRow["DeptId"].ToString() : ""),
                            Department   = lastRow.Table.Columns.Contains("Department") && lastRow["Department"] != DBNull.Value ? lastRow["Department"].ToString() : "",
                            RptEmpName   = lastRow.Table.Columns.Contains("RptEmpName") && lastRow["RptEmpName"] != DBNull.Value ? lastRow["RptEmpName"].ToString() : "",
                            ReprotingMailId = lastRow.Table.Columns.Contains("ReprotingMailId") && lastRow["ReprotingMailId"] != DBNull.Value ? lastRow["ReprotingMailId"].ToString() : "",
                            ReportingMobileNum = lastRow.Table.Columns.Contains("ReportingMobileNum") && lastRow["ReportingMobileNum"] != DBNull.Value ? lastRow["ReportingMobileNum"].ToString() : "",
                            IsActive     = true
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
    }
}

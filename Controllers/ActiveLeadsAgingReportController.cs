using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;
using VGN_CRM_CORE.Models;

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    [TrackPageVisit]
    public class ActiveLeadsAgingReportController : Controller
    {
        private readonly string _connMT;

        public ActiveLeadsAgingReportController(IConfiguration configuration)
        {
            // Use the project's connection-string helper for failover behavior
            _connMT = configuration.GetActiveConnectionString("ConnMT");
        }

        // GET: /ActiveLeadsAgingReport
        [HttpGet]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.UserName = user.UserName;
            ViewBag.UserId = user.UserId;
            ViewBag.UserType = user.LoginType ?? user.Role;

            return View("Index");
        }

        ///**************LOAD EXECUTIVES***************///
        // GET: /ActiveLeadsAgingReport/LoadExecutives
        [HttpGet]
        public IActionResult LoadExecutives([FromQuery] ActiveLeadsAgingModel objVal)
        {
            try
            {
                objVal ??= new ActiveLeadsAgingModel();

                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                {
                    var jerr = JsonConvert.SerializeObject(new { status = false, msg = "Session expired", data = new DataSet() });
                    return Content(jerr, "application/json");
                }

                var salesUserType = (user.Role ?? "").ToUpperInvariant();
                var loginType = (user.LoginType ?? "").ToUpperInvariant();
                bool isChannel = loginType.Contains("CHANNEL");

                // Determine stored-proc flag
                string setFlag;
                if (salesUserType.StartsWith("CP_") || salesUserType.Contains("CP_"))
                {
                    // Channel-partner variants
                    if (salesUserType.Contains("EXECUTIVE")) setFlag = "LoadCPExecuitves";
                    else if (salesUserType.Contains("TEAM_MANAGER")) setFlag = "LoadCPTeamManager";
                    else if (salesUserType.Contains("TEAMHEAD")) setFlag = "LoadCPTeamHead";
                    else setFlag = "LoadCPFullAccess";
                }
                else
                {
                    // Corporate variants
                    if (salesUserType.Contains("EXECUTIVE")) setFlag = "LoadExecuitves";
                    else if (salesUserType.Contains("TEAM_MANAGER")) setFlag = "LoadTeamManager";
                    else if (salesUserType.Contains("SALES_HEAD")) setFlag = "LoadTeamHead";
                    else setFlag = "LoadFullAccess";
                }

                // Prefer CBUserId if explicitly stored in raw session (legacy support), else use user.UserId
                var cbUserId = HttpContext.Session.GetString("CBUserId");
                var empId = isChannel ? (cbUserId ?? user.UserId) : user.UserId;

                using (var conn = new SqlConnection(_connMT))
                using (var cmd = new SqlCommand("Web_Executives", conn))
                {
                    conn.Open();
                    cmd.CommandTimeout = 500;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", setFlag);
                    cmd.Parameters.AddWithValue("@DesignFlag", isChannel ? "CP_EXECUTIVE" : "EXECUTIVE");
                    cmd.Parameters.AddWithValue("@EmpId", empId ?? (object)DBNull.Value);

                    var da = new SqlDataAdapter(cmd);
                    da.Fill(objVal.dsEXE);
                }

                bool has = objVal.dsEXE.Tables.Count > 0 && objVal.dsEXE.Tables[0].Rows.Count > 0;
                var json = JsonConvert.SerializeObject(new { status = has, msg = has ? "Success" : "False", data = objVal.dsEXE });
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadExecutives] {ex.Message}");
                var jerr = JsonConvert.SerializeObject(new { status = false, msg = ex.Message, data = new DataSet() });
                return Content(jerr, "application/json");
            }
        }

        // GET: /ActiveLeadsAgingReport/LoadTeamManager
        [HttpGet]
        public IActionResult LoadTeamManager([FromQuery] ActiveLeadsAgingModel objVal)
        {
            try
            {
                objVal ??= new ActiveLeadsAgingModel();

                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                {
                    var jerr = JsonConvert.SerializeObject(new { status = false, msg = "Session expired", data = new DataSet() });
                    return Content(jerr, "application/json");
                }

                var salesUserType = (user.Role ?? "").ToUpperInvariant();
                var loginType = (user.LoginType ?? "").ToUpperInvariant();
                bool isChannel = loginType.Contains("CHANNEL");

                string setFlag;
                if (salesUserType.StartsWith("CP_") || salesUserType.Contains("CP_"))
                {
                    if (salesUserType.Contains("TEAM_MANAGER")) setFlag = "LoadCPTeamManager";
                    else setFlag = "LoadCPFullAccess";
                }
                else
                {
                    if (salesUserType.Contains("TEAM_MANAGER")) setFlag = "LoadTeamManager";
                    else setFlag = "LoadFullAccess";
                }

                var cbUserId = HttpContext.Session.GetString("CBUserId");
                var empId = isChannel ? (cbUserId ?? user.UserId) : user.UserId;

                using (var conn = new SqlConnection(_connMT))
                using (var cmd = new SqlCommand("Web_Executives", conn))
                {
                    conn.Open();
                    cmd.CommandTimeout = 500;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", setFlag);
                    cmd.Parameters.AddWithValue("@DesignFlag", isChannel ? "CP_TEAM_MANAGER" : "TEAM_MANAGER");
                    cmd.Parameters.AddWithValue("@EmpId", empId ?? (object)DBNull.Value);

                    var da = new SqlDataAdapter(cmd);
                    da.Fill(objVal.dsTeam);
                }

                bool has = objVal.dsTeam.Tables.Count > 0 && objVal.dsTeam.Tables[0].Rows.Count > 0;
                var json = JsonConvert.SerializeObject(new { status = has, msg = has ? "Success" : "False", data = objVal.dsTeam });
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadTeamManager] {ex.Message}");
                var jerr = JsonConvert.SerializeObject(new { status = false, msg = ex.Message, data = new DataSet() });
                return Content(jerr, "application/json");
            }
        }

        // GET: /ActiveLeadsAgingReport/LoadTeamHead
        [HttpGet]
        public IActionResult LoadTeamHead([FromQuery] ActiveLeadsAgingModel objVal)
        {
            try
            {
                objVal ??= new ActiveLeadsAgingModel();

                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                {
                    var jerr = JsonConvert.SerializeObject(new { status = false, msg = "Session expired", data = new DataSet() });
                    return Content(jerr, "application/json");
                }

                var salesUserType = (user.Role ?? "").ToUpperInvariant();
                var loginType = (user.LoginType ?? "").ToUpperInvariant();
                bool isChannel = loginType.Contains("CHANNEL");

                string setFlag;
                if (salesUserType.StartsWith("CP_") || salesUserType.Contains("CP_"))
                {
                    if (salesUserType.Contains("TEAMHEAD")) setFlag = "LoadCPTeamHead";
                    else setFlag = "LoadCPFullAccess";
                }
                else
                {
                    if (salesUserType.Contains("SALES_HEAD")) setFlag = "LoadTeamHead";
                    else setFlag = "LoadFullAccess";
                }

                var cbUserId = HttpContext.Session.GetString("CBUserId");
                var empId = isChannel ? (cbUserId ?? user.UserId) : user.UserId;

                using (var conn = new SqlConnection(_connMT))
                using (var cmd = new SqlCommand("Web_Executives", conn))
                {
                    conn.Open();
                    cmd.CommandTimeout = 500;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", setFlag);
                    cmd.Parameters.AddWithValue("@DesignFlag", isChannel ? "CP_SALES_HEAD" : "SALES_HEAD");
                    cmd.Parameters.AddWithValue("@EmpId", empId ?? (object)DBNull.Value);

                    var da = new SqlDataAdapter(cmd);
                    da.Fill(objVal.dsAssistantManager);
                }

                bool has = objVal.dsAssistantManager.Tables.Count > 0 && objVal.dsAssistantManager.Tables[0].Rows.Count > 0;
                var json = JsonConvert.SerializeObject(new { status = has, msg = has ? "Success" : "False", data = objVal.dsAssistantManager });
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadTeamHead] {ex.Message}");
                var jerr = JsonConvert.SerializeObject(new { status = false, msg = ex.Message, data = new DataSet() });
                return Content(jerr, "application/json");
            }
        }




        // POST: /ActiveLeadsAgingReport/LoadEnquiries
        [HttpPost]
        public IActionResult LoadEnquiries([FromBody] ActiveLeadsAgingModel objVal)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Content(JsonConvert.SerializeObject(new { status = false, msg = "Session expired", data = new DataSet() }), "application/json");

                if (objVal == null)
                    return Content(JsonConvert.SerializeObject(new { status = false, msg = "Invalid request", data = new DataSet() }), "application/json");

                objVal.dsP ??= new DataSet();

                using (var conn = new SqlConnection(_connMT))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("Web_LoadNotUpdateLeedsReport", conn))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Determine a sales user-type string. Original legacy code relied on
                        // Session["SalesUserType"] values like "CP_EXECUTIVE". We use user.Role
                        // (or LoginType) as a best-effort mapping. Adjust mapping if needed.
                        var salesUserType = (user.Role ?? user.LoginType ?? "").ToUpperInvariant();

                        if (salesUserType == "CP_EXECUTIVE" || salesUserType == "CP_TEAM_MANAGER" || salesUserType == "CP_TEAMHEAD")
                        {
                            if (!string.IsNullOrEmpty(objVal.ExecutiveId) && objVal.ExecutiveId != "0")
                            {
                                cmd.Parameters.AddWithValue("Empid", objVal.ExecutiveId);
                                cmd.Parameters.AddWithValue("Flag", "CP_NOT_UPDATE");
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("Empid", objVal.ExecutiveId ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("TeamLeaderId", objVal.TeamHeadId ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("Flag", "NOT_UPDATE");
                            }
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("Empid", objVal.ExecutiveId ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("TeamLeaderId", objVal.TeamHeadId ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("Flag", "NOT_UPDATE");
                        }

                        cmd.Parameters.AddWithValue("ProcessType", objVal.Leadtype ?? (object)DBNull.Value);

                        var da = new SqlDataAdapter(cmd);
                        da.Fill(objVal.dsP);
                    }
                }

                bool hasData = objVal.dsP.Tables.Count > 0 && objVal.dsP.Tables[0].Rows.Count > 0;
                return Content(JsonConvert.SerializeObject(new
                {
                    status = hasData,
                    msg = hasData ? "Success" : "No records",
                    data = objVal.dsP
                }), "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ActiveLeadsAgingReport.LoadEnquiries] {ex.Message}");
                return Content(JsonConvert.SerializeObject(new
                {
                    status = false,
                    msg = "Error: " + ex.Message,
                    data = new DataSet()
                }), "application/json");
            }
        }

        // POST: /ActiveLeadsAgingReport/LoadManualLeads
        [HttpPost]
        public IActionResult LoadManualLeads([FromBody] ActiveLeadsAgingModel objVal)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Content(JsonConvert.SerializeObject(new { status = false, msg = "Session expired", data = new DataSet() }), "application/json");

                objVal.dsP ??= new DataSet();

                using (var conn = new SqlConnection(_connMT))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("Web_LoadNotUpdateLeedsReport", conn))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("Empid", objVal.ExecutiveId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("Flag", "ManualLeads");

                        var da = new SqlDataAdapter(cmd);
                        da.Fill(objVal.dsP);
                    }
                }

                bool hasData = objVal.dsP.Tables.Count > 0 && objVal.dsP.Tables[0].Rows.Count > 0;
                return Content(JsonConvert.SerializeObject(new
                {
                    status = hasData,
                    msg = hasData ? "Success" : "No records",
                    data = objVal.dsP
                }), "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ActiveLeadsAgingReport.LoadManualLeads] {ex.Message}");
                return Content(JsonConvert.SerializeObject(new
                {
                    status = false,
                    msg = "Error: " + ex.Message,
                    data = new DataSet()
                }), "application/json");
            }
        }
    }
}
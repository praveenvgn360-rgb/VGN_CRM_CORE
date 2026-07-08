using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
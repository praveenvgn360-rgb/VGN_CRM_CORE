using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;
using VGN_CRM_CORE.Models; // Note: assumes SourceAnalysisModel exists in VGN_CRM_CORE.Models

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    [TrackPageVisit]
    public class SalesDSRReportController : Controller
    {
        private readonly string _connMT;

        public SalesDSRReportController(IConfiguration configuration)
        {
            _connMT = configuration.GetActiveConnectionString("ConnMT");
        }

        [HttpGet]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = user.UserName;
            ViewBag.UserId = user.UserId;
            ViewBag.UserType = user.LoginType ?? user.Role;

            return View("Index");
        }

        [HttpPost]
        public IActionResult LoadLeads([FromBody] SourceAnalysisModel obj_data)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                {
                    return Content(JsonConvert.SerializeObject(new { status = false, msg = "Session expired", data = new DataSet() }), "application/json");
                }

                if (obj_data == null)
                    return Content(JsonConvert.SerializeObject(new { status = false, msg = "Invalid request", data = new DataSet() }), "application/json");

                obj_data.UserType = user.LoginType ?? user.Role;
                obj_data.dsGrd ??= new DataSet();
                obj_data.dsGrd.Clear();

                DateTime fromDate, toDate;

                if (!DateTime.TryParseExact(obj_data.FromDate, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate))
                {
                    return Content(JsonConvert.SerializeObject(new { status = false, msg = "Invalid From Date format. Expected dd/MM/yyyy HH:mm:ss" }), "application/json");
                }

                if (!DateTime.TryParseExact(obj_data.ToDate, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate))
                {
                    return Content(JsonConvert.SerializeObject(new { status = false, msg = "Invalid To Date format. Expected dd/MM/yyyy HH:mm:ss" }), "application/json");
                }

                using (SqlConnection connMT = new SqlConnection(_connMT))
                {
                    connMT.Open();
                    using (SqlCommand cmd = new SqlCommand("Web_Load_DSRReport", connMT))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;

                        string flag = obj_data.WithTime ? "SALES_WITHTIME" : "SALES";
                        cmd.Parameters.Add("@Flag", SqlDbType.VarChar).Value = flag;
                        cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = fromDate;
                        cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = toDate;
                        cmd.Parameters.Add("@TeamId", SqlDbType.VarChar).Value = string.IsNullOrEmpty(obj_data.TeamId) ? (object)DBNull.Value : obj_data.TeamId;
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(obj_data.dsGrd);
                    }
                }

                bool hasData = obj_data.dsGrd.Tables.Count > 0 && obj_data.dsGrd.Tables[0].Rows.Count > 0;
                var json = JsonConvert.SerializeObject(new
                {
                    status = hasData,
                    msg = hasData ? "Success" : "False",
                    data = obj_data.dsGrd
                });

                return Content(json, "application/json");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesDSRReport.LoadLeads] {e.Message}");
                var jerr = JsonConvert.SerializeObject(new { status = false, msg = "Error: " + e.Message, data = new DataSet() });
                return Content(jerr, "application/json");
            }
        }

        [HttpPost]
        public IActionResult LoadEachCell_StageWise([FromBody] SourceAnalysisModel obj_data)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                {
                    return Content(JsonConvert.SerializeObject(new { status = false, msg = "Session expired", data = new DataSet() }), "application/json");
                }

                if (obj_data == null)
                    return Content(JsonConvert.SerializeObject(new { status = false, msg = "Invalid request", data = new DataSet() }), "application/json");

                obj_data.UserType = user.LoginType ?? user.Role;
                string SetModeList = "", SetTaskId = "", SetFlag = "", SetUpdateFlag = "";

                if (obj_data.Mode == "OFFLINE") SetModeList = "OFFLINE,OTHERS";
                else if (obj_data.Mode == "ONLINE") SetModeList = "ONLINE";
                else if (obj_data.Mode == "CP") SetModeList = "CHANNEL PARTNER";

                if (obj_data.Stage_one == "ENQUIRY_STAGE" && obj_data.Stage_two == "NEW_ENQUIRY")
                {
                    SetTaskId = "TaskID-68";
                    SetFlag = "SALES_NEWENQUIRY";
                }
                else if (obj_data.Stage_one == "ENQUIRY_STAGE" && obj_data.Stage_two == "EXISTING_ENQUIRY")
                {
                    SetTaskId = "TaskID-68";
                    SetFlag = "SALES_EXITSENQUIRY";
                }
                else if (obj_data.Stage_one == "SITE_VISIT")
                {
                    SetTaskId = "TaskID-69";
                    SetFlag = "SITE_VISIT";

                    if (obj_data.Mode == "FOLLOW UP") SetUpdateFlag = "IN";
                    else if (obj_data.Mode == "COMPLETED") SetUpdateFlag = "COM";
                    else if (obj_data.Mode == "CANCELLED") SetUpdateFlag = "CAN";
                }
                else if (obj_data.Stage_one == "NEGOTIATION")
                {
                    SetTaskId = "TaskID-70";
                    SetFlag = "NEGOTIATION";

                    if (obj_data.Mode == "FOLLOW UP") SetUpdateFlag = "IN";
                    else if (obj_data.Mode == "COMPLETED") SetUpdateFlag = "COM";
                    else if (obj_data.Mode == "CANCELLED") SetUpdateFlag = "CAN";
                }
                else if (obj_data.Stage_one == "BOOKING_STAGE")
                {
                    SetTaskId = "TaskID-71";
                    SetFlag = "BOOKING_STAGE";

                    if (obj_data.Mode == "FOLLOW UP") SetUpdateFlag = "IN";
                    else if (obj_data.Mode == "COMPLETED") SetUpdateFlag = "COM";
                    else if (obj_data.Mode == "CANCELLED") SetUpdateFlag = "CAN";
                }

                obj_data.dsGrd ??= new DataSet();
                obj_data.dsGrd.Clear();

                DateTime getDate;
                string formattedDate = "";

                if (string.IsNullOrEmpty(obj_data.GetDateString))
                {
                    return Content(JsonConvert.SerializeObject(new { status = false, msg = "GetDate is required" }), "application/json");
                }

                if (DateTime.TryParseExact(obj_data.GetDateString, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out getDate))
                {
                    formattedDate = getDate.ToString("dd/MM/yyyy HH:mm:ss");
                }
                else
                {
                    return Content(JsonConvert.SerializeObject(new { status = false, msg = "Invalid Date format. Expected dd/MM/yyyy HH:mm:ss" }), "application/json");
                }

                string spName = obj_data.WithTime ? "Web_Load_DSR_WithTime_HyperLinkReport" : "Web_Load_DSR_HyperLinkReport";

                using (SqlConnection connMT = new SqlConnection(_connMT))
                {
                    connMT.Open();
                    using (SqlCommand cmd = new SqlCommand(spName, connMT))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@Flag", SqlDbType.VarChar).Value = SetFlag;
                        cmd.Parameters.Add("@GetDate", SqlDbType.VarChar).Value = formattedDate;
                        cmd.Parameters.Add("@EmpId", SqlDbType.VarChar).Value = obj_data.EmpId;
                        cmd.Parameters.Add("@TaskId", SqlDbType.VarChar).Value = SetTaskId;
                        cmd.Parameters.Add("@ModeList", SqlDbType.VarChar).Value = string.IsNullOrEmpty(SetModeList) ? (object)DBNull.Value : SetModeList;
                        cmd.Parameters.Add("@SetUpdateFlag", SqlDbType.VarChar).Value = string.IsNullOrEmpty(SetUpdateFlag) ? (object)DBNull.Value : SetUpdateFlag;

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(obj_data.dsGrd);
                    }
                }

                bool hasData = obj_data.dsGrd.Tables.Count > 0 && obj_data.dsGrd.Tables[0].Rows.Count > 0;
                var json = JsonConvert.SerializeObject(new
                {
                    status = hasData,
                    msg = hasData ? "Success" : "False",
                    data = obj_data.dsGrd
                });

                return Content(json, "application/json");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesDSRReport.LoadEachCell_StageWise] {e.Message}");
                var jerr = JsonConvert.SerializeObject(new { status = false, msg = "Error: " + e.Message, data = new DataSet() });
                return Content(jerr, "application/json");
            }
        }

        [HttpPost]
        public IActionResult LoadTeamData()
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                {
                    return Content(JsonConvert.SerializeObject(new { status = false, msg = "Session expired", data = new DataSet() }), "application/json");
                }

                DataSet dsP = new DataSet();
                using (SqlConnection connMT = new SqlConnection(_connMT))
                {
                    connMT.Open();
                    using (SqlCommand cmd1 = new SqlCommand("Web_Executives", connMT))
                    {
                        cmd1.CommandTimeout = 500;
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.Add("@Flag", SqlDbType.VarChar).Value = "TEAM";
                        SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                        da1.Fill(dsP);
                    }
                }

                bool hasData = dsP.Tables.Count > 0 && dsP.Tables[0].Rows.Count > 0;
                var json = JsonConvert.SerializeObject(new
                {
                    status = hasData,
                    msg = hasData ? "Success" : "False",
                    data = dsP
                });

                return Content(json, "application/json");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesDSRReport.LoadTeamData] {e.Message}");
                var jerr = JsonConvert.SerializeObject(new { status = false, msg = "Error: " + e.Message, data = new DataSet() });
                return Content(jerr, "application/json");
            }
        }
    }
}
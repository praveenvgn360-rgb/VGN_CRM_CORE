using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    public class ManagementSalesDashboardController : Controller
    {
        private readonly string _connMT;
        private readonly string _connDB;

        public ManagementSalesDashboardController(IConfiguration config)
        {
            _connMT = config.GetActiveConnectionString("ConnMT");
            _connDB = config.GetActiveConnectionString("ConnDB");
        }

        // ──────────────────────────────────────────────────────
        // GET: /ManagementSalesDashboard/Index
        // ──────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.CurrentMonth = DateTime.Now.ToString("yyyy-MM");
            return View();
        }

        // ──────────────────────────────────────────────────────
        // POST: /ManagementSalesDashboard/LoadEnquiryCount
        // Returns lead count KPI data (Total, Online, Offline, Channel, Other)
        // ──────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult LoadEnquiryCount([FromBody] SalesDashboardRequest req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired" });

                var month = req?.Month ?? DateTime.Now.ToString("yyyy-MM");
                if (!DateTime.TryParse(month + "-01", out DateTime parsedMonth))
                    parsedMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                var startDate = new DateTime(parsedMonth.Year, parsedMonth.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var dt = new DataTable();
                using (var con = new SqlConnection(_connMT))
                using (var cmd = new SqlCommand("Web_SalesDashBoard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@Flag", "LeadCount_Status");
                    con.Open();
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    var data = new
                    {
                        Total    = GetInt(row, "Total"),
                        Online   = GetInt(row, "Online"),
                        Offline  = GetInt(row, "Offline"),
                        Channel  = GetInt(row, "Channel"),
                        Others   = GetInt(row, "Others"),
                        Month    = parsedMonth.ToString("MMMM yyyy")
                    };
                    return Json(new { status = true, msg = "Success", data });
                }
                return Json(new { status = false, msg = "No data" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MgmtSalesDash LoadEnquiryCount] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────
        // POST: /ManagementSalesDashboard/LoadProjectwiseBookedSummary
        // Project-wise target vs achieved grid + chart data
        // ──────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult LoadProjectwiseBookedSummary([FromBody] SalesDashboardRequest req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired" });

                var month = req?.Month ?? DateTime.Now.ToString("yyyy-MM");
                if (!DateTime.TryParse(month + "-01", out DateTime parsedMonth))
                    parsedMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                var startDate = new DateTime(parsedMonth.Year, parsedMonth.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var dt = new DataTable();
                using (var con = new SqlConnection(_connMT))
                using (var cmd = new SqlCommand("Web_SalesDashBoard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@Flag", "Projectwise_Summary");
                    con.Open();
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                if (dt.Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new { status = true, msg = "Success", data = dt });
                    return Content(json, "application/json");
                }
                return Json(new { status = false, msg = "No data" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MgmtSalesDash LoadProjectwiseBookedSummary] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────
        // POST: /ManagementSalesDashboard/ExecutiveWiseBookingSummary
        // Executive-wise target vs achieved
        // ──────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult ExecutiveWiseBookingSummary([FromBody] SalesDashboardRequest req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired" });

                var month = req?.Month ?? DateTime.Now.ToString("yyyy-MM");
                if (!DateTime.TryParse(month + "-01", out DateTime parsedMonth))
                    parsedMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                var startDate = new DateTime(parsedMonth.Year, parsedMonth.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var dt = new DataTable();
                using (var con = new SqlConnection(_connMT))
                using (var cmd = new SqlCommand("Web_SalesDashBoard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.AddWithValue("@Month", parsedMonth.ToString("MMM-yyyy"));
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@Flag", "Executivewise_Summary");
                    con.Open();
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                if (dt.Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new { status = true, msg = "Success", data = dt });
                    return Content(json, "application/json");
                }
                return Json(new { status = false, msg = "No data" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MgmtSalesDash ExecutiveWiseBookingSummary] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────
        // POST: /ManagementSalesDashboard/TeamWiseAbstract
        // Team-wise booking abstract
        // ──────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult TeamWiseAbstract([FromBody] SalesDashboardRequest req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired" });

                var month = req?.Month ?? DateTime.Now.ToString("yyyy-MM");
                if (!DateTime.TryParse(month + "-01", out DateTime parsedMonth))
                    parsedMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                var startDate = new DateTime(parsedMonth.Year, parsedMonth.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var dt = new DataTable();
                using (var con = new SqlConnection(_connMT))
                using (var cmd = new SqlCommand("Web_SalesDashBoard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.AddWithValue("@Month", parsedMonth.ToString("MMM-yyyy"));
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@Flag", "Team_Booking");
                    con.Open();
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                if (dt.Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new { status = true, msg = "Success", data = dt });
                    return Content(json, "application/json");
                }
                return Json(new { status = false, msg = "No data" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MgmtSalesDash TeamWiseAbstract] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────
        // POST: /ManagementSalesDashboard/LoadExpectedProspect
        // Site-visit prospect data (executive/project/team-wise)
        // ──────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult LoadExpectedProspect([FromBody] SalesDashboardDateRangeRequest req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired" });

                if (!DateTime.TryParse(req?.FromDate, out DateTime fromDate))
                    fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                if (!DateTime.TryParse(req?.ToDate, out DateTime toDate))
                    toDate = fromDate.AddMonths(1).AddDays(-1);

                var spFlag = req?.ViewType?.ToUpper() switch
                {
                    "PROJECTWISE" => "SV_Projectwise",
                    "TEAMWISE"    => "SV_Teamwise",
                    _             => "SV_Executive"
                };

                var dt = new DataTable();
                using (var con = new SqlConnection(_connMT))
                using (var cmd = new SqlCommand("Web_SalesMISDashBoard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.AddWithValue("@StartDate", fromDate);
                    cmd.Parameters.AddWithValue("@EndDate", toDate);
                    cmd.Parameters.AddWithValue("@Flag", spFlag);
                    con.Open();
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                if (dt.Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new { status = true, msg = "Success", data = dt });
                    return Content(json, "application/json");
                }
                return Json(new { status = false, msg = "No data" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MgmtSalesDash LoadExpectedProspect] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────
        // POST: /ManagementSalesDashboard/LoadBookedData
        // Booked data grid
        // ──────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult LoadBookedData([FromBody] SalesDashboardDateRangeRequest req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired" });

                if (!DateTime.TryParse(req?.FromDate, out DateTime fromDate))
                    fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                if (!DateTime.TryParse(req?.ToDate, out DateTime toDate))
                    toDate = fromDate.AddMonths(1).AddDays(-1);

                var dt = new DataTable();
                using (var con = new SqlConnection(_connMT))
                using (var cmd = new SqlCommand("Web_SalesMISDashBoard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.AddWithValue("@StartDate", fromDate);
                    cmd.Parameters.AddWithValue("@EndDate", toDate);
                    cmd.Parameters.AddWithValue("@Flag", "Booked_Data");
                    con.Open();
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                if (dt.Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new { status = true, msg = "Success", data = dt });
                    return Content(json, "application/json");
                }
                return Json(new { status = false, msg = "No data" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MgmtSalesDash LoadBookedData] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────
        // POST: /ManagementSalesDashboard/LoadStockData
        // Project stock summary
        // ──────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult LoadStockData()
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired" });

                var dt = new DataTable();
                using (var con = new SqlConnection(_connDB))
                using (var cmd = new SqlCommand("Web_CRMDashboard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.AddWithValue("@Flag", "Projectwise_Stock");
                    con.Open();
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                if (dt.Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new { status = true, msg = "Success", data = dt });
                    return Content(json, "application/json");
                }
                return Json(new { status = false, msg = "No data" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MgmtSalesDash LoadStockData] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        // ── Helpers ──────────────────────────────────────────
        private static int GetInt(DataRow row, string col)
        {
            try { return row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToInt32(row[col]) : 0; }
            catch { return 0; }
        }
    }

    // ── Request Models ────────────────────────────────────────
    public class SalesDashboardRequest
    {
        public string Month { get; set; }   // "yyyy-MM"
    }

    public class SalesDashboardDateRangeRequest
    {
        public string FromDate { get; set; }
        public string ToDate   { get; set; }
        public string ViewType { get; set; }  // "EXECUTIVEWISE" | "PROJECTWISE" | "TEAMWISE"
    }
}

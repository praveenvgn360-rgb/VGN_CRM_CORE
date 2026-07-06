using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;
using VGN_CRM_CORE.Models;

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    public class MarketingDashboard_NewController : Controller
    {
        private readonly string _connMT;

        public MarketingDashboard_NewController(IConfiguration config)
        {
            _connMT = config.GetActiveConnectionString("ConnMT");
        }

        public IActionResult Index() => View();

        // ─────────────────────────────────────────────────────────────
        // Helper: returns DBNull if string is empty, else the string
        // ─────────────────────────────────────────────────────────────
        private static object DbVal(string s) =>
            string.IsNullOrWhiteSpace(s) ? (object)DBNull.Value : s;

        // ─────────────────────────────────────────────────────────────
        // GET: /MarketingDashboard_New/GetTopLeadCounts
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetTopLeadCounts(string fromDate = null, string toDate = null)
        {
            var data = new MarketingDashboard_NewModel();
            try
            {
                using (var con = new SqlConnection(_connMT))
                using (var cmd = Sp(con, fromDate, toDate, "Top_Count"))
                {
                    con.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr["MonthStatus"].ToString() == "CUR_MONTH")
                            {
                                data = new MarketingDashboard_NewModel
                                {
                                    TOTAL_LEADS = ToInt(dr, "TOTAL_LEADS"),
                                    ONLINE = ToInt(dr, "ONLINE"),
                                    OFFLINE = ToInt(dr, "OFFLINE"),
                                    CHANNEL_PARTNER = ToInt(dr, "CHANNEL_PARTNER"),
                                    OTHERS = ToInt(dr, "OTHERS"),
                                    TOTAL_LEADS_DIFF = ToDec(dr, "TOTAL_LEADS_DIFF"),
                                    ONLINE_DIFF = ToDec(dr, "ONLINE_DIFF"),
                                    OFFLINE_DIFF = ToDec(dr, "OFFLINE_DIFF"),
                                    CHANNEL_PARTNER_DIFF = ToDec(dr, "CHANNEL_PARTNER_DIFF"),
                                    OTHERS_DIFF = ToDec(dr, "OTHERS_DIFF")
                                };
                            }
                        }
                    }
                }
                return Json(data);
            }
            catch (Exception ex) { return Err(ex); }
        }

        // ─────────────────────────────────────────────────────────────
        // GET: /MarketingDashboard_New/GetLeadBookedPerformance
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetLeadBookedPerformance(string fromDate = null, string toDate = null)
        {
            var list = new List<MarketingDashboard_NewModel>();
            try
            {
                using (var con = new SqlConnection(_connMT))
                using (var cmd = Sp(con, fromDate, toDate, "Lead_Booked_Performanace"))
                {
                    con.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new MarketingDashboard_NewModel
                            {
                                EnqMonth = dr["EnqMonth"].ToString(),
                                TotLead = ToInt(dr, "TotLead"),
                                BookNOS = ToInt(dr, "BookNOS")
                            });
                        }
                    }
                }
                return Json(list);
            }
            catch (Exception ex) { return Err(ex); }
        }

        // ─────────────────────────────────────────────────────────────
        // GET: /MarketingDashboard_New/GetLeadSourceBreakdown
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetLeadSourceBreakdown(string fromDate = null, string toDate = null)
        {
            var list = new List<MarketingDashboard_NewModel>();
            try
            {
                using (var con = new SqlConnection(_connMT))
                using (var cmd = Sp(con, fromDate, toDate, "Source_Booking_Breakdown"))
                {
                    con.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new MarketingDashboard_NewModel
                            {
                                SourceHeadEnquiry = dr["SourceHeadEnquiry"].ToString(),
                                BKNOS = ToInt(dr, "BKNOS"),
                                Percentage = ToDec(dr, "Percentage")
                            });
                        }
                    }
                }
                return Json(list);
            }
            catch (Exception ex) { return Err(ex); }
        }

        // ─────────────────────────────────────────────────────────────
        // GET: /MarketingDashboard_New/GetChannelPerformance
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetChannelPerformance(string fromDate = null, string toDate = null)
        {
            var list = new List<MarketingDashboard_NewModel>();
            try
            {
                using (var con = new SqlConnection(_connMT))
                using (var cmd = Sp(con, fromDate, toDate, "Source_Performanace"))
                {
                    con.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new MarketingDashboard_NewModel
                            {
                                SourceHeadEnquiry = dr["SourceHeadEnquiry"].ToString(),
                                NOS = ToInt(dr, "NOS"),
                                Percentage = ToDec(dr, "Percentage")
                            });
                        }
                    }
                }
                return Json(list);
            }
            catch (Exception ex) { return Err(ex); }
        }

        // ─────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────
        private static SqlCommand Sp(SqlConnection con, string from, string to, string flag)
        {
            var cmd = new SqlCommand("Web_MarketingDashBoardLatest", con)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };
            cmd.Parameters.AddWithValue("@EmpId", DBNull.Value);
            cmd.Parameters.AddWithValue("@Month", DBNull.Value);
            cmd.Parameters.AddWithValue("@FromDate", DbVal(from));
            cmd.Parameters.AddWithValue("@ToDate", DbVal(to));
            cmd.Parameters.AddWithValue("@Flag", flag);
            return cmd;
        }

        private static int ToInt(SqlDataReader dr, string col) => Convert.ToInt32(dr[col] == DBNull.Value ? 0 : dr[col]);
        private static decimal ToDec(SqlDataReader dr, string col) => Convert.ToDecimal(dr[col] == DBNull.Value ? 0 : dr[col]);
        private IActionResult Err(Exception ex) => Json(new { error = ex.Message });
    }
}

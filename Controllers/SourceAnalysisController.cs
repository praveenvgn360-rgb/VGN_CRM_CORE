using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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
    public class SourceAnalysisController : Controller
    {
        private readonly string _connMT;
        private readonly string _connDB;

        public SourceAnalysisController(IConfiguration config)
        {
            _connMT = config.GetActiveConnectionString("ConnMT");
            _connDB = config.GetActiveConnectionString("ConnDB");
        }

        private string GetMappedUserType(string roleType)
        {
            if (string.IsNullOrEmpty(roleType)) return "VIEW";
            roleType = roleType.Trim().ToUpper();

            if (roleType == "EXECUITVE" || roleType == "EXECUTIVE")
                return "EXECUTIVE";
            else if (roleType == "TEAM_MANAGER")
                return "TEAM_MANAGER";
            else if (roleType == "SALES_HEAD")
                return "SALES_HEAD";
            else if (roleType == "PRESALES_EXECUTIVE" || roleType == "TELECALLER_EXECUTIVE")
                return "PRESALES_EXECUTIVE";
            else if (roleType == "PRESALES_HEAD" || roleType == "TELECALLER_HEAD")
                return "PRESALES_HEAD";
            else
                return "VIEW"; // full access / default
        }

        // ──────────────────────────────────────────────────────
        // GET: /SourceAnalysis/Index
        // ──────────────────────────────────────────────────────
        [HttpGet]
        [Route("SourceAnalysis")]
        [Route("SourceAnalysis/Index")]
        [Route("SourceAnalysis/SourceAnalysis")]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.UserName     = user.UserName;
            ViewBag.UserId       = user.UserId;
            ViewBag.DepartmentId = user.DepartmentId;
            ViewBag.UserType     = user.LoginType;

            return View("Index");
        }


        // ──────────────────────────────────────────────────────
        // GET: /SourceAnalysis/LoadProjectName
        // Returns project list for filter dropdown
        // ──────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult LoadProjectName()
        {
            try
            {
                var ds = new DataSet();
                using (var con = new SqlConnection(_connMT))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("Web_LoadSourceofEnquiryLeedsReport", con))
                    {
                        cmd.CommandType    = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 120;
                        cmd.Parameters.AddWithValue("@Flag", "PROJECT");
                        new SqlDataAdapter(cmd).Fill(ds);
                    }
                }

                bool found = ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0;
                return Content(JsonConvert.SerializeObject(new
                {
                    status = found,
                    msg    = found ? "Success" : "No data",
                    data   = ds
                }), "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SourceAnalysis.LoadProjectName] {ex.Message}");
                return Content(JsonConvert.SerializeObject(new
                {
                    status = false,
                    msg    = ex.Message,
                    data   = new DataSet()
                }), "application/json");
            }
        }

        // ──────────────────────────────────────────────────────
        // POST: /SourceAnalysis/LoadLeads
        // Returns source analysis lead data (Last Remarks mode)
        // ──────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult LoadLeads([FromBody] SourceAnalysisModel objVal)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired." });

                if (objVal == null)
                    return Json(new { status = false, msg = "Invalid request." });

                var ds = new DataSet();

                // Build comma-separated project IDs.
                // The SP uses "0" as the sentinel for "All Projects" — pass it through as-is.
                string projectIds = string.Empty;
                if (objVal.ProjectID != null && objVal.ProjectID.Count > 0)
                {
                    projectIds = string.Join(",", objVal.ProjectID);
                }

                using (var con = new SqlConnection(_connMT))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("Web_LoadSourceofEnquiryLeedsReport", con))
                    {
                        cmd.CommandType    = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 500;
                        string rawRole = string.IsNullOrEmpty(objVal.UserType) ? (user.Role ?? "") : objVal.UserType;
                        
                        cmd.Parameters.AddWithValue("@Flag",      GetMappedUserType(rawRole));
                        cmd.Parameters.AddWithValue("@ProjectId", projectIds);
                        cmd.Parameters.AddWithValue("@EmpId",     user.UserId ?? "");
                        cmd.Parameters.AddWithValue("@FromDate",  Convert.ToDateTime(objVal.FromDate));
                        cmd.Parameters.AddWithValue("@ToDate",    Convert.ToDateTime(objVal.ToDate));
                        cmd.Parameters.AddWithValue("@Month",     Convert.ToDateTime(objVal.FromDate));
                        
                        new SqlDataAdapter(cmd).Fill(ds);
                    }
                }

                bool found = ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0;
                return Content(JsonConvert.SerializeObject(new
                {
                    status = found,
                    msg    = found ? "Success" : "No Records Found",
                    data   = ds
                }), "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SourceAnalysis.LoadLeads] {ex.Message}");
                return Content(JsonConvert.SerializeObject(new
                {
                    status = false,
                    msg    = "Error: " + ex.Message,
                    data   = new DataSet()
                }), "application/json");
            }
        }

        // ──────────────────────────────────────────────────────
        // POST: /SourceAnalysis/LoadLeadsFullRemarks
        // Returns full remarks data
        // ──────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult LoadLeadsFullRemarks([FromBody] SourceAnalysisModel objVal)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired." });

                if (objVal == null)
                    return Json(new { status = false, msg = "Invalid request." });

                var ds = new DataSet();

                using (var con = new SqlConnection(_connMT))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("Web_LoadSourceofEnquiryLeadsFullRemarksReport", con))
                    {
                        cmd.CommandType    = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 500;
                        
                        cmd.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(objVal.FromDate));
                        cmd.Parameters.AddWithValue("@ToDate",   Convert.ToDateTime(objVal.ToDate));
                        cmd.Parameters.AddWithValue("@Flag",     "FULL_REMAKS"); 
                        
                        new SqlDataAdapter(cmd).Fill(ds);
                    }
                }

                bool found = ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0;
                return Content(JsonConvert.SerializeObject(new
                {
                    status = found,
                    msg    = found ? "Success" : "No Records Found",
                    data   = ds
                }), "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SourceAnalysis.LoadLeadsFullRemarks] {ex.Message}");
                return Content(JsonConvert.SerializeObject(new
                {
                    status = false,
                    msg    = "Error: " + ex.Message,
                    data   = new DataSet()
                }), "application/json");
            }
        }


    }
}

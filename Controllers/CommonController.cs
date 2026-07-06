using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;

namespace VGN_CRM_CORE.Controllers
{

    [AuthorizeSession]
    [TrackPageVisit]
    public class CommonController : Controller
    {
        private readonly string _connMT;
        private readonly string _connDB;
        private readonly string _connHR;
        private readonly string _connLP;
        private readonly string _connMIS;

        public CommonController(IConfiguration config)
        {
            _connMT = config.GetActiveConnectionString("ConnMT");
            _connDB = config.GetActiveConnectionString("ConnDB");
            _connHR = config.GetActiveConnectionString("ConnHR");
            _connLP = config.GetActiveConnectionString("ConnLP");
            _connMIS = config.GetActiveConnectionString("ConnMIS");
        }




        public IActionResult Index()
        {
            return View();
        }




        [HttpGet]
        public IActionResult LoadDepartment()
        {
            try
            {
                var ds = new DataSet();

                using (var connMT = new SqlConnection(_connMT))
                {
                    connMT.Open();
                    using (var cmd = new SqlCommand("Web_OldCustomerProjectDetails", connMT))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Flag", "Department");

                        var da = new SqlDataAdapter(cmd);
                        da.Fill(ds);
                    }
                }

                bool found = ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0;

                var json = JsonConvert.SerializeObject(new
                {
                    status = found,
                    msg = found ? "Success" : "False",
                    data = ds
                });

                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[LoadDepartment] " + ex.Message);
                var json = JsonConvert.SerializeObject(new { status = false, msg = ex.Message, data = new DataSet() });
                return Content(json, "application/json");
            }
        }


        [HttpGet]
        public IActionResult FindDuplicateMobile(string Mobile1)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired. Please login again." });

                string userId = user.UserId;
                var dt = new DataTable();

                using (var connMT = new SqlConnection(_connMT))
                {
                    connMT.Open();

                    using (var cmd = new SqlCommand("Web_FindMobileEnquiry", connMT))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Flag", "View");
                        cmd.Parameters.AddWithValue("@mobileno", Mobile1 ?? "");

                        var da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        // Log skipped mobile number
                        using (var cmd = new SqlCommand(
                            "INSERT INTO Cp_SkippedMobileNoData (SrcEnqId, Mobile1, EntryDateTime, Status) VALUES (@SrcEnqId, @MobileNo, @EntryDateTime, @Status)",
                            connMT))
                        {
                            cmd.Parameters.AddWithValue("@EntryDateTime", DateTime.Now);
                            cmd.Parameters.AddWithValue("@MobileNo", Mobile1 ?? "");
                            cmd.Parameters.AddWithValue("@SrcEnqId", userId);
                            cmd.Parameters.AddWithValue("@Status", "1");
                            cmd.ExecuteNonQuery();
                        }

                        return Json(new { status = true, msg = "Success" });
                    }
                    else
                    {
                        return Json(new { status = false, msg = "False" });
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine("SQL Exception: " + sqlEx.Message);
                return Json(new { status = false, msg = "Database error: " + sqlEx.Message });
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("General Exception: " + e.Message);
                return Json(new { status = false, msg = "Error: " + e.Message });
            }
        }

        // ──────────────────────────────────────────────────────
        // GET: /ChannelPartnerLeads/LoadParentSource
        // ──────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult LoadParentSource()
        {
            try
            {
                var ds = new DataSet();

                using (var connMT = new SqlConnection(_connMT))
                {
                    connMT.Open();
                    using (var cmd = new SqlCommand("Web_SourceDetails", connMT))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Flag", "Parent_Source_CP");

                        var da = new SqlDataAdapter(cmd);
                        da.Fill(ds);
                    }
                }

                bool found = ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0;

                var json = JsonConvert.SerializeObject(new
                {
                    status = found,
                    msg = found ? "Success" : "False",
                    data = ds
                });

                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[LoadParentSource] " + ex.Message);
                var json = JsonConvert.SerializeObject(new { status = false, msg = ex.Message, data = new DataSet() });
                return Content(json, "application/json");
            }
        }

        // ──────────────────────────────────────────────────────
        // GET: /ChannelPartnerLeads/LoadSubSource
        // ──────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult LoadSubSource(string PSrcTranid)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return RedirectToAction("Index", "Login");

                var ds = new DataSet();

                using (var connMT = new SqlConnection(_connMT))
                {
                    connMT.Open();
                    using (var cmd = new SqlCommand("Web_SourceDetails", connMT))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ParentSourceId", PSrcTranid ?? "0");
                        cmd.Parameters.AddWithValue("@SubSource", user.UserId);
                        cmd.Parameters.AddWithValue("@Flag", "Sub_source_CP");

                        var da = new SqlDataAdapter(cmd);
                        da.Fill(ds);
                    }
                }

                bool found = ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0;

                var json = JsonConvert.SerializeObject(new
                {
                    status = found,
                    msg = found ? "Success" : "False",
                    data = ds
                });

                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[LoadSubSource] " + ex.Message);
                var json = JsonConvert.SerializeObject(new { status = false, msg = ex.Message, data = new DataSet() });
                return Content(json, "application/json");
            }
        }




        // ──────────────────────────────────────────────────────
        // GET/POST: /Common/Load_Emp_With_Department
        // ──────────────────────────────────────────────────────
       
        [HttpPost]
        public IActionResult Load_Emp_With_Department(string Flag = "Employee_Dept")
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired" });

                var ds = new DataSet();
                using (var con = new SqlConnection(_connMIS))
                using (var cmd = new SqlCommand("Web_LoadHodDaybookAssignments", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.AddWithValue("@Flag", string.IsNullOrEmpty(Flag) ? "Emp_With_Department" : Flag);
                    con.Open();
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                }

                bool hasData = ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0;
                var json = JsonConvert.SerializeObject(new
                {
                    status = hasData,
                    msg = hasData ? "Success" : "No data",
                    data = ds
                });
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Common Load_Emp_With_Department] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

    }
}

using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
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
    public class GeneralDashboardController : Controller
    {
        private readonly string _connMT;
        private readonly string _connDB;
        private readonly string _connHR;
        private readonly string _connLP;

        public GeneralDashboardController(IConfiguration config)
        {
            _connMT = config.GetActiveConnectionString("ConnMT");
            _connDB = config.GetActiveConnectionString("ConnDB");
            _connHR = config.GetActiveConnectionString("ConnHR");
            _connLP = config.GetActiveConnectionString("ConnLP");
        }

        // ──────────────────────────────────────────────────────
        // GET: /GeneralDashboard/Index
        // ──────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.UserName = user.UserName;
            ViewBag.UserId = user.UserId;
            ViewBag.UserRole = user.Role;
            ViewBag.Designation = !string.IsNullOrEmpty(user.Designation) ? user.Designation : user.Role;
            ViewBag.DepartmentId = user.DepartmentId;
            ViewBag.LoginType = user.LoginType;
            ViewBag.LoginTime = user.LoginTime.ToString("dd-MMM-yyyy hh:mm tt");
            ViewBag.IPAddress = user.IPAddress;

            // ── Load menus from DB and store in session ──────────
            try
            {
                LoadUserMenus(user.UserId, user.LoginType);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GeneralDashboard] Menu load error: {ex.Message}");
            }

            return View();
        }

        /// <summary>
        /// Loads menu items from Web_UserRights SP and stores in session.
        /// Handles both Corporate and Channel Partner login types.
        /// </summary>
        private void LoadUserMenus(string userId, string loginType)
        {
            // Skip if already loaded in this session
            var existingMenu = SessionHelper.GetMenuList(HttpContext.Session);
            if (existingMenu != null && existingMenu.Count > 0)
                return;

            var menuList = new System.Collections.Generic.List<Models.MenuModel>();

            if ((loginType ?? "").ToUpper() == "CHANNELPARTNER")
            {
                // Channel Partner manual menu (as per vgn360 pattern)
                menuList.Add(new Models.MenuModel
                {
                    Department = "CHANNEL PARTNER",
                    ModuleType = "REPORTS",
                    ModuleCaptionName = "Source Analysis",
                    ControllerName = "GeneralDashboard",
                    ActionName = "Index"
                });
            }
            else
            {
                // Corporate login: Load from DB based on rights
                var ds = new DataSet();
                string upperUserId = (userId ?? "").Trim().ToUpper();

                // Determine which flag to use for rights
                string flag;
                bool isAdmin = upperUserId == "TEST" || upperUserId == "E1810" ||
                               upperUserId == "MD" || upperUserId == "CMD" ||
                               upperUserId == "E1655" || upperUserId == "AUDIT" ||
                               upperUserId == "CEO" || upperUserId == "D001" ||
                               upperUserId == "E0015";

                if (isAdmin)
                    flag = "UserLoginRights";
                else
                    flag = "DesignUserRights";

                using (var con = new SqlConnection(_connDB))
                using (var cmd = new SqlCommand("Web_UserRights", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.AddWithValue("@EmpId", userId);
                    cmd.Parameters.AddWithValue("@Flag", flag);

                    var da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                }

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    menuList = ConvertDataTable<Models.MenuModel>(ds.Tables[0]);
                }
            }

            if (menuList.Count > 0)
            {
                SessionHelper.SetMenuList(HttpContext.Session, menuList);
            }
        }

        // ── Generic DataTable → List<T> converter ───────────────
        private static System.Collections.Generic.List<T> ConvertDataTable<T>(DataTable dt)
        {
            var data = new System.Collections.Generic.List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = System.Activator.CreateInstance<T>();
            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (var pro in temp.GetProperties())
                {
                    if (string.Equals(pro.Name, column.ColumnName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (dr[column.ColumnName] != DBNull.Value)
                            pro.SetValue(obj, dr[column.ColumnName]?.ToString(), null);
                        break;
                    }
                }
            }
            return obj;
        }

        // ──────────────────────────────────────────────────────
        // GET: /ChannelPartnerLeads/FindDuplicateMobile
        // ──────────────────────────────────────────────────────



        [HttpPost]
        public IActionResult LoadAttendanceCalendar([FromBody] AttendanceRequestModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired" });

                string empId = user.UserId;
                string month = string.IsNullOrWhiteSpace(req?.Month)
                                ? DateTime.Now.ToString("MMM-yyyy")
                                : req.Month;

                var dt = new DataTable();

                using (var con = new SqlConnection(_connDB))
                using (var cmd = new SqlCommand("Web_LoadDashBoardData", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@EmpId", empId);
                    cmd.Parameters.AddWithValue("@Month", month);
                    cmd.Parameters.AddWithValue("@Flag", "Login_Attendance");

                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                if (dt.Rows.Count == 0)
                    return Json(new { status = false, msg = "No data", data = new object[0] });

                var days = new System.Collections.Generic.List<object>();
                var row = dt.Rows[0];
                int totalDays = DateTime.DaysInMonth(
                                    DateTime.ParseExact(month, "MMM-yyyy",
                                    System.Globalization.CultureInfo.InvariantCulture).Year,
                                    DateTime.ParseExact(month, "MMM-yyyy",
                                    System.Globalization.CultureInfo.InvariantCulture).Month);

                for (int d = 1; d <= totalDays; d++)
                {
                    string dd = d.ToString("D2");
                    string inCol = dd + "In";
                    string outCol = dd + "Out";
                    string durCol = dd + "Duration";
                    string stsCol = dd + "AttendanceStatus";

                    string inTime = "";
                    string outTime = "";
                    string duration = "";
                    string attStatus = "";

                    foreach (DataColumn col in dt.Columns)
                    {
                        string cName = col.ColumnName.Trim();
                        if (string.Equals(cName, inCol, StringComparison.OrdinalIgnoreCase)) inTime = row[col]?.ToString()?.Trim();
                        else if (string.Equals(cName, outCol, StringComparison.OrdinalIgnoreCase)) outTime = row[col]?.ToString()?.Trim();
                        else if (string.Equals(cName, durCol, StringComparison.OrdinalIgnoreCase)) duration = row[col]?.ToString()?.Trim();
                        else if (string.Equals(cName, stsCol, StringComparison.OrdinalIgnoreCase)) attStatus = row[col]?.ToString()?.Trim();
                    }

                    if (!string.IsNullOrEmpty(inTime) && DateTime.TryParse(inTime, out var parsedIn))
                        inTime = parsedIn.ToString("hh:mm tt");

                    if (!string.IsNullOrEmpty(outTime) && DateTime.TryParse(outTime, out var parsedOut))
                        outTime = parsedOut.ToString("hh:mm tt");

                    days.Add(new { Day = d, DayStr = dd, InTime = inTime, OutTime = outTime, Duration = duration, Status = attStatus });
                }

                return Json(new { status = true, msg = "Success", month = month, data = days });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GeneralDashboard] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────
        // GET: /GeneralDashboard/LoadDaybookData
        // Returns daybook records for the logged-in user.
        // ──────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult LoadDaybookData()
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired", data = new object[0] });

                string empId = user.UserId;
                var dt = new DataTable();

                using (var con = new SqlConnection(_connDB))
                using (SqlCommand cmd = new SqlCommand("Web_AssignmentUpdationDashboard", con))
                {
                    cmd.CommandTimeout = 500;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmpId", empId);
                    cmd.Parameters.AddWithValue("@Flag", "Individual_General_Dashboard_Table");
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                var daybookList = new System.Collections.Generic.List<object>();

                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        daybookList.Add(new
                        {
                            Sno = i + 1,
                            TitleId = dt.Columns.Contains("TitleId") ? dt.Rows[i]["TitleId"]?.ToString() ?? "" : "",
                            Title = dt.Columns.Contains("Title") ? dt.Rows[i]["Title"]?.ToString() ?? "" : "",
                            Assignmenttype = dt.Columns.Contains("Assignmenttype") ? dt.Rows[i]["Assignmenttype"]?.ToString() ?? "" : "",
                            GeneralJobId = dt.Columns.Contains("GeneralJobId") ? dt.Rows[i]["GeneralJobId"]?.ToString() ?? "" : "",
                            TitleName = dt.Columns.Contains("TitleName") ? dt.Rows[i]["TitleName"]?.ToString() ?? "" : "",
                            NoofJobs = dt.Columns.Contains("NoofJobs") && dt.Rows[i]["NoofJobs"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["NoofJobs"]) : 0,
                            Followup = dt.Columns.Contains("Followup") && dt.Rows[i]["Followup"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["Followup"]) : 0,
                            Completed = dt.Columns.Contains("Completed") && dt.Rows[i]["Completed"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["Completed"]) : 0,
                            To_Start = dt.Columns.Contains("To_Start") && dt.Rows[i]["To_Start"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["To_Start"]) : 0
                        });
                    }
                }

                return Content(JsonConvert.SerializeObject(new { status = true, msg = "Success", data = daybookList }), "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GeneralDashboard LoadDaybookData] {ex.Message}");
                return Json(new { status = false, msg = ex.Message, data = new object[0] });
            }
        }

        // ──────────────────────────────────────────────────────
        // GET: /GeneralDashboard/LoadAssignmentExtensionData
        // Returns assignment extension records for the logged-in user.
        // ──────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult LoadAssignmentExtensionData()
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired", data = new object[0] });

                string empId = user.UserId;
                var dt = new DataTable();

                using (var con = new SqlConnection(_connDB))
                using (var cmd = new SqlCommand("Web_AssignmentUpdationDashboard", con))
                {
                    cmd.CommandTimeout = 500;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmpId", empId);
                    cmd.Parameters.AddWithValue("@Flag", "EmpJob_Extension_Dashboard");
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                var assignmentList = new System.Collections.Generic.List<object>();

                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        assignmentList.Add(new
                        {
                            Sno = i + 1,
                            TitleId = dt.Columns.Contains("TitleId") ? dt.Rows[i]["TitleId"]?.ToString() ?? "" : "",
                            JobType = dt.Columns.Contains("JobType") ? dt.Rows[i]["JobType"]?.ToString() ?? "" : "",
                            Title = dt.Columns.Contains("Title") ? dt.Rows[i]["Title"]?.ToString() ?? "" : "",
                            SubTitle = dt.Columns.Contains("SubTitle") ? dt.Rows[i]["SubTitle"]?.ToString() ?? "" : "",
                            EmpName = dt.Columns.Contains("EmpName") ? dt.Rows[i]["EmpName"]?.ToString() ?? "" : "",
                            EmpId = dt.Columns.Contains("EmpId") ? dt.Rows[i]["EmpId"]?.ToString() ?? "" : "",
                            StageId = dt.Columns.Contains("StageId") ? dt.Rows[i]["StageId"]?.ToString() ?? "" : "",
                            substageId = dt.Columns.Contains("substageId") ? dt.Rows[i]["substageId"]?.ToString() ?? "" : "",
                            JobId = dt.Columns.Contains("JobId") ? dt.Rows[i]["JobId"]?.ToString() ?? "" : "",
                            SubTitleId = dt.Columns.Contains("SubTitleId") ? dt.Rows[i]["SubTitleId"]?.ToString() ?? "" : "",
                            EmpJobId = dt.Columns.Contains("EmpJobId") ? dt.Rows[i]["EmpJobId"]?.ToString() ?? "" : "",
                            JobExtensionApproveId = dt.Columns.Contains("JobExtensionApproveId") ? dt.Rows[i]["JobExtensionApproveId"]?.ToString() ?? "" : "",
                            NoofExtension = dt.Columns.Contains("NoofExtension") && dt.Rows[i]["NoofExtension"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["NoofExtension"]) : 0,
                            Status = dt.Columns.Contains("Status") ? dt.Rows[i]["Status"]?.ToString() ?? "Pending" : "Pending"
                        });
                    }
                }

                return Content(JsonConvert.SerializeObject(new { status = true, msg = "Success", data = assignmentList }), "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GeneralDashboard LoadAssignmentExtensionData] {ex.Message}");
                return Json(new { status = false, msg = ex.Message, data = new object[0] });
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
        // POST: /ChannelPartnerLeads/SaveLeads
        // ──────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult SaveLeads([FromBody] ChannelPartnerLeadsModel Save_Leads)
        {
            if (Save_Leads == null)
                return Json(new { status = false, msg = "Invalid input data" });

            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired. Please login again." });

                // Validate required fields
                if (string.IsNullOrWhiteSpace(Save_Leads.projectId) || Save_Leads.projectId == "0")
                    return Json(new { status = false, msg = "Project is required" });

                if (string.IsNullOrWhiteSpace(Save_Leads.SrcTranid) || Save_Leads.SrcTranid == "0")
                    return Json(new { status = false, msg = "Source of enquiry is required" });

                if (string.IsNullOrWhiteSpace(Save_Leads.SubSrcEnquiryId) || Save_Leads.SubSrcEnquiryId == "0")
                    return Json(new { status = false, msg = "Sub source of enquiry is required - session might be expired, kindly relogin" });

                if (string.IsNullOrWhiteSpace(Save_Leads.ClientName))
                    return Json(new { status = false, msg = "Client name is required" });

                if (string.IsNullOrWhiteSpace(Save_Leads.Mobile1))
                    return Json(new { status = false, msg = "Mobile number is required" });

                string userName = user.UserName;
                string userId = user.UserId;

                using (var connMT = new SqlConnection(_connMT))
                {
                    connMT.Open();

                    // Check Mobile1 duplication
                    var dsFM = new DataTable();
                    using (var cmd = new SqlCommand("Web_FindMobileEnquiry", connMT))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@mobileno", Save_Leads.Mobile1);
                        cmd.Parameters.AddWithValue("@Flag", "View");
                        var da = new SqlDataAdapter(cmd);
                        da.Fill(dsFM);
                    }

                    if (dsFM.Rows.Count > 0)
                        return Json(new { status = false, msg = "Mobile No 1 already exists" });

                    // Check Mobile2 duplication if provided
                    if (!string.IsNullOrWhiteSpace(Save_Leads.Mobile2))
                    {
                        var dsFM2 = new DataTable();
                        using (var cmd = new SqlCommand("Web_FindMobileEnquiry", connMT))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@mobileno", Save_Leads.Mobile2);
                            cmd.Parameters.AddWithValue("@Flag", "View");
                            var da = new SqlDataAdapter(cmd);
                            da.Fill(dsFM2);
                        }

                        if (dsFM2.Rows.Count > 0)
                            return Json(new { status = false, msg = "Mobile No 2 already exists" });
                    }

                    // Save the lead
                    var sDate = DateTime.Now.ToString("dd/MM/yyyy");

                    using (var cmd = new SqlCommand("Web_VendorEnquiryDataSave", connMT))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@CustomerName", Save_Leads.ClientName.ToUpper());
                        cmd.Parameters.AddWithValue("@Mobile1", Save_Leads.Mobile1);
                        cmd.Parameters.AddWithValue("@Mobile2", (object)Save_Leads.Mobile2 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Category", (object)Save_Leads.Category ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ProjectId", Save_Leads.projectId);
                        cmd.Parameters.AddWithValue("@Psrctranid", Save_Leads.SrcTranid);
                        cmd.Parameters.AddWithValue("@SrcTranid", Save_Leads.SubSrcEnquiryId);
                        cmd.Parameters.AddWithValue("@Email", (object)Save_Leads.EmailId1 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PANNo", (object)Save_Leads.Panno ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AadharNo", (object)Save_Leads.AadharNo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Location", (object)Save_Leads.Location ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Remarks", (object)Save_Leads.Remarks ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AgentName", (object)userName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@TodayDate", sDate);
                        cmd.Parameters.AddWithValue("@Countrycode1", (object)Save_Leads.CountryCode1 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Countrycode2", (object)Save_Leads.CountryCode2 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Flag", "Save");

                        var outParam = new SqlParameter("@OutType", SqlDbType.Int);
                        outParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outParam);

                        cmd.ExecuteNonQuery();

                        int outType = outParam.Value != DBNull.Value ? Convert.ToInt32(outParam.Value) : 0;

                        switch (outType)
                        {
                            case 1:
                                return Json(new { status = true, msg = "Success" });
                            case 2:
                                return Json(new { status = false, msg = "Mobile number already exists" });
                            case 3:
                                return Json(new { status = false, msg = "Duplicate entry found" });
                            default:
                                return Json(new { status = true, msg = "Success" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[SaveLeads] " + ex.Message);
                return Json(new { status = false, msg = "Internal Server Error: " + ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────
        // POST/GET: /ChannelPartnerLeads/SearchMobile
        // ──────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult SearchMobile(string SearchMobile)
        {
            try
            {
                var ds = new DataSet();

                using (var connMT = new SqlConnection(_connMT))
                {
                    connMT.Open();
                    using (var cmd = new SqlCommand("SearchingDataEQandTC", connMT))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@mobileno", SearchMobile ?? "");

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
                System.Diagnostics.Debug.WriteLine("[SearchMobile] " + ex.Message);
                var json = JsonConvert.SerializeObject(new { status = false, msg = ex.Message, data = new DataSet() });
                return Content(json, "application/json");
            }
        }

        // ──────────────────────────────────────────────────────
        // GET: /ChannelPartnerLeads/LoadProject
        // ──────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult LoadProject(string Category)
        {
            try
            {
                var ds = new DataSet();

                using (var connMT = new SqlConnection(_connMT))
                {
                    connMT.Open();
                    using (var cmd = new SqlCommand("Web_ProjectDetails", connMT))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;

                        string flag;
                        switch (Category)
                        {
                            case "VILLA": flag = "Available_ProjectVILLA"; break;
                            case "PLOT": flag = "Available_ProjectPLOT"; break;
                            default: flag = "Available_ProjectFLAT"; break;
                        }

                        cmd.Parameters.AddWithValue("@Flag", flag);

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
                System.Diagnostics.Debug.WriteLine("[LoadProject] " + ex.Message);
                var json = JsonConvert.SerializeObject(new { status = false, msg = ex.Message, data = new DataSet() });
                return Content(json, "application/json");
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
        // GET: /ChannelPartnerLeads/LoadAutoId
        // ──────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult LoadAutoId()
        {
            try
            {
                var ds = new DataSet();

                using (var connMT = new SqlConnection(_connMT))
                {
                    connMT.Open();
                    using (var cmd = new SqlCommand("Web_VendorEnquiryDataSave", connMT))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Flag", "AutoId");

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
                System.Diagnostics.Debug.WriteLine("[LoadAutoId] " + ex.Message);
                var json = JsonConvert.SerializeObject(new { status = false, msg = ex.Message, data = new DataSet() });
                return Content(json, "application/json");
            }
        }

        // ──────────────────────────────────────────────────────
        // POST: /ChannelPartnerLeads/LoadLeads
        // ──────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult LoadLeads([FromBody] SourceAnalysisModel objVal)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired. Please login again." });

                var ds = new DataSet();

                using (var connMT = new SqlConnection(_connMT))
                {
                    connMT.Open();
                    using (var cmd = new SqlCommand("Web_LoadChannelPartnersLeadsReport", connMT))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserId", user.UserId);
                        cmd.Parameters.AddWithValue("@flag", "Fetch_Report");

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
                System.Diagnostics.Debug.WriteLine("[LoadLeads] " + ex.Message);
                var json = JsonConvert.SerializeObject(new { status = false, msg = ex.Message, data = new DataSet() });
                return Content(json, "application/json");
            }
        }

        // ──────────────────────────────────────────────────────
        // POST: /ChannelPartnerLeads/LoadPresalesPreviousRemarks
        // ──────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult LoadPresalesPreviousRemarks([FromBody] EnquiryRemarksModel objVal)
        {
            try
            {
                if (objVal == null || string.IsNullOrWhiteSpace(objVal.ClientId))
                    return Json(new { status = false, msg = "ClientId is required" });

                var ds = new DataSet();

                using (var connMT = new SqlConnection(_connMT))
                {
                    connMT.Open();
                    using (var cmd = new SqlCommand("Web_LoadRemarks", connMT))
                    {
                        cmd.CommandTimeout = 500;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Clientid", objVal.ClientId);
                        cmd.Parameters.AddWithValue("@Flag", "LoadPresalesExeRemarks");

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
                System.Diagnostics.Debug.WriteLine("[LoadPresalesPreviousRemarks] " + ex.Message);
                var json = JsonConvert.SerializeObject(new { status = false, msg = ex.Message, data = new DataSet() });
                return Content(json, "application/json");
            }
        }

        [HttpPost]
        public IActionResult LoadSalesExecutivePreviousRemarks([FromBody] EnquiryRemarksModel objVal)
        {
            try
            {
                if (objVal == null || string.IsNullOrWhiteSpace(objVal.ClientId))
                    return Json(new { status = false, msg = "ClientId is required" });

                objVal.dsExePrevRem.Clear();
                string setClientId = "";

                using (SqlConnection connMT = new SqlConnection(_connMT))
                {
                    connMT.Open();

                    string sqlClient = "SELECT EnqClientid AS ClientId FROM EnquiryDataTelecaller WHERE Status = '1' AND TCClientid = @TCClientId";
                    using (SqlCommand cmdClient = new SqlCommand(sqlClient, connMT))
                    {
                        cmdClient.Parameters.AddWithValue("@TCClientId", objVal.ClientId);
                        using (SqlDataReader sdr = cmdClient.ExecuteReader())
                        {
                            if (sdr.Read())
                                setClientId = Convert.ToString(sdr["ClientId"]);
                        }
                    }

                    // If no mapping found, this ClientId has no enquiry record — return early
                    if (string.IsNullOrEmpty(setClientId))
                    {
                        var noMapJson = JsonConvert.SerializeObject(new { status = false, msg = "No enquiry mapping found for this client.", data = new DataSet() });
                        return Content(noMapJson, "application/json");
                    }

                    using (SqlCommand cmdE = new SqlCommand("Web_LoadRemarks", connMT))
                    {
                        cmdE.CommandTimeout = 500;
                        cmdE.CommandType = CommandType.StoredProcedure;
                        cmdE.Parameters.AddWithValue("@Clientid", setClientId);
                        cmdE.Parameters.AddWithValue("@Flag", "Sales_FullRemarks");
                        SqlDataAdapter daE = new SqlDataAdapter(cmdE);
                        daE.Fill(objVal.dsExePrevRem);
                    }

                    bool found = objVal.dsExePrevRem.Tables.Count > 0 && objVal.dsExePrevRem.Tables[0].Rows.Count > 0;
                    var json = JsonConvert.SerializeObject(new
                    {
                        status = found,
                        msg = found ? "Success" : "No remarks found",
                        data = objVal.dsExePrevRem
                    });
                    return Content(json, "application/json");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[LoadSalesExecutivePreviousRemarks] " + ex.Message);
                var json = JsonConvert.SerializeObject(new { status = false, msg = ex.Message, data = new DataSet() });
                return Content(json, "application/json");
            }
        }

        [HttpGet]
        public IActionResult LoadEmployeeDocument()
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired" });

                var dt = new DataTable();
                using (var con = new SqlConnection(_connDB))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("Web_UserRights", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 120;
                        cmd.Parameters.AddWithValue("@EmpId", user.UserId);
                        cmd.Parameters.AddWithValue("@Flag", "LoadEmployeeimage");
                        var da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    return Content(JsonConvert.SerializeObject(new { status = true, msg = "Success", data = dt }), "application/json");
                }
                else
                {
                    return Json(new { status = false, msg = "No image found" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadEmployeeDocument] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }
        // ──────────────────────────────────────────────────────
        // CRM EXECUTIVE DASHBOARD METHODS
        // ──────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult LoadCurrentDateExtension()
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null) return Json(new { status = false, msg = "Session expired" });

                var dt = new DataTable();
                using (var con = new SqlConnection(_connDB))
                using (var cmd = new SqlCommand("Web_CRMIndividualDashboard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 500;
                    cmd.Parameters.AddWithValue("@EmpId", user.UserId);
                    cmd.Parameters.AddWithValue("@Flag", "CRM_CurrentDate_Extension");

                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                return Content(JsonConvert.SerializeObject(new { status = dt.Rows.Count > 0, msg = dt.Rows.Count > 0 ? "Success" : "False", data = new { Table = dt } }), "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadCurrentDateExtension] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult LoadPreDateExtension()
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null) return Json(new { status = false, msg = "Session expired" });

                var dt = new DataTable();
                using (var con = new SqlConnection(_connDB))
                using (var cmd = new SqlCommand("Web_CRMIndividualDashboard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 500;
                    cmd.Parameters.AddWithValue("@EmpId", user.UserId);
                    cmd.Parameters.AddWithValue("@Flag", "CRM_PreDate_Extension");

                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                return Content(JsonConvert.SerializeObject(new { status = dt.Rows.Count > 0, msg = dt.Rows.Count > 0 ? "Success" : "False", data = new { Table = dt } }), "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadPreDateExtension] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult LoadPendingWelcomeMail()
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null) return Json(new { status = false, msg = "Session expired" });

                DateTime setDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var dt = new DataTable();
                using (var con = new SqlConnection(_connDB))
                using (var cmd = new SqlCommand("Web_CRMIndividualDashboard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 500;
                    cmd.Parameters.AddWithValue("@EmpId", user.UserId);
                    cmd.Parameters.AddWithValue("@BookedDate", setDate);
                    cmd.Parameters.AddWithValue("@Flag", "Load_WelcomeMail");

                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                return Content(JsonConvert.SerializeObject(new { status = dt.Rows.Count > 0, msg = dt.Rows.Count > 0 ? "Success" : "False", data = new { Table = dt } }), "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadPendingWelcomeMail] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult LoadBookingToReg()
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null) return Json(new { status = false, msg = "Session expired" });

                var dt = new DataTable();
                using (var con = new SqlConnection(_connMT))
                using (var cmd = new SqlCommand("Web_BookingToRegProcessFlowEmpEntryFillTask", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 500;
                    cmd.Parameters.AddWithValue("@CRMId", user.UserId);
                    cmd.Parameters.AddWithValue("@Flag", "LOAD_IndividualBR");

                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                return Content(JsonConvert.SerializeObject(new { status = dt.Rows.Count > 0, msg = dt.Rows.Count > 0 ? "Success" : "False", data = new { Table = dt } }), "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadBookingToReg] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult LoadCustomerExtensions()
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null) return Json(new { status = false, msg = "Session expired" });

                var dt = new DataTable();
                using (var con = new SqlConnection(_connDB))
                using (var cmd = new SqlCommand("Web_CRMIndividualDashboard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 500;
                    cmd.Parameters.AddWithValue("@EmpId", user.UserId);
                    cmd.Parameters.AddWithValue("@Flag", "CRM_TOBE_APPROVE");

                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                return Content(JsonConvert.SerializeObject(new { status = dt.Rows.Count > 0, msg = dt.Rows.Count > 0 ? "Success" : "False", data = new { Table = dt } }), "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadCustomerExtensions] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult LoadSaleDeedAgreementNotification()
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null) return Json(new { status = false, msg = "Session expired" });

                DateTime setDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var dt = new DataTable();
                using (var con = new SqlConnection(_connMT))
                using (var cmd = new SqlCommand("FlatPlotSaleAggreementDeedFollowupReminder", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 500;
                    cmd.Parameters.AddWithValue("@Flag", "Load");
                    cmd.Parameters.AddWithValue("@RequestDate", setDate);

                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                return Content(JsonConvert.SerializeObject(new { status = dt.Rows.Count > 0, msg = dt.Rows.Count > 0 ? "Success" : "False", data = new { Table = dt } }), "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadSaleDeedAgreementNotification] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────
        // SALES DASHBOARD (DEPT-5) METHODS
        // ──────────────────────────────────────────────────────

        [HttpPost]
        public IActionResult LoadUpcomingFollowup()
        {
            return ExecuteSalesDashboardProcedure("Executive_UpcomingFollowup", "EXECUTIVE");
        }

        [HttpPost]
        public IActionResult LoadLeadCancellationRequests()
        {
            return ExecuteSalesDashboardProcedure("PartialCancel");
        }

        [HttpPost]
        public IActionResult LoadExecutiveBookedData()
        {
            return ExecuteSalesDashboardProcedure("Executive_BookingHistory");
        }

        [HttpPost]
        public IActionResult LoadExecutiveTargetAchived()
        {
            return ExecuteSalesDashboardProcedure("Executivewise_Target");
        }

        [HttpPost]
        public IActionResult LoadExecutiveTarget()
        {
            return ExecuteSalesDashboardProcedure("Executive_Target");
        }

        [HttpPost]
        public IActionResult LoadProjectWiseLeadCount()
        {
            return ExecuteSalesDashboardProcedure("Executive_ProjectLeadCount");
        }

        [HttpPost]
        public IActionResult LoadLeadCategoryHotWarmCold()
        {
            return ExecuteSalesDashboardProcedure("HotWarmColdLeadSummary");
        }

        [HttpPost]
        public IActionResult LoadStageLeadChart()
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null) return Json(new { status = false, msg = "Session expired" });

                var month = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                var firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddSeconds(-1);

                var dt = new DataTable();
                using (var con = new SqlConnection(_connMT))
                using (var cmd = new SqlCommand("Web_SalesTeamDashboard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 500;
                    cmd.Parameters.AddWithValue("@EmpId", user.UserId);
                    cmd.Parameters.AddWithValue("@Month", "");
                    cmd.Parameters.AddWithValue("@StartDate", firstDayOfMonth);
                    cmd.Parameters.AddWithValue("@EndDate", lastDayOfMonth);
                    cmd.Parameters.AddWithValue("@Flag", "LeadCount_Status");

                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
                
                return Content(JsonConvert.SerializeObject(new { status = dt.Rows.Count > 0, msg = dt.Rows.Count > 0 ? "Success" : "False", data = new { Table = dt } }), "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadStageLeadChart] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        private IActionResult ExecuteSalesDashboardProcedure(string flag, string forcedUserType = null)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null) return Json(new { status = false, msg = "Session expired" });

                var ds = new DataSet();
                using (var con = new SqlConnection(_connMT))
                using (var cmd = new SqlCommand("Web_SalesTeamDashboard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 500;
                    cmd.Parameters.AddWithValue("@Flag", flag);
                    cmd.Parameters.AddWithValue("@EmpId", user.UserId);
                    cmd.Parameters.AddWithValue("@UserType", forcedUserType ?? user.Role);

                    var da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                }
                
                var dataObj = new System.Collections.Generic.Dictionary<string, object>();
                if (ds.Tables.Count > 0)
                {
                    for (int i = 0; i < ds.Tables.Count; i++)
                    {
                        string tableName = i == 0 ? "Table" : "Table" + i;
                        dataObj.Add(tableName, ds.Tables[i]);
                    }
                }

                return Content(JsonConvert.SerializeObject(new { status = ds.Tables.Count > 0, msg = ds.Tables.Count > 0 ? "Success" : "False", data = dataObj }), "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ExecuteSalesDashboardProcedure - {flag}] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult LoadProjectwiseSummary()
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null) return Json(new { status = false, msg = "Session expired" });

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

                return Content(JsonConvert.SerializeObject(new { status = dt.Rows.Count > 0, msg = dt.Rows.Count > 0 ? "Success" : "False", data = new { Table = dt } }), "application/json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadProjectwiseSummary] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }
    }
}

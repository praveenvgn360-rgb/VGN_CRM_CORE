using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;
using VGN_CRM_CORE.Models;

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    public class IndentRequestController : Controller
    {
        private readonly string _connPROJ;
        private readonly IConfiguration _configuration;

        public IndentRequestController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connPROJ = configuration.GetActiveConnectionString("connPROJ");
        }

        // GET: /IndentRequest or /IndentRequest/Index
        [HttpGet]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.Title = "Indent Request — VGN ERP";
            ViewBag.ActiveMenu = "Indent Request";
            ViewBag.UserId = user.UserId;
            ViewBag.UserName = user.UserName;
            ViewBag.CurrentDate = DateTime.Now.ToString("dd-MM-yyyy");
            ViewBag.CurrentDateTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm");

            return View();
        }

        // GET: /IndentRequest/GetNextRefNo
        // Calls Stored Procedure: dbo.Web_GetNextIndentRequestNo
        [HttpGet]
        public async Task<IActionResult> GetNextRefNo()
        {
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("dbo.Web_GetNextIndentRequestNo", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    await con.OpenAsync();
                    var res = await cmd.ExecuteScalarAsync();
                    string nextNo = (res != null && res != DBNull.Value) ? res.ToString() : "1";
                    return Json(new { success = true, nextRefNo = nextNo });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error getting next RequestNo: " + ex.Message, nextRefNo = "1" });
            }
        }

        // GET: /IndentRequest/GetProjects
        // Calls Stored Procedure: dbo.Web_GetProjects
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var list = new List<object>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("dbo.Web_GetProjects", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    await con.OpenAsync();
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                        {
                            list.Add(new
                            {
                                ProjectKickoffId = r["ProjectKickoffId"] != DBNull.Value ? Convert.ToInt32(r["ProjectKickoffId"]) : 0,
                                ProjectName = r["ProjectName"] != DBNull.Value ? r["ProjectName"].ToString() : "",
                                CostCentreId = r["CostCentreId"] != DBNull.Value ? r["CostCentreId"].ToString() : ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading projects: " + ex.Message, data = new List<object>() });
            }

            return Json(new { success = true, data = list });
        }

        // GET: /IndentRequest/GetWorkGroups
        // Calls Stored Procedure: dbo.Web_SaveWorkGroupMas @Flag='FETCHBYALL'
        [HttpGet]
        public async Task<IActionResult> GetWorkGroups()
        {
            var list = new List<object>();
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("dbo.Web_SaveWorkGroupMas", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Flag", "FETCHBYALL");
                    await con.OpenAsync();
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                        {
                            list.Add(new
                            {
                                WorkGroupId = r["WorkGroupId"] != DBNull.Value ? r["WorkGroupId"].ToString() : "",
                                WorkGroupName = r["WorkGroupName"] != DBNull.Value ? r["WorkGroupName"].ToString() : "",
                                SerialNo = r["SerialNo"] != DBNull.Value ? r["SerialNo"].ToString() : ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading work groups: " + ex.Message, data = new List<object>() });
            }

            return Json(new { success = true, data = list });
        }

        // GET: /IndentRequest/GetProjectWBSList
        // Calls Stored Procedure: dbo.Web_GetProjectWBSList
        [HttpGet]
        public async Task<IActionResult> GetProjectWBSList(int? projectId, string q)
        {
            var list = new List<object>();
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("dbo.Web_GetProjectWBSList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@ProjectKickoffId", projectId.HasValue ? (object)projectId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@SearchTerm", string.IsNullOrWhiteSpace(q) ? (object)DBNull.Value : q.Trim());
                    await con.OpenAsync();
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                        {
                            list.Add(new
                            {
                                WBSId = r["WBSId"] != DBNull.Value ? r["WBSId"].ToString() : "",
                                WBSName = r["WBSName"] != DBNull.Value ? r["WBSName"].ToString() : "",
                                FullWBSName = r["FullWBSName"] != DBNull.Value ? r["FullWBSName"].ToString() : ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading WBS: " + ex.Message, data = new List<object>() });
            }

            return Json(new { success = true, data = list });
        }

        // GET: /IndentRequest/GetResources
        // Calls Stored Procedure: dbo.Web_LoadResourceMas @Flag='FETCHALL'
        [HttpGet]
        public async Task<IActionResult> GetResources()
        {
            var list = new List<object>();
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        rm.ResourceId, 
                        rm.ResourceName, 
                        ISNULL(rg.ResourceGroupName, '') AS ResourceGroupName,
                        CASE 
                            WHEN u.UnitName IS NOT NULL AND u.UnitName <> '' AND u.UnitName NOT LIKE '%?%' THEN u.UnitName
                            WHEN rm.UnitId IS NOT NULL AND rm.UnitId <> '' AND rm.UnitId NOT LIKE '%?%' THEN rm.UnitId
                            ELSE 'Nos'
                        END AS UnitName,
                        ISNULL(rm.UnitId, 'Nos') AS UnitId
                    FROM dbo.ResourceMas rm
                    LEFT JOIN dbo.ResourceGroupMas rg ON CAST(rg.ResourceGroupId AS VARCHAR(50)) = rm.ResourceGroupId
                    LEFT JOIN dbo.UOM u ON CAST(u.UnitId AS VARCHAR(50)) = rm.UnitId
                    WHERE ISNULL(rm.Status, '1') = '1'
                    ORDER BY rm.ResourceName ASC", con))
                {
                    cmd.CommandTimeout = 60;
                    await con.OpenAsync();
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                        {
                            list.Add(new
                            {
                                ResourceId = r["ResourceId"] != DBNull.Value ? r["ResourceId"].ToString() : "",
                                ResourceName = r["ResourceName"] != DBNull.Value ? r["ResourceName"].ToString() : "",
                                ResourceGroupName = r["ResourceGroupName"] != DBNull.Value ? r["ResourceGroupName"].ToString() : "",
                                UnitName = r["UnitName"] != DBNull.Value ? r["UnitName"].ToString() : "Nos",
                                UnitId = r["UnitId"] != DBNull.Value ? r["UnitId"].ToString() : ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading resources: " + ex.Message, data = new List<object>() });
            }

            return Json(new { success = true, data = list });
        }

        // GET: /IndentRequest/GetIOWList
        // Calls Stored Procedure: dbo.Web_SearchIOWMas @Flag='FETCHALL'
        [HttpGet]
        public async Task<IActionResult> GetIOWList()
        {
            var list = new List<object>();
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("dbo.Web_SearchIOWMas", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Flag", "FETCHALL");
                    await con.OpenAsync();
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                        {
                            string id = r["IOWId"] != DBNull.Value ? r["IOWId"].ToString() : "";
                            string sno = r["SerialNo"] != DBNull.Value ? r["SerialNo"].ToString() : "";
                            string spec = r["Specification"] != DBNull.Value ? r["Specification"].ToString() : "";
                            string text = string.IsNullOrWhiteSpace(sno) ? spec : (sno + " - " + spec);
                            list.Add(new
                            {
                                id = id,
                                text = text,
                                iowId = id,
                                serialNo = sno,
                                specification = spec
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading IOW list: " + ex.Message, data = new List<object>() });
            }

            return Json(new { success = true, data = list, results = list });
        }

        // GET: /IndentRequest/SearchIOW?q=cement
        // Fast Select2 AJAX search across 13,000+ IOWs in IOWMas
        [HttpGet]
        public async Task<IActionResult> SearchIOW(string q)
        {
            var list = new List<object>();
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(@"
                    SELECT TOP 50
                        i.IOWId,
                        ISNULL(i.SerialNo, '') AS SerialNo,
                        ISNULL(i.Specification, '') AS Specification
                    FROM dbo.IOWMas i
                    WHERE ISNULL(i.Status, '1') = '1'
                      AND (
                          @Search IS NULL OR @Search = '' OR 
                          i.Specification LIKE '%' + @Search + '%' OR 
                          i.SerialNo LIKE '%' + @Search + '%'
                      )
                    ORDER BY i.IOWId DESC", con))
                {
                    cmd.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(q) ? (object)DBNull.Value : q.Trim());
                    await con.OpenAsync();
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                        {
                            string id = r["IOWId"].ToString();
                            string sno = r["SerialNo"].ToString();
                            string spec = r["Specification"].ToString();
                            string text = string.IsNullOrWhiteSpace(sno) ? spec : (sno + " - " + spec);
                            list.Add(new { id = id, text = text, iowId = id, serialNo = sno, specification = spec });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, results = new List<object>() });
            }
            return Json(new { success = true, results = list });
        }

        // GET: /IndentRequest/GetWBSList
        // Returns active WBS from WBSMaster for Select2 dropdown
        [HttpGet]
        public async Task<IActionResult> GetWBSList(string q)
        {
            var list = new List<object>();
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(@"
                    SELECT WBSId, WBSName
                    FROM dbo.WBSMaster
                    WHERE ISNULL(Status, '1') = '1'
                      AND (@Search IS NULL OR @Search = '' OR WBSName LIKE '%' + @Search + '%')
                    ORDER BY WBSId ASC", con))
                {
                    cmd.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(q) ? (object)DBNull.Value : q.Trim());
                    await con.OpenAsync();
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                        {
                            string id = r["WBSId"].ToString();
                            string name = r["WBSName"].ToString();
                            list.Add(new { id = id, text = name, wbsId = id, wbsName = name });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading WBS: " + ex.Message, data = new List<object>() });
            }
            return Json(new { success = true, data = list, results = list });
        }

        // GET: /IndentRequest/GetProjectResources?projectId=123
        // Fetches resources allocated in the selected project's BOQ/Estimation
        [HttpGet]
        public async Task<IActionResult> GetProjectResources(int? projectId)
        {
            var list = new List<object>();
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(@"
                    SELECT DISTINCT
                        r.ResourceId,
                        r.ResourceCode,
                        r.ResourceName,
                        ISNULL(u.UnitName, ISNULL(r.UnitId, 'Nos')) AS UnitName,
                        r.UnitId
                    FROM dbo.Project_BOQ_ResourceMas r
                    LEFT JOIN dbo.UOM u ON CAST(u.UnitId AS VARCHAR(50)) = CAST(r.UnitId AS VARCHAR(50))
                    WHERE ISNULL(r.Status, '1') = '1'
                      AND (@ProjectId IS NULL OR r.ProectKickOfId = CAST(@ProjectId AS VARCHAR(50)))
                    ORDER BY r.ResourceName ASC", con))
                {
                    cmd.Parameters.AddWithValue("@ProjectId", projectId.HasValue ? (object)projectId.Value : DBNull.Value);
                    await con.OpenAsync();
                    using (var rd = await cmd.ExecuteReaderAsync())
                    {
                        while (await rd.ReadAsync())
                        {
                            list.Add(new
                            {
                                ResourceId = rd["ResourceId"] != DBNull.Value ? rd["ResourceId"].ToString() : "",
                                ResourceCode = rd["ResourceCode"] != DBNull.Value ? rd["ResourceCode"].ToString() : "",
                                ResourceName = rd["ResourceName"] != DBNull.Value ? rd["ResourceName"].ToString() : "",
                                UnitName = rd["UnitName"] != DBNull.Value ? rd["UnitName"].ToString() : "",
                                UnitId = rd["UnitId"] != DBNull.Value ? rd["UnitId"].ToString() : ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading project resources: " + ex.Message, data = new List<object>() });
            }

            return Json(new { success = true, data = list });
        }

        // GET: /IndentRequest/GetUOMList
        // Calls Stored Procedure: dbo.Web_GetUOMList
        [HttpGet]
        public async Task<IActionResult> GetUOMList()
        {
            var list = new List<object>();
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("dbo.Web_GetUOMList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    await con.OpenAsync();
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                        {
                            list.Add(new
                            {
                                UnitId = r["UnitId"] != DBNull.Value ? r["UnitId"].ToString() : "",
                                UnitName = r["UnitName"] != DBNull.Value ? r["UnitName"].ToString() : ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading UOM: " + ex.Message, data = new List<object>() });
            }

            return Json(new { success = true, data = list });
        }

        // GET: /IndentRequest/GetIndentList
        // Calls Stored Procedure: dbo.Web_GetProject_IndentRequest @Flag='FETCHALL'
        [HttpGet]
        public async Task<IActionResult> GetIndentList()
        {
            var list = new List<IndentRequestListItemModel>();
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("dbo.Web_GetProject_IndentRequest", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Flag", "FETCHALL");
                    await con.OpenAsync();
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                        {
                            list.Add(new IndentRequestListItemModel
                            {
                                IndentMasId = r["IndentMasId"] != DBNull.Value ? Convert.ToInt32(r["IndentMasId"]) : 0,
                                RequestNo = r["RequestNo"] != DBNull.Value ? r["RequestNo"].ToString() : "",
                                CostcenterId = r["CostcenterId"] != DBNull.Value ? r["CostcenterId"].ToString() : "",
                                ProectKickOfId = r["ProectKickOfId"] != DBNull.Value ? r["ProectKickOfId"].ToString() : "",
                                ProjectName = r["ProjectName"] != DBNull.Value ? r["ProjectName"].ToString() : "",
                                RequestType = r["RequestType"] != DBNull.Value ? r["RequestType"].ToString() : "Material",
                                RequestDatetime = r["RequestDatetime"] != DBNull.Value ? r["RequestDatetime"].ToString() : "",
                                Remarks = r["Remarks"] != DBNull.Value ? r["Remarks"].ToString() : "",
                                CreatedBy = r["CreatedBy"] != DBNull.Value ? r["CreatedBy"].ToString() : "",
                                CreatedDate = r["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(r["CreatedDate"]) : (DateTime?)null,
                                Status = r["Status"] != DBNull.Value ? r["Status"].ToString() : "1",
                                TotalItems = r["TotalItems"] != DBNull.Value ? Convert.ToInt32(r["TotalItems"]) : 0
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading Indent list: " + ex.Message, data = new List<IndentRequestListItemModel>() });
            }

            return Json(new { success = true, data = list });
        }

        // GET: /IndentRequest/GetIndentById?id=123
        // Calls Stored Procedure: dbo.Web_GetProject_IndentRequest @Flag='FETCHBYID'
        [HttpGet]
        public async Task<IActionResult> GetIndentById(int id)
        {
            if (id <= 0) return Json(new { success = false, message = "Invalid Indent ID." });

            IndentRequestHeaderModel header = null;
            var items = new List<IndentRequestItemModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("dbo.Web_GetProject_IndentRequest", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Flag", "FETCHBYID");
                    cmd.Parameters.AddWithValue("@IndentMasId", id);

                    await con.OpenAsync();
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        // Table 1: Master
                        if (await r.ReadAsync())
                        {
                            header = new IndentRequestHeaderModel
                            {
                                IndentMasId = r["IndentMasId"] != DBNull.Value ? Convert.ToInt32(r["IndentMasId"]) : 0,
                                CostcenterId = r["CostcenterId"] != DBNull.Value ? r["CostcenterId"].ToString() : "",
                                ProectKickOfId = r["ProectKickOfId"] != DBNull.Value ? r["ProectKickOfId"].ToString() : "",
                                ProjectName = r["ProjectName"] != DBNull.Value ? r["ProjectName"].ToString() : "",
                                RequestType = r["RequestType"] != DBNull.Value ? r["RequestType"].ToString() : "Material",
                                RequestDatetime = r["RequestDatetime"] != DBNull.Value ? r["RequestDatetime"].ToString() : "",
                                RequestNo = r["RequestNo"] != DBNull.Value ? r["RequestNo"].ToString() : "",
                                Remarks = r["Remarks"] != DBNull.Value ? r["Remarks"].ToString() : "",
                                CreatedBy = r["CreatedBy"] != DBNull.Value ? r["CreatedBy"].ToString() : "",
                                CreatedDate = r["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(r["CreatedDate"]) : (DateTime?)null,
                                Status = r["Status"] != DBNull.Value ? r["Status"].ToString() : "1"
                            };
                        }

                        // Table 2: Detail items
                        if (await r.NextResultAsync())
                        {
                            while (await r.ReadAsync())
                            {
                                items.Add(new IndentRequestItemModel
                                {
                                    IndentId = r["IndentId"] != DBNull.Value ? Convert.ToInt32(r["IndentId"]) : 0,
                                    IndentMasId = r["IndentMasId"] != DBNull.Value ? r["IndentMasId"].ToString() : "",
                                    CostcenterId = r["CostcenterId"] != DBNull.Value ? r["CostcenterId"].ToString() : "",
                                    ProectKickOfId = r["ProectKickOfId"] != DBNull.Value ? r["ProectKickOfId"].ToString() : "",
                                    WorkGroupId = r["WorkGroupId"] != DBNull.Value ? r["WorkGroupId"].ToString() : "",
                                    WorkGroupName = r["WorkGroupName"] != DBNull.Value ? r["WorkGroupName"].ToString() : "",
                                    WBSId = r["WBSId"] != DBNull.Value ? r["WBSId"].ToString() : "",
                                    Project_WBSId = r["Project_WBSId"] != DBNull.Value ? r["Project_WBSId"].ToString() : "",
                                    WBSName = r["WBSName"] != DBNull.Value ? r["WBSName"].ToString() : "",
                                    IOWId = r["IOWId"] != DBNull.Value ? r["IOWId"].ToString() : "",
                                    Project_IOWId = r["Project_IOWId"] != DBNull.Value ? r["Project_IOWId"].ToString() : "",
                                    IOWSpecification = r["IOWSpecification"] != DBNull.Value ? r["IOWSpecification"].ToString() : "",
                                    ResourceId = r["ResourceId"] != DBNull.Value ? r["ResourceId"].ToString() : "",
                                    ResourceName = r["ResourceName"] != DBNull.Value ? r["ResourceName"].ToString() : "",
                                    Qty = r["Qty"] != DBNull.Value ? r["Qty"].ToString() : "0",
                                    Unit = r["Unit"] != DBNull.Value ? r["Unit"].ToString() : "",
                                    RequiredDate = r["RequiredDate"] != DBNull.Value ? Convert.ToDateTime(r["RequiredDate"]) : (DateTime?)null,
                                    Remarks = r["Remarks"] != DBNull.Value ? r["Remarks"].ToString() : "",
                                    ApprovalLogId = r["ApprovalLogId"] != DBNull.Value ? r["ApprovalLogId"].ToString() : "",
                                    ApprovalDatetime = r["ApprovalDatetime"] != DBNull.Value ? Convert.ToDateTime(r["ApprovalDatetime"]) : (DateTime?)null,
                                    Reject = r["Reject"] != DBNull.Value ? r["Reject"].ToString() : "",
                                    Status = r["Status"] != DBNull.Value ? r["Status"].ToString() : "1"
                                });
                            }
                        }
                    }
                }

                if (header == null)
                {
                    return Json(new { success = false, message = $"Indent Request #{id} not found." });
                }

                return Json(new { success = true, header, items });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error fetching Indent details: " + ex.Message });
            }
        }

        // POST: /IndentRequest/SaveIndentRequest
        // Transactional Save coordinating Master and Detail Stored Procedures
        [HttpPost]
        public async Task<IActionResult> SaveIndentRequest([FromBody] IndentRequestSavePayload payload)
        {
            if (payload == null || payload.Header == null)
            {
                return Json(new { success = false, message = "Invalid request payload. No data received." });
            }

            if (string.IsNullOrWhiteSpace(payload.Header.ProectKickOfId))
            {
                return Json(new { success = false, message = "Please select a Project." });
            }

            if (payload.Items == null || payload.Items.Count == 0)
            {
                return Json(new { success = false, message = "At least one item must be added to the Indent Request." });
            }

            var user = SessionHelper.GetUserSession(HttpContext.Session);
            string currentUser = user != null ? user.UserName : "Admin";
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            string hostName = Environment.MachineName;

            bool isUpdate = payload.Header.IndentMasId > 0;
            int savedIndentMasId = payload.Header.IndentMasId;

            using (var con = new SqlConnection(_connPROJ))
            {
                await con.OpenAsync();
                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        // -------------------------------------------------------------
                        // Step 1: Save Master via dbo.Web_SaveProject_IndentRequestMas
                        // -------------------------------------------------------------
                        using (var cmdMas = new SqlCommand("dbo.Web_SaveProject_IndentRequestMas", con, tran))
                        {
                            cmdMas.CommandType = CommandType.StoredProcedure;
                            cmdMas.CommandTimeout = 60;

                            cmdMas.Parameters.AddWithValue("@Flag", isUpdate ? "UPDATE" : "INSERT");
                            cmdMas.Parameters.AddWithValue("@IndentMasId", isUpdate ? savedIndentMasId : 0);
                            cmdMas.Parameters.AddWithValue("@CostcenterId", payload.Header.CostcenterId ?? "");
                            cmdMas.Parameters.AddWithValue("@ProectKickOfId", payload.Header.ProectKickOfId ?? "");
                            cmdMas.Parameters.AddWithValue("@RequestType", payload.Header.RequestType ?? "Material");
                            cmdMas.Parameters.AddWithValue("@RequestDatetime", payload.Header.RequestDatetime ?? DateTime.Now.ToString("dd-MM-yyyy HH:mm"));
                            cmdMas.Parameters.AddWithValue("@RequestNo", payload.Header.RequestNo ?? "");
                            cmdMas.Parameters.AddWithValue("@Remarks", payload.Header.Remarks ?? "");
                            cmdMas.Parameters.AddWithValue("@CreatedBy", currentUser);
                            cmdMas.Parameters.AddWithValue("@IPAddress", ipAddress);
                            cmdMas.Parameters.AddWithValue("@HostName", hostName);

                            using (var r = await cmdMas.ExecuteReaderAsync())
                            {
                                if (await r.ReadAsync())
                                {
                                    int outId = r["IndentMasId"] != DBNull.Value ? Convert.ToInt32(r["IndentMasId"]) : 0;
                                    if (outId > 0) savedIndentMasId = outId;
                                }
                            }
                        }

                        if (savedIndentMasId <= 0)
                        {
                            throw new Exception("Master stored procedure dbo.Web_SaveProject_IndentRequestMas failed to return a valid IndentMasId.");
                        }

                        // -------------------------------------------------------------
                        // Step 2: If Update, deactivate existing detail entries before sync
                        // -------------------------------------------------------------
                        if (isUpdate)
                        {
                            using (var cmdDeact = new SqlCommand("dbo.Web_SaveProject_IndentRequestEntry", con, tran))
                            {
                                cmdDeact.CommandType = CommandType.StoredProcedure;
                                cmdDeact.CommandTimeout = 60;
                                cmdDeact.Parameters.AddWithValue("@Flag", "DEACTIVATE_BY_MAS");
                                cmdDeact.Parameters.AddWithValue("@IndentMasId", savedIndentMasId.ToString());
                                cmdDeact.Parameters.AddWithValue("@CreatedBy", currentUser);
                                await cmdDeact.ExecuteNonQueryAsync();
                            }
                        }

                        // -------------------------------------------------------------
                        // Step 3: Insert / Upsert each detail item via dbo.Web_SaveProject_IndentRequestEntry
                        // -------------------------------------------------------------
                        foreach (var item in payload.Items)
                        {
                            // Skip empty row if no resource is selected
                            if (string.IsNullOrWhiteSpace(item.ResourceId) && string.IsNullOrWhiteSpace(item.ResourceName))
                            {
                                continue;
                            }

                            using (var cmdDetail = new SqlCommand("dbo.Web_SaveProject_IndentRequestEntry", con, tran))
                            {
                                cmdDetail.CommandType = CommandType.StoredProcedure;
                                cmdDetail.CommandTimeout = 60;

                                cmdDetail.Parameters.AddWithValue("@Flag", "INSERT");
                                cmdDetail.Parameters.AddWithValue("@IndentId", item.IndentId > 0 && !isUpdate ? item.IndentId : 0);
                                cmdDetail.Parameters.AddWithValue("@IndentMasId", savedIndentMasId.ToString());
                                cmdDetail.Parameters.AddWithValue("@CostcenterId", payload.Header.CostcenterId ?? "");
                                cmdDetail.Parameters.AddWithValue("@ProectKickOfId", payload.Header.ProectKickOfId ?? "");
                                cmdDetail.Parameters.AddWithValue("@WorkGroupId", item.WorkGroupId ?? "");
                                cmdDetail.Parameters.AddWithValue("@IOWId", item.IOWId ?? "");
                                cmdDetail.Parameters.AddWithValue("@Project_IOWId", item.Project_IOWId ?? "");
                                cmdDetail.Parameters.AddWithValue("@WBSId", item.WBSId ?? "");
                                cmdDetail.Parameters.AddWithValue("@Project_WBSId", item.Project_WBSId ?? "");
                                cmdDetail.Parameters.AddWithValue("@ResourceId", item.ResourceId ?? "");
                                cmdDetail.Parameters.AddWithValue("@Qty", item.Qty ?? "0");
                                cmdDetail.Parameters.AddWithValue("@Unit", item.Unit ?? "");
                                cmdDetail.Parameters.AddWithValue("@RequiredDate", item.RequiredDate.HasValue ? (object)item.RequiredDate.Value : DBNull.Value);
                                cmdDetail.Parameters.AddWithValue("@Remarks", item.Remarks ?? "");
                                cmdDetail.Parameters.AddWithValue("@ApprovalLogId", item.ApprovalLogId ?? "");
                                cmdDetail.Parameters.AddWithValue("@ApprovalDatetime", item.ApprovalDatetime.HasValue ? (object)item.ApprovalDatetime.Value : DBNull.Value);
                                cmdDetail.Parameters.AddWithValue("@Reject", item.Reject ?? "");
                                cmdDetail.Parameters.AddWithValue("@CreatedBy", currentUser);
                                cmdDetail.Parameters.AddWithValue("@IPAddress", ipAddress);
                                cmdDetail.Parameters.AddWithValue("@HostName", hostName);

                                await cmdDetail.ExecuteNonQueryAsync();
                            }
                        }

                        tran.Commit();

                        return Json(new
                        {
                            success = true,
                            indentMasId = savedIndentMasId,
                            message = $"Indent Request #{(payload.Header.RequestNo ?? savedIndentMasId.ToString())} saved successfully!"
                        });
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        return Json(new { success = false, message = "Error saving Indent Request: " + ex.Message });
                    }
                }
            }
        }

        // POST: /IndentRequest/DeleteIndent
        // Soft delete master and detail records
        [HttpPost]
        public async Task<IActionResult> DeleteIndent(int id)
        {
            if (id <= 0) return Json(new { success = false, message = "Invalid Indent ID." });

            var user = SessionHelper.GetUserSession(HttpContext.Session);
            string currentUser = user != null ? user.UserName : "Admin";

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("dbo.Web_SaveProject_IndentRequestMas", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Flag", "DELETE");
                    cmd.Parameters.AddWithValue("@IndentMasId", id);
                    cmd.Parameters.AddWithValue("@CreatedBy", currentUser);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }

                return Json(new { success = true, message = $"Indent Request #{id} deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting Indent Request: " + ex.Message });
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;
using VGN_CRM_CORE.Models;

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    public class OtherCostBreakupController : Controller
    {
        private readonly string _connPROJ;
        private const string SP_LOAD_BREAKUP = "Web_LoadOtherCostBreakup";
        private const string SP_SAVE_BREAKUP = "Web_SaveOtherCostBreakup";
        private const string SP_DELETE_BREAKUP = "Web_DeleteOtherCostBreakup";
        private const string SP_SERVICE_MAS = "Web_SaveProjectServiceMas";
        private const string SP_GET_PROJECTS = "Web_GetProjects";

        public OtherCostBreakupController(IConfiguration configuration)
        {
            _connPROJ = configuration.GetActiveConnectionString("connPROJ")
                        ?? configuration.GetConnectionString("ConnPROJ")
                        ?? configuration.GetConnectionString("connPROJ");
        }

        // GET: /OtherCostBreakup or /OtherCostBreakup/Index
        [HttpGet]
        [Route("/OtherCostBreakup")]
        [Route("/OtherCostBreakup/Index")]
        public IActionResult Index(string projectKickoffId = null)
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.Title = "Other Cost Breakup — VGN ERP";
            ViewBag.ActiveMenu = "Other Cost Breakup";
            ViewBag.UserId = user.UserId;
            ViewBag.UserName = user.UserName;
            ViewBag.SelectedProjectId = projectKickoffId ?? "";

            var menus = SessionHelper.GetMenuList(HttpContext.Session);
            if (menus != null)
            {
                var boqMenu = menus.FirstOrDefault(m => m.ControllerName == "ProjectIOW");
                if (!menus.Any(m => m.ControllerName == "OtherCostBreakup"))
                {
                    menus.Add(new MenuModel
                    {
                        Department = boqMenu != null ? boqMenu.Department : "PROJECTS",
                        ModuleType = boqMenu != null ? boqMenu.ModuleType : "ACTIVITIES",
                        ModuleCaptionName = "Other Cost Breakup",
                        ControllerName = "OtherCostBreakup",
                        ActionName = "Index"
                    });
                }
                if (!menus.Any(m => m.ControllerName == "ServiceMaster"))
                {
                    menus.Add(new MenuModel
                    {
                        Department = boqMenu != null ? boqMenu.Department : "PROJECTS",
                        ModuleType = "MASTER",
                        ModuleCaptionName = "Service Master",
                        ControllerName = "ServiceMaster",
                        ActionName = "Index"
                    });
                }
                SessionHelper.SetMenuList(HttpContext.Session, menus);
            }

            return View();
        }

        // GET: /OtherCostBreakup/GetNextRefNo
        // Calls Stored Procedure: dbo.Web_GetNextBOQRefNo (same auto-gen ID as Bill of Quantities screen)
        [HttpGet]
        public async Task<IActionResult> GetNextRefNo()
        {
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("dbo.Web_GetNextBOQRefNo", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    await con.OpenAsync();
                    var res = await cmd.ExecuteScalarAsync();
                    int nextNo = (res != null && res != DBNull.Value) ? Convert.ToInt32(res) : 1;
                    return Json(new { success = true, nextRefNo = nextNo });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error retrieving next RefNo: " + ex.Message, nextRefNo = 1 });
            }
        }

        // GET: /OtherCostBreakup/GetProjects
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var list = new List<object>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_GET_PROJECTS, con))
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

                if (list.Count == 0)
                {
                    list.Add(new { ProjectKickoffId = 1, ProjectName = "Demo Project", CostCentreId = "CC-DEMO-01" });
                    list.Add(new { ProjectKickoffId = 6, ProjectName = "VGN Fairmont", CostCentreId = "CC-006" });
                }

                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                var fallbackList = new List<object>
                {
                    new { ProjectKickoffId = 1, ProjectName = "Demo Project", CostCentreId = "CC-DEMO-01" },
                    new { ProjectKickoffId = 6, ProjectName = "VGN Fairmont", CostCentreId = "CC-006" }
                };
                return Json(new { success = true, data = fallbackList, message = ex.Message });
            }
        }

        // GET: /OtherCostBreakup/GetRegister?projectKickoffId=1
        // Loads master register rows + child transaction lines matching Design Images 1 & 3
        [HttpGet]
        public async Task<IActionResult> GetRegister(string projectKickoffId)
        {
            var masterRows = new List<OtherCostRegisterRowModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_LOAD_BREAKUP, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Action", "REGISTER");
                    cmd.Parameters.AddWithValue("@ProjectKickoffId", (object)projectKickoffId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProectKickOfId", (object)projectKickoffId ?? DBNull.Value);

                    await con.OpenAsync();

                    var ds = new DataSet();
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }

                    // Table 0: Master Rows
                    if (ds.Tables.Count > 0)
                    {
                        foreach (DataRow r in ds.Tables[0].Rows)
                        {
                            masterRows.Add(new OtherCostRegisterRowModel
                            {
                                OtherCostMasId = r["OtherCostMasId"] != DBNull.Value ? Convert.ToInt32(r["OtherCostMasId"]) : (int?)null,
                                ProectKickOfId = r["ProectKickOfId"] != DBNull.Value ? r["ProectKickOfId"].ToString() : "",
                                CostcenterId = r["CostcenterId"] != DBNull.Value ? r["CostcenterId"].ToString() : "",
                                Name = r["Name"] != DBNull.Value ? r["Name"].ToString() : "",
                                Type = r["Type"] != DBNull.Value ? r["Type"].ToString() : "Budget",
                                AllocationPer = r["AllocationPer"] != DBNull.Value ? Convert.ToDouble(r["AllocationPer"]) : 0,
                                AllocationAmount = r["AllocationAmount"] != DBNull.Value ? Convert.ToDouble(r["AllocationAmount"]) : 0,
                                BreakupAmount = r.Table.Columns.Contains("TotalAmount") && r["TotalAmount"] != DBNull.Value ? Convert.ToDouble(r["TotalAmount"]) : (r["BreakupAmount"] != DBNull.Value ? Convert.ToDouble(r["BreakupAmount"]) : 0),
                                TotalAmount = r.Table.Columns.Contains("TotalAmount") && r["TotalAmount"] != DBNull.Value ? Convert.ToDouble(r["TotalAmount"]) : (r["BreakupAmount"] != DBNull.Value ? Convert.ToDouble(r["BreakupAmount"]) : 0),
                                RemainingAmount = r["RemainingAmount"] != DBNull.Value ? Convert.ToDouble(r["RemainingAmount"]) : 0,
                                OtherCostBreakupId = r["OtherCostBreakupId"] != DBNull.Value ? Convert.ToInt32(r["OtherCostBreakupId"]) : (int?)null,
                                RefNo = r["RefNo"] != DBNull.Value ? Convert.ToDouble(r["RefNo"]) : (double?)null,
                                RefDate = r["RefDate"] != DBNull.Value ? Convert.ToDateTime(r["RefDate"]) : (DateTime?)null,
                                Remarks = r.Table.Columns.Contains("Remarks") && r["Remarks"] != DBNull.Value ? r["Remarks"].ToString() : "",
                                ApprovedId = r.Table.Columns.Contains("ApprovedId") && r["ApprovedId"] != DBNull.Value ? r["ApprovedId"].ToString() : "",
                                AprovedDateTime = r.Table.Columns.Contains("AprovedDateTime") && r["AprovedDateTime"] != DBNull.Value ? Convert.ToDateTime(r["AprovedDateTime"]) : (DateTime?)null,
                                DeptName = r["DeptName"] != DBNull.Value ? r["DeptName"].ToString() : "",
                                ServiceGroupId = r["ServiceGroupId"] != DBNull.Value ? r["ServiceGroupId"].ToString() : ""
                            });
                        }
                    }

                    // Table 1: Child Detail Lines
                    if (ds.Tables.Count > 1)
                    {
                        var childDict = new Dictionary<string, List<OtherCostBreakupItemModel>>();

                        foreach (DataRow r in ds.Tables[1].Rows)
                        {
                            string masId = r["OtherCostMasId"] != DBNull.Value ? r["OtherCostMasId"].ToString() : "";
                            if (!childDict.ContainsKey(masId))
                            {
                                childDict[masId] = new List<OtherCostBreakupItemModel>();
                            }

                            childDict[masId].Add(new OtherCostBreakupItemModel
                            {
                                OtherCostBkTranId = r["OtherCostBkTranId"] != DBNull.Value ? Convert.ToInt32(r["OtherCostBkTranId"]) : 0,
                                OtherCostBkAllocationId = r["OtherCostBkAllocationId"] != DBNull.Value ? r["OtherCostBkAllocationId"].ToString() : "",
                                OtherCostMasId = masId,
                                ServiceId = r["ServiceId"] != DBNull.Value ? r["ServiceId"].ToString() : "",
                                ServiceName = r["ServiceName"] != DBNull.Value ? r["ServiceName"].ToString() : "",
                                Description = r["Description"] != DBNull.Value ? r["Description"].ToString() : "",
                                UnitId = r["UnitId"] != DBNull.Value ? r["UnitId"].ToString() : "",
                                UnitName = r["UnitName"] != DBNull.Value ? r["UnitName"].ToString() : "Nos",
                                Qty = r["Qty"] != DBNull.Value ? Convert.ToDouble(r["Qty"]) : 1,
                                Rate = r["Rate"] != DBNull.Value ? Convert.ToDouble(r["Rate"]) : 0,
                                Amount = r["Amount"] != DBNull.Value ? Convert.ToDouble(r["Amount"]) : 0
                            });
                        }

                        foreach (var row in masterRows)
                        {
                            string mId = row.OtherCostMasId?.ToString() ?? "";
                            if (childDict.ContainsKey(mId))
                            {
                                row.Items = childDict[mId];
                            }
                        }
                    }
                }

                return Json(new { success = true, data = masterRows });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, data = new List<OtherCostRegisterRowModel>() });
            }
        }

        // GET: /OtherCostBreakup/GetAllocations?projectKickoffId=1
        // Loads available other cost allocation items for a project
        [HttpGet]
        public async Task<IActionResult> GetAllocations(string projectKickoffId)
        {
            var list = new List<OtherCostAllocationOptionModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_LOAD_BREAKUP, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Action", "FETCH_ALLOCATIONS");
                    cmd.Parameters.AddWithValue("@ProjectKickoffId", (object)projectKickoffId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProectKickOfId", (object)projectKickoffId ?? DBNull.Value);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow r in dt.Rows)
                        {
                            list.Add(new OtherCostAllocationOptionModel
                            {
                                OtherCostMasId = r["OtherCostMasId"] != DBNull.Value ? r["OtherCostMasId"].ToString() : "",
                                ProectKickOfId = r["ProectKickOfId"] != DBNull.Value ? r["ProectKickOfId"].ToString() : "",
                                CostName = r["CostName"] != DBNull.Value ? r["CostName"].ToString() : "",
                                DeptName = r["DeptName"] != DBNull.Value ? r["DeptName"].ToString() : "",
                                AllocatedAmount = r["AllocatedAmount"] != DBNull.Value ? Convert.ToDouble(r["AllocatedAmount"]) : 0,
                                AllocationPer = r["AllocationPer"] != DBNull.Value ? Convert.ToDouble(r["AllocationPer"]) : 0,
                                AlreadyBreakupAmount = r["AlreadyBreakupAmount"] != DBNull.Value ? Convert.ToDouble(r["AlreadyBreakupAmount"]) : 0,
                                RemainingToBreakup = r["RemainingToBreakup"] != DBNull.Value ? Convert.ToDouble(r["RemainingToBreakup"]) : 0,
                                ServiceGroupId = r["ServiceGroupId"] != DBNull.Value ? r["ServiceGroupId"].ToString() : "",
                                ServiceTypeId = r["ServiceTypeId"] != DBNull.Value ? r["ServiceTypeId"].ToString() : ""
                            });
                        }
                    }
                }

                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, data = new List<OtherCostAllocationOptionModel>() });
            }
        }

        // GET: /OtherCostBreakup/GetById?id=1
        // Loads breakup header + detail lines for editing
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                OtherCostBreakupModel header = null;
                var items = new List<OtherCostBreakupItemModel>();

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_LOAD_BREAKUP, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Action", "FETCH_BY_ID");
                    cmd.Parameters.AddWithValue("@OtherCostBreakupId", id);

                    await con.OpenAsync();

                    var ds = new DataSet();
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }

                    if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        var r = ds.Tables[0].Rows[0];
                        header = new OtherCostBreakupModel
                        {
                            OtherCostBreakupId = r["OtherCostBreakupId"] != DBNull.Value ? Convert.ToInt32(r["OtherCostBreakupId"]) : (int?)null,
                            CostcenterId = r["CostcenterId"] != DBNull.Value ? r["CostcenterId"].ToString() : "",
                            ProectKickOfId = r["ProectKickOfId"] != DBNull.Value ? r["ProectKickOfId"].ToString() : "",
                            OtherCostMasId = r["OtherCostMasId"] != DBNull.Value ? r["OtherCostMasId"].ToString() : "",
                            OtherCostName = r["OtherCostName"] != DBNull.Value ? r["OtherCostName"].ToString() : "",
                            RefDate = r["RefDate"] != DBNull.Value ? Convert.ToDateTime(r["RefDate"]) : (DateTime?)null,
                            RefNo = r["RefNo"] != DBNull.Value ? Convert.ToDouble(r["RefNo"]) : (double?)null,
                            Type = r["Type"] != DBNull.Value ? r["Type"].ToString() : "Budget",
                            RevisionStatus = r["RevisionStatus"] != DBNull.Value ? r["RevisionStatus"].ToString() : "No",
                            ServiceGroupId = r.Table.Columns.Contains("ServiceGroupId") && r["ServiceGroupId"] != DBNull.Value ? r["ServiceGroupId"].ToString() : "",
                            ServiceTypeId = r.Table.Columns.Contains("ServiceTypeId") && r["ServiceTypeId"] != DBNull.Value ? r["ServiceTypeId"].ToString() : "",
                            BreakupAmount = r.Table.Columns.Contains("TotalAmount") && r["TotalAmount"] != DBNull.Value ? Convert.ToDouble(r["TotalAmount"]) : (r["BreakupAmount"] != DBNull.Value ? Convert.ToDouble(r["BreakupAmount"]) : 0),
                            TotalAmount = r.Table.Columns.Contains("TotalAmount") && r["TotalAmount"] != DBNull.Value ? Convert.ToDouble(r["TotalAmount"]) : (r["BreakupAmount"] != DBNull.Value ? Convert.ToDouble(r["BreakupAmount"]) : 0),
                            AllocatedAmount = r["AllocatedAmount"] != DBNull.Value ? Convert.ToDouble(r["AllocatedAmount"]) : 0,
                            Remarks = r.Table.Columns.Contains("Remarks") && r["Remarks"] != DBNull.Value ? r["Remarks"].ToString() : "",
                            ApprovedId = r.Table.Columns.Contains("ApprovedId") && r["ApprovedId"] != DBNull.Value ? r["ApprovedId"].ToString() : "",
                            AprovedDateTime = r.Table.Columns.Contains("AprovedDateTime") && r["AprovedDateTime"] != DBNull.Value ? Convert.ToDateTime(r["AprovedDateTime"]) : (DateTime?)null,
                            RevisionId = r["RevisionId"] != DBNull.Value ? r["RevisionId"].ToString() : "0"
                        };
                    }

                    if (ds.Tables.Count > 1)
                    {
                        foreach (DataRow r in ds.Tables[1].Rows)
                        {
                            items.Add(new OtherCostBreakupItemModel
                            {
                                OtherCostBkTranId = r["OtherCostBkTranId"] != DBNull.Value ? Convert.ToInt32(r["OtherCostBkTranId"]) : 0,
                                OtherCostBkAllocationId = r["OtherCostBkAllocationId"] != DBNull.Value ? r["OtherCostBkAllocationId"].ToString() : "",
                                OtherCostMasId = r["OtherCostMasId"] != DBNull.Value ? r["OtherCostMasId"].ToString() : "",
                                ServiceId = r["ServiceId"] != DBNull.Value ? r["ServiceId"].ToString() : "",
                                ServiceName = r["ServiceName"] != DBNull.Value ? r["ServiceName"].ToString() : "",
                                Description = r["Description"] != DBNull.Value ? r["Description"].ToString() : "",
                                UnitId = r["UnitId"] != DBNull.Value ? r["UnitId"].ToString() : "",
                                UnitName = r["UnitName"] != DBNull.Value ? r["UnitName"].ToString() : "Nos",
                                Qty = r["Qty"] != DBNull.Value ? Convert.ToDouble(r["Qty"]) : 1,
                                Rate = r["Rate"] != DBNull.Value ? Convert.ToDouble(r["Rate"]) : 0,
                                Amount = r["Amount"] != DBNull.Value ? Convert.ToDouble(r["Amount"]) : 0
                            });
                        }
                    }
                }

                if (header == null)
                    return Json(new { success = false, message = "Breakup record not found." });

                header.Items = items;
                return Json(new { success = true, header = header, items = items });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /OtherCostBreakup/GetServices
        // Active services from Project_ServiceMas
        // GET: /OtherCostBreakup/GetServices
        // Active services from Project_ServiceMas via stored procedure Web_SaveProjectServiceMas
        [HttpGet]
        public async Task<IActionResult> GetServices()
        {
            var list = new List<ServiceModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_SERVICE_MAS, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Action", "Fetch");

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow r in dt.Rows)
                        {
                            list.Add(new ServiceModel
                            {
                                ServiceId = r["ServiceId"] != DBNull.Value ? Convert.ToInt32(r["ServiceId"]) : 0,
                                Servicecode = r["Servicecode"] != DBNull.Value ? r["Servicecode"].ToString() : "",
                                ServiceName = r["ServiceName"] != DBNull.Value ? r["ServiceName"].ToString() : "",
                                ServiceTypeId = r["ServiceTypeId"] != DBNull.Value ? r["ServiceTypeId"].ToString() : "",
                                UnitId = r["UnitId"] != DBNull.Value ? r["UnitId"].ToString() : ""
                            });
                        }
                    }
                }

                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, data = new List<ServiceModel>() });
            }
        }

        // GET: /OtherCostBreakup/GetUOMList
        // Always load units from UOM table via stored procedure dbo.Web_GetUOMList
        [HttpGet]
        public async Task<IActionResult> GetUOMList()
        {
            var list = new List<UOMModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("dbo.Web_GetUOMList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow r in dt.Rows)
                        {
                            list.Add(new UOMModel
                            {
                                UnitId = r["UnitId"] != DBNull.Value ? Convert.ToInt32(r["UnitId"]) : 0,
                                UnitName = r["UnitName"] != DBNull.Value ? r["UnitName"].ToString() : ""
                            });
                        }
                    }
                }

                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, data = new List<UOMModel>() });
            }
        }

        // GET: /OtherCostBreakup/GetServiceTypes
        // Load service types via stored procedure dbo.Web_SaveProjectServiceTypeMas @Action = 'FETCH'
        [HttpGet]
        public async Task<IActionResult> GetServiceTypes()
        {
            var list = new List<ServiceTypeMasterModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("dbo.Web_SaveProjectServiceTypeMas", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Action", "FETCH");

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow r in dt.Rows)
                        {
                            list.Add(new ServiceTypeMasterModel
                            {
                                ServiceTypeId = r["ServiceTypeId"] != DBNull.Value ? Convert.ToInt32(r["ServiceTypeId"]) : 0,
                                ServiceTypeName = r["ServiceTypeName"] != DBNull.Value ? r["ServiceTypeName"].ToString() : "",
                                Status = r["Status"] != DBNull.Value ? r["Status"].ToString() : "1"
                            });
                        }
                    }
                }

                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, data = new List<ServiceTypeMasterModel>() });
            }
        }

        // POST: /OtherCostBreakup/Save
        // Upserts into Project_BOQ_OtherCostBreakupAllocationMas & Project_BOQ_OtherCostBreakupTransaction
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] OtherCostBreakupModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired. Please log in again." });

                if (req == null)
                    return Json(new { success = false, message = "Invalid breakup payload." });

                if (string.IsNullOrWhiteSpace(req.ProectKickOfId))
                    return Json(new { success = false, message = "Please select a Project." });

                if (string.IsNullOrWhiteSpace(req.OtherCostMasId))
                    return Json(new { success = false, message = "Please select an Other Cost Allocation reference." });

                if (req.Items == null || req.Items.Count == 0)
                    return Json(new { success = false, message = "Please enter at least one breakup line item." });

                double totalBreakup = req.Items.Sum(x => x.Amount ?? ((x.Qty ?? 1) * (x.Rate ?? 0)));
                if (totalBreakup <= 0)
                    return Json(new { success = false, message = "Total breakup amount must be greater than zero." });

                string ipAddress = SessionHelper.GetClientIPAddress(Request);
                string hostName = SessionHelper.GetClientHostName(ipAddress);

                int breakupId = 0;
                int result = 0;
                string message = "";

                using (var con = new SqlConnection(_connPROJ))
                {
                    await con.OpenAsync();

                    // Auto-resolve or insert services in Project_ServiceMas for any line without ServiceId
                    foreach (var item in req.Items)
                    {
                        string desc = !string.IsNullOrWhiteSpace(item.Description)
                            ? item.Description.Trim()
                            : (!string.IsNullOrWhiteSpace(item.ServiceName) ? item.ServiceName.Trim() : "");

                        if ((string.IsNullOrWhiteSpace(item.ServiceId) || item.ServiceId == "0") && !string.IsNullOrWhiteSpace(desc))
                        {
                            int existingSvcId = 0;
                            using (var chkCmd = new SqlCommand(SP_SERVICE_MAS, con))
                            {
                                chkCmd.CommandType = CommandType.StoredProcedure;
                                chkCmd.Parameters.AddWithValue("@Action", "FindByName");
                                chkCmd.Parameters.AddWithValue("@ServiceName", desc);
                                var found = await chkCmd.ExecuteScalarAsync();
                                if (found != null && found != DBNull.Value)
                                    existingSvcId = Convert.ToInt32(found);
                            }

                            if (existingSvcId == 0)
                            {
                                using (var insCmd = new SqlCommand(SP_SERVICE_MAS, con))
                                {
                                    insCmd.CommandType = CommandType.StoredProcedure;
                                    insCmd.Parameters.AddWithValue("@Action", "Insert");
                                    insCmd.Parameters.AddWithValue("@ServiceName", desc);
                                    insCmd.Parameters.AddWithValue("@ServiceTypeId", "1");
                                    insCmd.Parameters.AddWithValue("@UnitId", (object)item.UnitId ?? "4");
                                    insCmd.Parameters.AddWithValue("@CreatedBy", user.UserId ?? "User");
                                    insCmd.Parameters.AddWithValue("@Status", "1");
                                    using (var dr = await insCmd.ExecuteReaderAsync())
                                    {
                                        if (await dr.ReadAsync() && dr["ServiceId"] != DBNull.Value)
                                        {
                                            existingSvcId = Convert.ToInt32(dr["ServiceId"]);
                                        }
                                    }
                                }
                            }

                            if (existingSvcId > 0)
                                item.ServiceId = existingSvcId.ToString();
                        }
                    }

                    string jsonLines = JsonSerializer.Serialize(req.Items.Select(x => new
                    {
                        serviceId = x.ServiceId ?? "",
                        serviceTypeId = !string.IsNullOrWhiteSpace(x.ServiceTypeId) ? x.ServiceTypeId : "1",
                        unitId = x.UnitId ?? "4",
                        qty = x.Qty ?? 1,
                        rate = x.Rate ?? x.Amount ?? 0,
                        amount = x.Amount ?? ((x.Qty ?? 1) * (x.Rate ?? 0))
                    }));

                    using (var cmd = new SqlCommand(SP_SAVE_BREAKUP, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 60;
                        cmd.Parameters.AddWithValue("@Action", (req.OtherCostBreakupId.HasValue && req.OtherCostBreakupId.Value > 0) ? "UPDATE" : "INSERT");
                        cmd.Parameters.AddWithValue("@OtherCostBreakupId", (object)req.OtherCostBreakupId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CostcenterId", (object)req.CostcenterId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ProectKickOfId", req.ProectKickOfId);
                        cmd.Parameters.AddWithValue("@OtherCostMasId", req.OtherCostMasId);
                        cmd.Parameters.AddWithValue("@RefDate", req.RefDate.HasValue ? (object)req.RefDate.Value : DateTime.Now);
                        cmd.Parameters.AddWithValue("@RefNo", req.RefNo.HasValue ? (object)req.RefNo.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Type", req.Type ?? "Budget");
                        cmd.Parameters.AddWithValue("@RevisionStatus", req.RevisionStatus ?? "No");
                        cmd.Parameters.AddWithValue("@BreakupAmount", totalBreakup);
                        cmd.Parameters.AddWithValue("@TotalAmount", totalBreakup);
                        cmd.Parameters.AddWithValue("@Remarks", (object)req.Remarks ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ApprovedId", req.ReadyForApproval ? (object)(user.UserId ?? "User") : DBNull.Value);
                        cmd.Parameters.AddWithValue("@AprovedDateTime", req.ReadyForApproval ? (object)DateTime.Now : DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedBy", user.UserId ?? "User");
                        cmd.Parameters.AddWithValue("@UpdatedBy", user.UserId ?? "User");
                        cmd.Parameters.AddWithValue("@IPAddress", (object)ipAddress ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@HostName", (object)hostName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DetailJson", jsonLines);

                        using (var dr = await cmd.ExecuteReaderAsync())
                        {
                            if (await dr.ReadAsync())
                            {
                                result = dr["Result"] != DBNull.Value ? Convert.ToInt32(dr["Result"]) : 0;
                                message = dr["Message"] != DBNull.Value ? dr["Message"].ToString() : "";
                                if (dr.FieldCount > 2 && dr["OtherCostBreakupId"] != DBNull.Value)
                                {
                                    int.TryParse(dr["OtherCostBreakupId"].ToString(), out breakupId);
                                }
                            }
                        }
                    }
                }

                if (result == 1)
                {
                    return Json(new { success = true, message = message, breakupId = breakupId, breakupAmount = totalBreakup });
                }
                else
                {
                    return Json(new { success = false, message = !string.IsNullOrEmpty(message) ? message : "Failed to save breakup." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /OtherCostBreakup/SaveBatch
        // Saves all allocation breakups for a project in one transaction
        [HttpPost]
        public async Task<IActionResult> SaveBatch([FromBody] OtherCostBreakupBatchRequest req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired. Please log in again." });

                if (req == null)
                    return Json(new { success = false, message = "Invalid breakup payload." });

                if (string.IsNullOrWhiteSpace(req.ProectKickOfId))
                    return Json(new { success = false, message = "Please select a Project." });

                if (req.Breakups == null || req.Breakups.Count == 0)
                    return Json(new { success = false, message = "No breakup records provided." });

                string ipAddress = SessionHelper.GetClientIPAddress(Request);
                string hostName = SessionHelper.GetClientHostName(ipAddress);

                int savedCount = 0;

                using (var con = new SqlConnection(_connPROJ))
                {
                    await con.OpenAsync();

                    foreach (var alloc in req.Breakups)
                    {
                        if (string.IsNullOrWhiteSpace(alloc.OtherCostMasId))
                            continue;

                        // Filter out empty lines
                        var activeLines = alloc.Items?
                            .Where(x => (!string.IsNullOrWhiteSpace(x.Description) || !string.IsNullOrWhiteSpace(x.ServiceName)) &&
                                        ((x.Amount ?? 0) > 0 || ((x.Qty ?? 0) > 0 && (x.Rate ?? 0) > 0)))
                            .ToList() ?? new List<OtherCostBreakupItemModel>();

                        // If it has no lines and no prior breakup ID, skip
                        if (activeLines.Count == 0 && (!alloc.OtherCostBreakupId.HasValue || alloc.OtherCostBreakupId.Value <= 0))
                            continue;

                        // Auto-resolve or insert services in Project_ServiceMas for any line without ServiceId
                        foreach (var item in activeLines)
                        {
                            string desc = !string.IsNullOrWhiteSpace(item.Description)
                                ? item.Description.Trim()
                                : (!string.IsNullOrWhiteSpace(item.ServiceName) ? item.ServiceName.Trim() : "");

                            if ((string.IsNullOrWhiteSpace(item.ServiceId) || item.ServiceId == "0") && !string.IsNullOrWhiteSpace(desc))
                            {
                                int existingSvcId = 0;
                                using (var chkCmd = new SqlCommand(SP_SERVICE_MAS, con))
                                {
                                    chkCmd.CommandType = CommandType.StoredProcedure;
                                    chkCmd.Parameters.AddWithValue("@Action", "FindByName");
                                    chkCmd.Parameters.AddWithValue("@ServiceName", desc);
                                    var found = await chkCmd.ExecuteScalarAsync();
                                    if (found != null && found != DBNull.Value)
                                        existingSvcId = Convert.ToInt32(found);
                                }

                                if (existingSvcId == 0)
                                {
                                    using (var insCmd = new SqlCommand(SP_SERVICE_MAS, con))
                                    {
                                        insCmd.CommandType = CommandType.StoredProcedure;
                                        insCmd.Parameters.AddWithValue("@Action", "Insert");
                                        insCmd.Parameters.AddWithValue("@ServiceName", desc);
                                        insCmd.Parameters.AddWithValue("@ServiceTypeId", "1");
                                        insCmd.Parameters.AddWithValue("@UnitId", (object)item.UnitId ?? "4");
                                        insCmd.Parameters.AddWithValue("@CreatedBy", user.UserId ?? "User");
                                        insCmd.Parameters.AddWithValue("@Status", "1");
                                        using (var dr = await insCmd.ExecuteReaderAsync())
                                        {
                                            if (await dr.ReadAsync() && dr["ServiceId"] != DBNull.Value)
                                            {
                                                existingSvcId = Convert.ToInt32(dr["ServiceId"]);
                                            }
                                        }
                                    }
                                }

                                if (existingSvcId > 0)
                                    item.ServiceId = existingSvcId.ToString();
                            }
                        }

                        double totalBreakup = activeLines.Sum(x => x.Amount ?? ((x.Qty ?? 1) * (x.Rate ?? 0)));

                        string jsonLines = JsonSerializer.Serialize(activeLines.Select(x => new
                        {
                            serviceId = x.ServiceId ?? "",
                            serviceTypeId = !string.IsNullOrWhiteSpace(x.ServiceTypeId) ? x.ServiceTypeId : "1",
                            unitId = x.UnitId ?? "4",
                            qty = x.Qty ?? 1,
                            rate = x.Rate ?? x.Amount ?? 0,
                            amount = x.Amount ?? ((x.Qty ?? 1) * (x.Rate ?? 0))
                        }));

                        using (var cmd = new SqlCommand(SP_SAVE_BREAKUP, con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandTimeout = 60;
                            cmd.Parameters.AddWithValue("@Action", (alloc.OtherCostBreakupId.HasValue && alloc.OtherCostBreakupId.Value > 0) ? "UPDATE" : "INSERT");
                            cmd.Parameters.AddWithValue("@OtherCostBreakupId", (object)alloc.OtherCostBreakupId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CostcenterId", (object)req.CostcenterId ?? (object)alloc.CostcenterId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ProectKickOfId", req.ProectKickOfId);
                            cmd.Parameters.AddWithValue("@OtherCostMasId", alloc.OtherCostMasId);
                            cmd.Parameters.AddWithValue("@RefDate", req.RefDate.HasValue ? (object)req.RefDate.Value : DateTime.Now);
                            cmd.Parameters.AddWithValue("@RefNo", req.RefNo.HasValue ? (object)req.RefNo.Value : DBNull.Value);
                            cmd.Parameters.AddWithValue("@Type", req.Type ?? "Budget");
                            cmd.Parameters.AddWithValue("@RevisionStatus", req.RevisionStatus ?? "No");
                            cmd.Parameters.AddWithValue("@BreakupAmount", totalBreakup);
                            cmd.Parameters.AddWithValue("@TotalAmount", totalBreakup);
                            cmd.Parameters.AddWithValue("@Remarks", (object)(req.Remarks ?? alloc.Remarks) ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ApprovedId", (req.ReadyForApproval || alloc.ReadyForApproval) ? (object)(user.UserId ?? "User") : DBNull.Value);
                            cmd.Parameters.AddWithValue("@AprovedDateTime", (req.ReadyForApproval || alloc.ReadyForApproval) ? (object)DateTime.Now : DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedBy", user.UserId ?? "User");
                            cmd.Parameters.AddWithValue("@UpdatedBy", user.UserId ?? "User");
                            cmd.Parameters.AddWithValue("@IPAddress", (object)ipAddress ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@HostName", (object)hostName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@DetailJson", jsonLines);

                            using (var dr = await cmd.ExecuteReaderAsync())
                            {
                                if (await dr.ReadAsync())
                                {
                                    int resCode = dr["Result"] != DBNull.Value ? Convert.ToInt32(dr["Result"]) : 0;
                                    if (resCode == 1) savedCount++;
                                }
                            }
                        }
                    }
                }

                return Json(new { success = true, message = $"Saved {savedCount} Other Cost Breakup item(s) successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /OtherCostBreakup/Delete
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] OtherCostBreakupModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired. Please log in again." });

                if (!req.OtherCostBreakupId.HasValue || req.OtherCostBreakupId.Value <= 0)
                    return Json(new { success = false, message = "Invalid breakup record ID." });

                string ipAddress = SessionHelper.GetClientIPAddress(Request);
                string hostName = SessionHelper.GetClientHostName(ipAddress);

                int result = 0;
                string message = "";

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_DELETE_BREAKUP, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@OtherCostBreakupId", req.OtherCostBreakupId.Value);
                    cmd.Parameters.AddWithValue("@UpdatedBy", user.UserId ?? "User");
                    cmd.Parameters.AddWithValue("@IPAddress", (object)ipAddress ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HostName", (object)hostName ?? DBNull.Value);

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            result = dr["Result"] != DBNull.Value ? Convert.ToInt32(dr["Result"]) : 0;
                            message = dr["Message"] != DBNull.Value ? dr["Message"].ToString() : "";
                        }
                    }
                }

                return Json(new { success = result == 1, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /OtherCostBreakup/SaveService
        // Quick-adds a new service to Project_ServiceMas via Web_SaveProjectServiceMas
        [HttpPost]
        public async Task<IActionResult> SaveService([FromBody] ServiceModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired. Please log in again." });

                if (string.IsNullOrWhiteSpace(req?.ServiceName))
                    return Json(new { success = false, message = "Service Name cannot be blank." });

                string code = !string.IsNullOrWhiteSpace(req.Servicecode)
                    ? req.Servicecode.Trim()
                    : "SVC-" + DateTime.Now.Ticks.ToString().Substring(10);

                string ipAddress = SessionHelper.GetClientIPAddress(Request);
                string hostName = SessionHelper.GetClientHostName(ipAddress);

                int result = 0;
                string message = "";
                int newServiceId = 0;

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_SERVICE_MAS, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Action", "Insert");
                    cmd.Parameters.AddWithValue("@Servicecode", code);
                    cmd.Parameters.AddWithValue("@ServiceName", req.ServiceName.Trim());
                    cmd.Parameters.AddWithValue("@ServiceTypeId", string.IsNullOrWhiteSpace(req.ServiceTypeId) ? "1" : req.ServiceTypeId);
                    cmd.Parameters.AddWithValue("@UnitId", (object)req.UnitId ?? "4");
                    cmd.Parameters.AddWithValue("@CreatedBy", user.UserId ?? "User");
                    cmd.Parameters.AddWithValue("@IPAddress", (object)ipAddress ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HostName", (object)hostName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", "1");

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            result = dr["Result"] != DBNull.Value ? Convert.ToInt32(dr["Result"]) : 0;
                            message = dr["Message"] != DBNull.Value ? dr["Message"].ToString() : "";
                            for (int i = 0; i < dr.FieldCount; i++)
                            {
                                if (string.Equals(dr.GetName(i), "ServiceId", StringComparison.OrdinalIgnoreCase) && dr[i] != DBNull.Value)
                                {
                                    int.TryParse(dr[i].ToString(), out newServiceId);
                                    break;
                                }
                            }
                        }
                    }
                }

                return Json(new { success = result == 1, serviceId = newServiceId, serviceName = req.ServiceName.Trim(), message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

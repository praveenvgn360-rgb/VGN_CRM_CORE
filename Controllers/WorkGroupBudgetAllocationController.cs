using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;
using VGN_CRM_CORE.Models;

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    public class WorkGroupBudgetAllocationController : Controller
    {
        private readonly string _connPROJ;

        public WorkGroupBudgetAllocationController(IConfiguration configuration)
        {
            _connPROJ = configuration.GetActiveConnectionString("connPROJ")
                        ?? configuration.GetConnectionString("ConnPROJ")
                        ?? configuration.GetConnectionString("connPROJ");
        }

        // GET: /WorkGroupBudgetAllocation/Index
        [HttpGet]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.Title = "Work Group Budget Allocation — VGN ERP";
            ViewBag.UserId = user.UserId;
            ViewBag.UserName = user.UserName;
            return View();
        }

        // GET: /WorkGroupBudgetAllocation/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = new List<WorkGroupBudgetAllocationModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("Web_SaveWorkGroupBudgetAllocation", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "FETCHALL");
                    cmd.Parameters.AddWithValue("@WorkGroupBudgetId", DBNull.Value);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(new WorkGroupBudgetAllocationModel
                            {
                                WorkGroupBudgetId = row["WorkGroupBudgetId"] != DBNull.Value ? Convert.ToInt32(row["WorkGroupBudgetId"]) : (int?)null,
                                ProjectKickoffId = row["ProjectKickoffId"] != DBNull.Value ? Convert.ToInt32(row["ProjectKickoffId"]) : (int?)null,
                                CostCentreId = row["CostCentreId"] != DBNull.Value ? Convert.ToInt32(row["CostCentreId"]) : (int?)null,
                                BudgetAllocationId = row["BudgetAllocationId"] != DBNull.Value ? Convert.ToInt32(row["BudgetAllocationId"]) : (int?)null,
                                WorkGroupId = row["WorkGroupId"] != DBNull.Value ? Convert.ToInt32(row["WorkGroupId"]) : (int?)null,
                                WorkTypeId = row["WorkTypeId"] != DBNull.Value ? Convert.ToInt32(row["WorkTypeId"]) : (int?)null,
                                WorkGroupBudgetAmount = row["WorkGroupBudgetAmount"] != DBNull.Value ? Convert.ToDouble(row["WorkGroupBudgetAmount"]) : (double?)null,
                                WorkGroupBudgetDescription = row["WorkGroupBudgetDescription"] != DBNull.Value ? row["WorkGroupBudgetDescription"].ToString() : "",
                                CreatedBy = row.Table.Columns.Contains("CreatedBy") && row["CreatedBy"] != DBNull.Value ? row["CreatedBy"].ToString() : null,
                                CreatedDate = row.Table.Columns.Contains("CreatedDate") && row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : (DateTime?)null,
                                UpdatedBy = row.Table.Columns.Contains("UpdatedBy") && row["UpdatedBy"] != DBNull.Value ? row["UpdatedBy"].ToString() : null,
                                UpdatedDate = row.Table.Columns.Contains("UpdatedDate") && row["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedDate"]) : (DateTime?)null,
                                Status = "1"
                            });
                        }
                    }
                }

                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, data = list });
            }
        }

        // GET: /WorkGroupBudgetAllocation/GetById?id=1
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("Web_SaveWorkGroupBudgetAllocation", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "FETCHBYID");
                    cmd.Parameters.AddWithValue("@WorkGroupBudgetId", id);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            var row = dt.Rows[0];
                            var model = new WorkGroupBudgetAllocationModel
                            {
                                WorkGroupBudgetId = row["WorkGroupBudgetId"] != DBNull.Value ? Convert.ToInt32(row["WorkGroupBudgetId"]) : (int?)null,
                                ProjectKickoffId = row["ProjectKickoffId"] != DBNull.Value ? Convert.ToInt32(row["ProjectKickoffId"]) : (int?)null,
                                CostCentreId = row["CostCentreId"] != DBNull.Value ? Convert.ToInt32(row["CostCentreId"]) : (int?)null,
                                BudgetAllocationId = row["BudgetAllocationId"] != DBNull.Value ? Convert.ToInt32(row["BudgetAllocationId"]) : (int?)null,
                                WorkGroupId = row["WorkGroupId"] != DBNull.Value ? Convert.ToInt32(row["WorkGroupId"]) : (int?)null,
                                WorkTypeId = row["WorkTypeId"] != DBNull.Value ? Convert.ToInt32(row["WorkTypeId"]) : (int?)null,
                                WorkGroupBudgetAmount = row["WorkGroupBudgetAmount"] != DBNull.Value ? Convert.ToDouble(row["WorkGroupBudgetAmount"]) : (double?)null,
                                WorkGroupBudgetDescription = row["WorkGroupBudgetDescription"] != DBNull.Value ? row["WorkGroupBudgetDescription"].ToString() : "",
                                CreatedBy = row.Table.Columns.Contains("CreatedBy") && row["CreatedBy"] != DBNull.Value ? row["CreatedBy"].ToString() : null,
                                CreatedDate = row.Table.Columns.Contains("CreatedDate") && row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : (DateTime?)null,
                                UpdatedBy = row.Table.Columns.Contains("UpdatedBy") && row["UpdatedBy"] != DBNull.Value ? row["UpdatedBy"].ToString() : null,
                                UpdatedDate = row.Table.Columns.Contains("UpdatedDate") && row["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedDate"]) : (DateTime?)null,
                                Status = "1"
                            };

                            return Json(new { success = true, data = model });
                        }
                    }
                }

                return Json(new { success = false, message = "Record not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /WorkGroupBudgetAllocation/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] WorkGroupBudgetAllocationModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired. Please log in again." });

                if (req == null)
                    return Json(new { success = false, message = "Invalid data received." });

                string ipAddress = SessionHelper.GetClientIPAddress(Request);
                string hostName = SessionHelper.GetClientHostName(ipAddress);

                bool isInsert = !req.WorkGroupBudgetId.HasValue || req.WorkGroupBudgetId.Value <= 0;
                string flag = isInsert ? "INSERT" : "UPDATE";

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("Web_SaveWorkGroupBudgetAllocation", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", flag);
                    cmd.Parameters.AddWithValue("@WorkGroupBudgetId", (object)(req.WorkGroupBudgetId ?? 0));
                    cmd.Parameters.AddWithValue("@ProjectKickoffId", (object)(req.ProjectKickoffId ?? 0));
                    cmd.Parameters.AddWithValue("@CostCentreId", (object)(req.CostCentreId ?? 0));
                    cmd.Parameters.AddWithValue("@BudgetAllocationId", (object)(req.BudgetAllocationId ?? 0));
                    cmd.Parameters.AddWithValue("@WorkGroupId", (object)(req.WorkGroupId ?? 0));
                    cmd.Parameters.AddWithValue("@WorkTypeId", (object)(req.WorkTypeId ?? 0));
                    cmd.Parameters.AddWithValue("@WorkGroupBudgetAmount", (object)(req.WorkGroupBudgetAmount ?? 0.0));
                    cmd.Parameters.AddWithValue("@WorkGroupBudgetDescription", (object)(req.WorkGroupBudgetDescription?.Trim() ?? ""));

                    cmd.Parameters.AddWithValue("@CreatedBy", isInsert ? (object)(user.UserId ?? "User") : DBNull.Value);
                    cmd.Parameters.AddWithValue("@UpdatedBy", !isInsert ? (object)(user.UserId ?? "User") : DBNull.Value);

                    cmd.Parameters.AddWithValue("@IPAddress", (object)ipAddress ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HostName", (object)hostName ?? DBNull.Value);

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            string action = dr["Action"] != DBNull.Value ? dr["Action"].ToString() : "";
                            string message = dr.FieldCount > 2 && dr["Message"] != DBNull.Value ? dr["Message"].ToString() : "";
                            int savedId = dr["WorkGroupBudgetId"] != DBNull.Value ? Convert.ToInt32(dr["WorkGroupBudgetId"]) : 0;

                            if (action.Equals("INSERT", StringComparison.OrdinalIgnoreCase) ||
                                action.Equals("UPDATE", StringComparison.OrdinalIgnoreCase))
                            {
                                return Json(new
                                {
                                    success = true,
                                    message = !string.IsNullOrEmpty(message) ? message : (isInsert ? "Work group budget allocation created successfully!" : "Work group budget allocation updated successfully!"),
                                    id = savedId
                                });
                            }
                            else
                            {
                                return Json(new { success = false, message = !string.IsNullOrEmpty(message) ? message : action });
                            }
                        }
                    }
                }

                return Json(new { success = false, message = "No response from stored procedure." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /WorkGroupBudgetAllocation/Delete
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] WorkGroupBudgetAllocationModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired." });

                if (req == null || !req.WorkGroupBudgetId.HasValue)
                    return Json(new { success = false, message = "Invalid Work Group Budget ID." });

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("UPDATE dbo.WorkGroupBudgetAllocation SET Status = '0', UpdatedBy = @UpdatedBy, UpdatedDate = GETDATE() WHERE WorkGroupBudgetId = @Id", con))
                {
                    cmd.Parameters.AddWithValue("@Id", req.WorkGroupBudgetId.Value);
                    cmd.Parameters.AddWithValue("@UpdatedBy", user.UserId ?? "User");

                    await con.OpenAsync();
                    int rows = await cmd.ExecuteNonQueryAsync();

                    return Json(new { success = rows > 0, message = rows > 0 ? "Work group budget allocation deleted successfully." : "Record not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /WorkGroupBudgetAllocation/GetLookups
        [HttpGet]
        public async Task<IActionResult> GetLookups()
        {
            var projects = new List<object>();
            var workGroups = new List<object>();
            var workTypes = new List<object>();
            var budgetAllocations = new List<object>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                {
                    await con.OpenAsync();

                    // Projects
                    try
                    {
                        using (var cmd = new SqlCommand("Web_SaveProjectKickOffMas", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Action", "FETCH");
                            cmd.Parameters.AddWithValue("@ProjectKickoffId", DBNull.Value);
                            using (var da = new SqlDataAdapter(cmd))
                            {
                                var dt = new DataTable();
                                da.Fill(dt);
                                foreach (DataRow row in dt.Rows)
                                {
                                    projects.Add(new
                                    {
                                        Id = row["ProjectKickoffId"] != DBNull.Value ? Convert.ToInt32(row["ProjectKickoffId"]) : 0,
                                        Name = row["ProjectName"] != DBNull.Value ? row["ProjectName"].ToString() : ""
                                    });
                                }
                            }
                        }
                    }
                    catch { }

                    // Work Groups
                    try
                    {
                        using (var cmd = new SqlCommand("Web_SaveWorkGroupMas", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Flag", "FetchbyAll");
                            cmd.Parameters.AddWithValue("@WorkGroupId", DBNull.Value);
                            using (var da = new SqlDataAdapter(cmd))
                            {
                                var dt = new DataTable();
                                da.Fill(dt);
                                foreach (DataRow row in dt.Rows)
                                {
                                    workGroups.Add(new
                                    {
                                        Id = row["WorkGroupId"] != DBNull.Value ? Convert.ToInt32(row["WorkGroupId"]) : 0,
                                        Name = row["WorkGroupName"] != DBNull.Value ? row["WorkGroupName"].ToString() : ""
                                    });
                                }
                            }
                        }
                    }
                    catch { }

                    // Work Types
                    try
                    {
                        using (var cmd = new SqlCommand("Web_SaveWorkType", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Flag", "FetchByAll");
                            cmd.Parameters.AddWithValue("@WorkTypeId", DBNull.Value);
                            using (var da = new SqlDataAdapter(cmd))
                            {
                                var dt = new DataTable();
                                da.Fill(dt);
                                foreach (DataRow row in dt.Rows)
                                {
                                    workTypes.Add(new
                                    {
                                        Id = row["WorkTypeId"] != DBNull.Value ? Convert.ToInt32(row["WorkTypeId"]) : 0,
                                        Name = row["WorkTypeName"] != DBNull.Value ? row["WorkTypeName"].ToString() : ""
                                    });
                                }
                            }
                        }
                    }
                    catch { }

                    // Budget Allocations
                    try
                    {
                        using (var cmd = new SqlCommand("SELECT BudgetAllocationId, BudgetDescription, BudgetAmount FROM dbo.ProjectBudgetAllocation WHERE Status = 1", con))
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);
                            foreach (DataRow row in dt.Rows)
                            {
                                budgetAllocations.Add(new
                                {
                                    Id = row["BudgetAllocationId"] != DBNull.Value ? Convert.ToInt32(row["BudgetAllocationId"]) : 0,
                                    Name = (row["BudgetDescription"] != DBNull.Value ? row["BudgetDescription"].ToString() : "") +
                                           (row["BudgetAmount"] != DBNull.Value ? $" (₹ {Convert.ToDouble(row["BudgetAmount"]):N0})" : "")
                                });
                            }
                        }
                    }
                    catch { }
                }

                return Json(new
                {
                    success = true,
                    projects,
                    workGroups,
                    workTypes,
                    budgetAllocations
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

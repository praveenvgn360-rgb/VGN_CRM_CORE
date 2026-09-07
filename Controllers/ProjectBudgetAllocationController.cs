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
    public class ProjectBudgetAllocationController : Controller
    {
        private readonly string _connPROJ;

        public ProjectBudgetAllocationController(IConfiguration configuration)
        {
            _connPROJ = configuration.GetActiveConnectionString("connPROJ")
                        ?? configuration.GetConnectionString("ConnPROJ")
                        ?? configuration.GetConnectionString("connPROJ");
        }

        // GET: /ProjectBudgetAllocation/Index
        [HttpGet]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.Title = "Project Budget Allocation — VGN ERP";
            ViewBag.UserId = user.UserId;
            ViewBag.UserName = user.UserName;
            return View();
        }

        // GET: /ProjectBudgetAllocation/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = new List<ProjectBudgetAllocationModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("Web_SaveProjectBudgetAllocation", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "FetchbyAll");
                    cmd.Parameters.AddWithValue("@BudgetAllocationId", DBNull.Value);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        // If SP returned 0 rows because of unpatched WHERE clause (BudgetAllocationId = @BudgetAllocationId),
                        // fall back to direct select of active rows to guarantee user sees data immediately.
                        if (dt.Rows.Count == 0)
                        {
                            using (var fallbackCmd = new SqlCommand(
                                "SELECT BudgetAllocationId, ProjectKickoffId, CostCentreId, BudgetAmount, BudgetDescription, " +
                                "CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, IPAddress, HostName, Status " +
                                "FROM dbo.ProjectBudgetAllocation WHERE Status = 1 ORDER BY BudgetAllocationId DESC", con))
                            using (var fallbackDa = new SqlDataAdapter(fallbackCmd))
                            {
                                dt.Clear();
                                fallbackDa.Fill(dt);
                            }
                        }

                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(new ProjectBudgetAllocationModel
                            {
                                BudgetAllocationId = row["BudgetAllocationId"] != DBNull.Value ? Convert.ToInt32(row["BudgetAllocationId"]) : (int?)null,
                                ProjectKickoffId = row["ProjectKickoffId"] != DBNull.Value ? Convert.ToInt32(row["ProjectKickoffId"]) : (int?)null,
                                CostCentreId = row["CostCentreId"] != DBNull.Value ? Convert.ToInt32(row["CostCentreId"]) : (int?)null,
                                BudgetAmount = row["BudgetAmount"] != DBNull.Value ? Convert.ToDouble(row["BudgetAmount"]) : (double?)null,
                                BudgetDescription = row["BudgetDescription"] != DBNull.Value ? row["BudgetDescription"].ToString() : null,
                                CreatedBy = row["CreatedBy"] != DBNull.Value ? row["CreatedBy"].ToString() : null,
                                CreatedDate = row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : (DateTime?)null,
                                UpdatedBy = row["UpdatedBy"] != DBNull.Value ? row["UpdatedBy"].ToString() : null,
                                UpdatedDate = row["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedDate"]) : (DateTime?)null,
                                Status = row["Status"] != DBNull.Value ? row["Status"].ToString() : "1"
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

        // GET: /ProjectBudgetAllocation/GetById?id=1
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("Web_SaveProjectBudgetAllocation", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "FetchbyId");
                    cmd.Parameters.AddWithValue("@BudgetAllocationId", id);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            var row = dt.Rows[0];
                            var model = new ProjectBudgetAllocationModel
                            {
                                BudgetAllocationId = row["BudgetAllocationId"] != DBNull.Value ? Convert.ToInt32(row["BudgetAllocationId"]) : (int?)null,
                                ProjectKickoffId = row["ProjectKickoffId"] != DBNull.Value ? Convert.ToInt32(row["ProjectKickoffId"]) : (int?)null,
                                CostCentreId = row["CostCentreId"] != DBNull.Value ? Convert.ToInt32(row["CostCentreId"]) : (int?)null,
                                BudgetAmount = row["BudgetAmount"] != DBNull.Value ? Convert.ToDouble(row["BudgetAmount"]) : (double?)null,
                                BudgetDescription = row["BudgetDescription"] != DBNull.Value ? row["BudgetDescription"].ToString() : null,
                                CreatedBy = row["CreatedBy"] != DBNull.Value ? row["CreatedBy"].ToString() : null,
                                CreatedDate = row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : (DateTime?)null,
                                UpdatedBy = row["UpdatedBy"] != DBNull.Value ? row["UpdatedBy"].ToString() : null,
                                UpdatedDate = row["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedDate"]) : (DateTime?)null,
                                Status = row["Status"] != DBNull.Value ? row["Status"].ToString() : "1"
                            };

                            return Json(new { success = true, data = model });
                        }
                    }
                }

                return Json(new { success = false, message = "Record not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /ProjectBudgetAllocation/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] ProjectBudgetAllocationModel req)
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

                bool isInsert = !req.BudgetAllocationId.HasValue || req.BudgetAllocationId.Value <= 0;
                string flag = isInsert ? "Insert" : "Update";

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("Web_SaveProjectBudgetAllocation", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", flag);
                    cmd.Parameters.AddWithValue("@BudgetAllocationId", (object)(req.BudgetAllocationId ?? 0));
                    cmd.Parameters.AddWithValue("@ProjectKickoffId", (object)(req.ProjectKickoffId ?? 0));
                    cmd.Parameters.AddWithValue("@CostCentreId", (object)(req.CostCentreId ?? 0));
                    cmd.Parameters.AddWithValue("@BudgetAmount", (object)(req.BudgetAmount ?? 0.0));
                    cmd.Parameters.AddWithValue("@BudgetDescription", (object)(req.BudgetDescription ?? ""));

                    cmd.Parameters.AddWithValue("@CreatedBy", isInsert ? (object)(user.UserId ?? "User") : DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedDate", isInsert ? (object)DateTime.Now : DBNull.Value);
                    cmd.Parameters.AddWithValue("@UpdatedBy", !isInsert ? (object)(user.UserId ?? "User") : DBNull.Value);
                    cmd.Parameters.AddWithValue("@UpdatedDate", !isInsert ? (object)DateTime.Now : DBNull.Value);

                    cmd.Parameters.AddWithValue("@IPAddress", (object)ipAddress ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HostName", (object)hostName ?? DBNull.Value);

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            string result = dr["Result"] != DBNull.Value ? dr["Result"].ToString() : "";
                            int savedId = dr["BudgetAllocationId"] != DBNull.Value ? Convert.ToInt32(dr["BudgetAllocationId"]) : 0;

                            if (result.Equals("INSERTED", StringComparison.OrdinalIgnoreCase) ||
                                result.Equals("UPDATED", StringComparison.OrdinalIgnoreCase))
                            {
                                return Json(new
                                {
                                    success = true,
                                    message = isInsert ? "Project budget allocation created successfully!" : "Project budget allocation updated successfully!",
                                    id = savedId
                                });
                            }
                            else
                            {
                                return Json(new { success = false, message = $"Operation failed: {result}" });
                            }
                        }
                    }
                }

                return Json(new { success = false, message = "No response received from stored procedure." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /ProjectBudgetAllocation/Delete
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] ProjectBudgetAllocationModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired." });

                if (req == null || !req.BudgetAllocationId.HasValue)
                    return Json(new { success = false, message = "Invalid allocation ID." });

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("UPDATE dbo.ProjectBudgetAllocation SET Status = 0, UpdatedBy = @UpdatedBy, UpdatedDate = GETDATE() WHERE BudgetAllocationId = @Id", con))
                {
                    cmd.Parameters.AddWithValue("@Id", req.BudgetAllocationId.Value);
                    cmd.Parameters.AddWithValue("@UpdatedBy", user.UserId ?? "User");

                    await con.OpenAsync();
                    int rows = await cmd.ExecuteNonQueryAsync();

                    return Json(new { success = rows > 0, message = rows > 0 ? "Record deleted successfully." : "Record not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /ProjectBudgetAllocation/GetProjectLookups
        [HttpGet]
        public async Task<IActionResult> GetProjectLookups()
        {
            var projectList = new List<object>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("Web_SaveProjectKickOffMas", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "FETCH");
                    cmd.Parameters.AddWithValue("@ProjectKickoffId", DBNull.Value);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            projectList.Add(new
                            {
                                ProjectKickoffId = row["ProjectKickoffId"] != DBNull.Value ? Convert.ToInt32(row["ProjectKickoffId"]) : 0,
                                ProjectName = row["ProjectName"] != DBNull.Value ? row["ProjectName"].ToString() : ""
                            });
                        }
                    }
                }

                return Json(new { success = true, data = projectList });
            }
            catch
            {
                return Json(new { success = true, data = projectList });
            }
        }
    }
}

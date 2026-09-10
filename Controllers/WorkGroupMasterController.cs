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
    public class WorkGroupMasterController : Controller
    {
        private readonly string _connPROJ;
        private const string SP_NAME = "Web_SaveWorkGroupMas";

        public WorkGroupMasterController(IConfiguration configuration)
        {
            _connPROJ = configuration.GetActiveConnectionString("connPROJ")
                        ?? configuration.GetConnectionString("ConnPROJ")
                        ?? configuration.GetConnectionString("connPROJ");
        }

        // GET: /WorkGroupMaster/Index
        [HttpGet]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.Title = "Work Group Master — VGN ERP";
            ViewBag.UserId = user.UserId;
            ViewBag.UserName = user.UserName;
            return View();
        }

        // GET: /WorkGroupMaster/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = new List<WorkGroupMasterModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "FetchbyAll");
                    cmd.Parameters.AddWithValue("@WorkGroupId", DBNull.Value);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(new WorkGroupMasterModel
                            {
                                WorkGroupId = row["WorkGroupId"] != DBNull.Value ? Convert.ToInt32(row["WorkGroupId"]) : (int?)null,
                                SerialNo = row["SerialNo"] != DBNull.Value ? row["SerialNo"].ToString() : "",
                                WorkGroupName = row["WorkGroupName"] != DBNull.Value ? row["WorkGroupName"].ToString() : "",
                                WorkGroupDescription = row["WorkGroupDescription"] != DBNull.Value ? row["WorkGroupDescription"].ToString() : "",
                                WorkTypeId = row["WorkTypeId"] != DBNull.Value ? row["WorkTypeId"].ToString() : null,
                                ParentId = row["ParentId"] != DBNull.Value ? row["ParentId"].ToString() : null,
                                CreatedBy = row.Table.Columns.Contains("CreatedBy") && row["CreatedBy"] != DBNull.Value ? row["CreatedBy"].ToString() : null,
                                CreatedDate = row.Table.Columns.Contains("CreatedDate") && row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : (DateTime?)null,
                                UpdatedBy = row.Table.Columns.Contains("UpdatedBy") && row["UpdatedBy"] != DBNull.Value ? row["UpdatedBy"].ToString() : null,
                                UpdatedDate = row.Table.Columns.Contains("UpdatedDate") && row["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedDate"]) : (DateTime?)null,
                                Status = row.Table.Columns.Contains("Status") && row["Status"] != DBNull.Value ? row["Status"].ToString() : "1"
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

        // GET: /WorkGroupMaster/GetById?id=1
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "FetchbyId");
                    cmd.Parameters.AddWithValue("@WorkGroupId", id);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            var row = dt.Rows[0];
                            var model = new WorkGroupMasterModel
                            {
                                WorkGroupId = row["WorkGroupId"] != DBNull.Value ? Convert.ToInt32(row["WorkGroupId"]) : (int?)null,
                                SerialNo = row["SerialNo"] != DBNull.Value ? row["SerialNo"].ToString() : "",
                                WorkGroupName = row["WorkGroupName"] != DBNull.Value ? row["WorkGroupName"].ToString() : "",
                                WorkGroupDescription = row["WorkGroupDescription"] != DBNull.Value ? row["WorkGroupDescription"].ToString() : "",
                                WorkTypeId = row["WorkTypeId"] != DBNull.Value ? row["WorkTypeId"].ToString() : null,
                                ParentId = row["ParentId"] != DBNull.Value ? row["ParentId"].ToString() : null,
                                CreatedBy = row.Table.Columns.Contains("CreatedBy") && row["CreatedBy"] != DBNull.Value ? row["CreatedBy"].ToString() : null,
                                CreatedDate = row.Table.Columns.Contains("CreatedDate") && row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : (DateTime?)null,
                                UpdatedBy = row.Table.Columns.Contains("UpdatedBy") && row["UpdatedBy"] != DBNull.Value ? row["UpdatedBy"].ToString() : null,
                                UpdatedDate = row.Table.Columns.Contains("UpdatedDate") && row["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedDate"]) : (DateTime?)null,
                                Status = row.Table.Columns.Contains("Status") && row["Status"] != DBNull.Value ? row["Status"].ToString() : "1"
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

        // POST: /WorkGroupMaster/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] WorkGroupMasterModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired. Please log in again." });

                if (req == null || string.IsNullOrWhiteSpace(req.WorkGroupName))
                    return Json(new { success = false, message = "Work Group Name is required." });

                string ipAddress = SessionHelper.GetClientIPAddress(Request);
                string hostName = SessionHelper.GetClientHostName(ipAddress);

                bool isInsert = !req.WorkGroupId.HasValue || req.WorkGroupId.Value <= 0;
                string flag = isInsert ? "Insert" : "U";

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", flag);
                    cmd.Parameters.AddWithValue("@WorkGroupId", (object)(req.WorkGroupId ?? 0));
                    cmd.Parameters.AddWithValue("@SerialNo", (object)(req.SerialNo?.Trim() ?? ""));
                    cmd.Parameters.AddWithValue("@WorkGroupName", req.WorkGroupName.Trim());
                    cmd.Parameters.AddWithValue("@WorkGroupDescription", (object)(req.WorkGroupDescription?.Trim() ?? ""));
                    cmd.Parameters.AddWithValue("@WorkTypeId", string.IsNullOrWhiteSpace(req.WorkTypeId) ? "0" : req.WorkTypeId.Trim());
                    cmd.Parameters.AddWithValue("@ParentId", string.IsNullOrWhiteSpace(req.ParentId) ? "0" : req.ParentId.Trim());

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
                            int savedId = dr["WorkGroupId"] != DBNull.Value ? Convert.ToInt32(dr["WorkGroupId"]) : 0;

                            if (result.Equals("INSERTED", StringComparison.OrdinalIgnoreCase) ||
                                result.Equals("UPDATED", StringComparison.OrdinalIgnoreCase))
                            {
                                return Json(new
                                {
                                    success = true,
                                    message = isInsert ? "Work group created successfully!" : "Work group updated successfully!",
                                    id = savedId
                                });
                            }
                            else
                            {
                                return Json(new { success = false, message = result });
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

        // POST: /WorkGroupMaster/Delete
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] WorkGroupMasterModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired." });

                if (req == null || !req.WorkGroupId.HasValue)
                    return Json(new { success = false, message = "Invalid Work Group ID." });

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "DELETE");
                    cmd.Parameters.AddWithValue("@WorkGroupId", req.WorkGroupId.Value);
                    cmd.Parameters.AddWithValue("@UpdatedBy", user.UserId ?? "User");
                    cmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            string result = dr["Result"] != DBNull.Value ? dr["Result"].ToString() : "";
                            bool isDeleted = result.Equals("DELETED", StringComparison.OrdinalIgnoreCase);

                            return Json(new
                            {
                                success = isDeleted,
                                message = isDeleted ? "Work group deleted successfully." : result
                            });
                        }
                    }

                    return Json(new { success = true, message = "Work group deleted successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /WorkGroupMaster/GetWorkTypeLookups
        [HttpGet]
        public async Task<IActionResult> GetWorkTypeLookups()
        {
            var list = new List<object>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "LOOKUP_WORKTYPES");
                    cmd.Parameters.AddWithValue("@WorkGroupId", DBNull.Value);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(new
                            {
                                WorkTypeId = row["WorkTypeId"] != DBNull.Value ? row["WorkTypeId"].ToString() : "",
                                WorkTypeName = row["WorkTypeName"] != DBNull.Value ? row["WorkTypeName"].ToString() : ""
                            });
                        }
                    }
                }

                return Json(new { success = true, data = list });
            }
            catch
            {
                return Json(new { success = true, data = list });
            }
        }
    }
}

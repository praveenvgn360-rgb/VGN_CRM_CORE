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
    public class ResourceGroupController : Controller
    {
        private readonly string _connPROJ;
        private const string SP_SAVE = "Web_SaveResourceGroupMas";
        private const string SP_LOAD = "Web_LoadResourceGroupMas";

        public ResourceGroupController(IConfiguration configuration)
        {
            _connPROJ = configuration.GetActiveConnectionString("connPROJ")
                        ?? configuration.GetConnectionString("ConnPROJ")
                        ?? configuration.GetConnectionString("connPROJ");
        }

        // GET: /ResourceGroup/Index
        [HttpGet]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.Title = "Resource Group Master — VGN ERP";
            ViewBag.UserId = user.UserId;
            ViewBag.UserName = user.UserName;
            return View();
        }

        // GET: /ResourceGroup/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = new List<ResourceGroupModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_LOAD, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "FETCHALL");

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(new ResourceGroupModel
                            {
                                ResourceGroupId = row["ResourceGroupId"] != DBNull.Value ? Convert.ToInt32(row["ResourceGroupId"]) : (int?)null,
                                ResourceCode = row["ResourceCode"] != DBNull.Value ? row["ResourceCode"].ToString() : "",
                                ResourceGroupName = row["ResourceGroupName"] != DBNull.Value ? row["ResourceGroupName"].ToString() : "",
                                TypeId = row["TypeId"] != DBNull.Value ? row["TypeId"].ToString() : "",
                                ParentId = row["ParentId"] != DBNull.Value ? row["ParentId"].ToString() : "0",
                                ParentGroupName = row.Table.Columns.Contains("ParentGroupName") && row["ParentGroupName"] != DBNull.Value ? row["ParentGroupName"].ToString() : "Top Level (Root)",
                                Remarks = row.Table.Columns.Contains("Remarks") && row["Remarks"] != DBNull.Value ? row["Remarks"].ToString() : "",
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

        // GET: /ResourceGroup/GetById?id=1
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_LOAD, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "FETCHBYID");
                    cmd.Parameters.AddWithValue("@ResourceGroupId", id);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            var row = dt.Rows[0];
                            var model = new ResourceGroupModel
                            {
                                ResourceGroupId = row["ResourceGroupId"] != DBNull.Value ? Convert.ToInt32(row["ResourceGroupId"]) : (int?)null,
                                ResourceCode = row["ResourceCode"] != DBNull.Value ? row["ResourceCode"].ToString() : "",
                                ResourceGroupName = row["ResourceGroupName"] != DBNull.Value ? row["ResourceGroupName"].ToString() : "",
                                TypeId = row["TypeId"] != DBNull.Value ? row["TypeId"].ToString() : "",
                                ParentId = row["ParentId"] != DBNull.Value ? row["ParentId"].ToString() : "0",
                                ParentGroupName = row.Table.Columns.Contains("ParentGroupName") && row["ParentGroupName"] != DBNull.Value ? row["ParentGroupName"].ToString() : "Top Level (Root)",
                                Remarks = row.Table.Columns.Contains("Remarks") && row["Remarks"] != DBNull.Value ? row["Remarks"].ToString() : "",
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

        // POST: /ResourceGroup/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] ResourceGroupModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired. Please log in again." });

                if (req == null || string.IsNullOrWhiteSpace(req.ResourceGroupName))
                    return Json(new { success = false, message = "Resource Group Name is required." });

                string ipAddress = SessionHelper.GetClientIPAddress(Request);
                string hostName = SessionHelper.GetClientHostName(ipAddress);

                bool isInsert = !req.ResourceGroupId.HasValue || req.ResourceGroupId.Value <= 0;
                string flag = isInsert ? "INSERT" : "UPDATE";

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_SAVE, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", flag);
                    cmd.Parameters.AddWithValue("@ResourceGroupId", (object)(req.ResourceGroupId ?? 0));
                    cmd.Parameters.AddWithValue("@ResourceCode", (object)(req.ResourceCode?.Trim() ?? ""));
                    cmd.Parameters.AddWithValue("@ResourceGroupName", req.ResourceGroupName.Trim());
                    cmd.Parameters.AddWithValue("@TypeId", (object)(req.TypeId?.Trim() ?? ""));
                    cmd.Parameters.AddWithValue("@ParentId", string.IsNullOrWhiteSpace(req.ParentId) ? "0" : req.ParentId.Trim());
                    cmd.Parameters.AddWithValue("@Remarks", (object)(req.Remarks?.Trim() ?? ""));

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
                            int savedId = dr["ResourceGroupId"] != DBNull.Value ? Convert.ToInt32(dr["ResourceGroupId"]) : 0;

                            if (result.Equals("INSERTED", StringComparison.OrdinalIgnoreCase) ||
                                result.Equals("UPDATED", StringComparison.OrdinalIgnoreCase))
                            {
                                return Json(new
                                {
                                    success = true,
                                    message = isInsert ? "Resource group created successfully!" : "Resource group updated successfully!",
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

        // POST: /ResourceGroup/Delete
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] ResourceGroupModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired." });

                if (req == null || !req.ResourceGroupId.HasValue)
                    return Json(new { success = false, message = "Invalid Resource Group ID." });

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_SAVE, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "DELETE");
                    cmd.Parameters.AddWithValue("@ResourceGroupId", req.ResourceGroupId.Value);
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
                                message = isDeleted ? "Resource group deleted successfully." : result
                            });
                        }
                    }

                    return Json(new { success = true, message = "Resource group deleted successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /ResourceGroup/GetParentLookups
        [HttpGet]
        public async Task<IActionResult> GetParentLookups()
        {
            var list = new List<object>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_LOAD, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "LOOKUP");

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(new
                            {
                                ResourceGroupId = row["ResourceGroupId"] != DBNull.Value ? row["ResourceGroupId"].ToString() : "",
                                ResourceGroupName = row["ResourceGroupName"] != DBNull.Value ? row["ResourceGroupName"].ToString() : "",
                                ResourceCode = row["ResourceCode"] != DBNull.Value ? row["ResourceCode"].ToString() : "",
                                TypeId = row["TypeId"] != DBNull.Value ? row["TypeId"].ToString() : ""
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
    }
}

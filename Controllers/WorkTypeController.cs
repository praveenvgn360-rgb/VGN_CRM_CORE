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
    public class WorkTypeController : Controller
    {
        private readonly string _connPROJ;
        private const string SP_NAME = "Web_SaveWorkType";

        public WorkTypeController(IConfiguration configuration)
        {
            _connPROJ = configuration.GetActiveConnectionString("connPROJ")
                        ?? configuration.GetConnectionString("ConnPROJ")
                        ?? configuration.GetConnectionString("connPROJ");
        }

        // GET: /WorkType/Index
        [HttpGet]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.Title = "Work Type Master — VGN ERP";
            ViewBag.UserId = user.UserId;
            ViewBag.UserName = user.UserName;
            return View();
        }

        // GET: /WorkType/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = new List<WorkTypeModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "FetchByAll");
                    cmd.Parameters.AddWithValue("@WorkTypeId", DBNull.Value);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(new WorkTypeModel
                            {
                                WorkTypeId = row["WorkTypeId"] != DBNull.Value ? Convert.ToInt32(row["WorkTypeId"]) : (int?)null,
                                WorkTypeName = row["WorkTypeName"] != DBNull.Value ? row["WorkTypeName"].ToString() : "",
                                WorkTypeDescription = row["WorkTypeDescription"] != DBNull.Value ? row["WorkTypeDescription"].ToString() : "",
                                CreatedBy = row.Table.Columns.Contains("CreatedBy") && row["CreatedBy"] != DBNull.Value ? row["CreatedBy"].ToString() : null,
                                CreatedDate = row.Table.Columns.Contains("CreatedDate") && row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : (DateTime?)null,
                                UpdatedBy = row.Table.Columns.Contains("UpdatedBy") && row["UpdatedBy"] != DBNull.Value ? row["UpdatedBy"].ToString() : null,
                                UpdatedDate = row.Table.Columns.Contains("UpdatedDate") && row["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedDate"]) : (DateTime?)null,
                                IPAddress = row.Table.Columns.Contains("IPAddress") && row["IPAddress"] != DBNull.Value ? row["IPAddress"].ToString() : null,
                                HostName = row.Table.Columns.Contains("HostName") && row["HostName"] != DBNull.Value ? row["HostName"].ToString() : null,
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

        // GET: /WorkType/GetById?id=1
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "FetchById");
                    cmd.Parameters.AddWithValue("@WorkTypeId", id);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            var row = dt.Rows[0];
                            var model = new WorkTypeModel
                            {
                                WorkTypeId = row["WorkTypeId"] != DBNull.Value ? Convert.ToInt32(row["WorkTypeId"]) : (int?)null,
                                WorkTypeName = row["WorkTypeName"] != DBNull.Value ? row["WorkTypeName"].ToString() : "",
                                WorkTypeDescription = row["WorkTypeDescription"] != DBNull.Value ? row["WorkTypeDescription"].ToString() : "",
                                CreatedBy = row.Table.Columns.Contains("CreatedBy") && row["CreatedBy"] != DBNull.Value ? row["CreatedBy"].ToString() : null,
                                CreatedDate = row.Table.Columns.Contains("CreatedDate") && row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : (DateTime?)null,
                                UpdatedBy = row.Table.Columns.Contains("UpdatedBy") && row["UpdatedBy"] != DBNull.Value ? row["UpdatedBy"].ToString() : null,
                                UpdatedDate = row.Table.Columns.Contains("UpdatedDate") && row["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedDate"]) : (DateTime?)null,
                                IPAddress = row.Table.Columns.Contains("IPAddress") && row["IPAddress"] != DBNull.Value ? row["IPAddress"].ToString() : null,
                                HostName = row.Table.Columns.Contains("HostName") && row["HostName"] != DBNull.Value ? row["HostName"].ToString() : null,
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

        // POST: /WorkType/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] WorkTypeModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired. Please log in again." });

                if (req == null || string.IsNullOrWhiteSpace(req.WorkTypeName))
                    return Json(new { success = false, message = "Work Type Name is required." });

                string ipAddress = SessionHelper.GetClientIPAddress(Request);
                string hostName = SessionHelper.GetClientHostName(ipAddress);

                bool isInsert = !req.WorkTypeId.HasValue || req.WorkTypeId.Value <= 0;
                string flag = isInsert ? "Insert" : "Update";

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", flag);
                    cmd.Parameters.AddWithValue("@WorkTypeId", (object)(req.WorkTypeId ?? 0));
                    cmd.Parameters.AddWithValue("@WorkTypeName", req.WorkTypeName.Trim());
                    cmd.Parameters.AddWithValue("@WorkTypeDescription", (object)(req.WorkTypeDescription?.Trim() ?? ""));

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
                            int savedId = dr["WorkTypeId"] != DBNull.Value ? Convert.ToInt32(dr["WorkTypeId"]) : 0;

                            if (result.Equals("INSERTED", StringComparison.OrdinalIgnoreCase) ||
                                result.Equals("UPDATED", StringComparison.OrdinalIgnoreCase))
                            {
                                return Json(new
                                {
                                    success = true,
                                    message = isInsert ? "Work type created successfully!" : "Work type updated successfully!",
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

        // POST: /WorkType/Delete
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] WorkTypeModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired." });

                if (req == null || !req.WorkTypeId.HasValue)
                    return Json(new { success = false, message = "Invalid Work Type ID." });

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "DELETE");
                    cmd.Parameters.AddWithValue("@WorkTypeId", req.WorkTypeId.Value);
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
                                message = isDeleted ? "Work type deleted successfully." : result
                            });
                        }
                    }

                    return Json(new { success = true, message = "Work type deleted successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

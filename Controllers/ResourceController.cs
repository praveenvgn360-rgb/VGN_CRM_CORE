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
    public class ResourceController : Controller
    {
        private readonly string _connPROJ;
        private const string SP_SAVE = "Web_SaveResourceMas";
        private const string SP_LOAD = "Web_LoadResourceMas";

        public ResourceController(IConfiguration configuration)
        {
            _connPROJ = configuration.GetActiveConnectionString("connPROJ")
                        ?? configuration.GetConnectionString("ConnPROJ")
                        ?? configuration.GetConnectionString("connPROJ");
        }

        // GET: /Resource/Index
        [HttpGet]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.Title = "Resource Master — VGN ERP";
            ViewBag.UserId = user.UserId;
            ViewBag.UserName = user.UserName;
            return View();
        }

        // GET: /Resource/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = new List<ResourceModel>();

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
                            list.Add(new ResourceModel
                            {
                                ResourceId = row["ResourceId"] != DBNull.Value ? Convert.ToInt32(row["ResourceId"]) : (int?)null,
                                ResourceName = row["ResourceName"] != DBNull.Value ? row["ResourceName"].ToString() : "",
                                TypeId = row["TypeId"] != DBNull.Value ? row["TypeId"].ToString() : "",
                                ResourceGroupId = row["ResourceGroupId"] != DBNull.Value ? row["ResourceGroupId"].ToString() : "",
                                ResourceGroupName = row.Table.Columns.Contains("ResourceGroupName") && row["ResourceGroupName"] != DBNull.Value ? row["ResourceGroupName"].ToString() : "—",
                                UnitId = row["UnitId"] != DBNull.Value ? row["UnitId"].ToString() : "",
                                UnitName = row.Table.Columns.Contains("UnitName") && row["UnitName"] != DBNull.Value ? row["UnitName"].ToString() : "—",
                                Rate = row.Table.Columns.Contains("Rate") && row["Rate"] != DBNull.Value ? row["Rate"].ToString() : "0",
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

        // GET: /Resource/GetById?id=1
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
                    cmd.Parameters.AddWithValue("@ResourceId", id);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            var row = dt.Rows[0];
                            var model = new ResourceModel
                            {
                                ResourceId = row["ResourceId"] != DBNull.Value ? Convert.ToInt32(row["ResourceId"]) : (int?)null,
                                ResourceName = row["ResourceName"] != DBNull.Value ? row["ResourceName"].ToString() : "",
                                TypeId = row["TypeId"] != DBNull.Value ? row["TypeId"].ToString() : "",
                                ResourceGroupId = row["ResourceGroupId"] != DBNull.Value ? row["ResourceGroupId"].ToString() : "",
                                ResourceGroupName = row.Table.Columns.Contains("ResourceGroupName") && row["ResourceGroupName"] != DBNull.Value ? row["ResourceGroupName"].ToString() : "—",
                                UnitId = row["UnitId"] != DBNull.Value ? row["UnitId"].ToString() : "",
                                UnitName = row.Table.Columns.Contains("UnitName") && row["UnitName"] != DBNull.Value ? row["UnitName"].ToString() : "—",
                                Rate = row.Table.Columns.Contains("Rate") && row["Rate"] != DBNull.Value ? row["Rate"].ToString() : "0",
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

        // POST: /Resource/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] ResourceModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired. Please log in again." });

                if (req == null || string.IsNullOrWhiteSpace(req.ResourceName))
                    return Json(new { success = false, message = "Resource Name is required." });

                string ipAddress = SessionHelper.GetClientIPAddress(Request);
                string hostName = SessionHelper.GetClientHostName(ipAddress);

                bool isInsert = !req.ResourceId.HasValue || req.ResourceId.Value <= 0;
                string flag = isInsert ? "INSERT" : "UPDATE";

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_SAVE, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", flag);
                    cmd.Parameters.AddWithValue("@ResourceId", (object)(req.ResourceId ?? 0));
                    cmd.Parameters.AddWithValue("@ResourceName", req.ResourceName.Trim());
                    cmd.Parameters.AddWithValue("@TypeId", (object)(req.TypeId?.Trim() ?? ""));
                    cmd.Parameters.AddWithValue("@ResourceGroupId", (object)(req.ResourceGroupId?.Trim() ?? ""));
                    cmd.Parameters.AddWithValue("@UnitId", (object)(req.UnitId?.Trim() ?? ""));
                    cmd.Parameters.AddWithValue("@Rate", (object)(req.Rate?.Trim() ?? "0"));

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
                            int savedId = dr["ResourceId"] != DBNull.Value ? Convert.ToInt32(dr["ResourceId"]) : 0;

                            if (result.Equals("INSERTED", StringComparison.OrdinalIgnoreCase) ||
                                result.Equals("UPDATED", StringComparison.OrdinalIgnoreCase))
                            {
                                return Json(new
                                {
                                    success = true,
                                    message = isInsert ? "Resource created successfully!" : "Resource updated successfully!",
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

        // POST: /Resource/Delete
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] ResourceModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired." });

                if (req == null || !req.ResourceId.HasValue)
                    return Json(new { success = false, message = "Invalid Resource ID." });

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_SAVE, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "DELETE");
                    cmd.Parameters.AddWithValue("@ResourceId", req.ResourceId.Value);
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
                                message = isDeleted ? "Resource deleted successfully." : result
                            });
                        }
                    }

                    return Json(new { success = true, message = "Resource deleted successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /Resource/GetLookups
        [HttpGet]
        public async Task<IActionResult> GetLookups()
        {
            var groups = new List<object>();
            var units = new List<object>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_LOAD, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "LOOKUPS");

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var ds = new DataSet();
                        da.Fill(ds);

                        // Table 0: Resource Groups
                        if (ds.Tables.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                groups.Add(new
                                {
                                    ResourceGroupId = row["ResourceGroupId"] != DBNull.Value ? row["ResourceGroupId"].ToString() : "",
                                    ResourceGroupName = row["ResourceGroupName"] != DBNull.Value ? row["ResourceGroupName"].ToString() : "",
                                    ResourceCode = row["ResourceCode"] != DBNull.Value ? row["ResourceCode"].ToString() : "",
                                    TypeId = row["TypeId"] != DBNull.Value ? row["TypeId"].ToString() : ""
                                });
                            }
                        }

                        // Table 1: UOM / Units
                        if (ds.Tables.Count > 1)
                        {
                            foreach (DataRow row in ds.Tables[1].Rows)
                            {
                                string uId = row.Table.Columns.Contains("UnitId") && row["UnitId"] != DBNull.Value ? row["UnitId"].ToString()
                                            : (row.Table.Columns.Contains("TypeId") && row["TypeId"] != DBNull.Value ? row["TypeId"].ToString() : "");
                                string uName = row.Table.Columns.Contains("UnitName") && row["UnitName"] != DBNull.Value ? row["UnitName"].ToString()
                                            : (row.Table.Columns.Contains("TypeName") && row["TypeName"] != DBNull.Value ? row["TypeName"].ToString() : "");

                                units.Add(new
                                {
                                    UnitId = uId,
                                    UnitName = uName,
                                    TypeId = uId,
                                    TypeName = uName
                                });
                            }
                        }
                    }
                }

                return Json(new { success = true, groups = groups, units = units });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, groups = groups, units = units });
            }
        }
    }
}

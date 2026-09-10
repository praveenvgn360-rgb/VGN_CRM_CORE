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
    [Route("[controller]")]
    [Route("WBS")]
    public class WBSMasterController : Controller
    {
        private readonly string _connPROJ;
        private const string SP_NAME = "dbo.Web_SaveWBSMaster";

        public WBSMasterController(IConfiguration configuration)
        {
            _connPROJ = configuration.GetActiveConnectionString("connPROJ")
                        ?? configuration.GetConnectionString("ConnPROJ")
                        ?? configuration.GetConnectionString("connPROJ");
        }

        // GET: /WBSMaster or /WBS
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.Title = "WBS (Work Break Structure) Master — VGN ERP";
            ViewBag.UserId = user.UserId;
            ViewBag.UserName = user.UserName;
            return View();
        }

        // GET: /WBSMaster/GetAll or /WBS/GetAll
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var list = new List<WBSMasterModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "FETCHBYALL");
                    cmd.Parameters.AddWithValue("@WBSId", DBNull.Value);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(new WBSMasterModel
                            {
                                WBSId = row["WBSId"] != DBNull.Value ? Convert.ToInt32(row["WBSId"]) : (int?)null,
                                WBSName = row["WBSName"] != DBNull.Value ? row["WBSName"].ToString() : "",
                                ParentId = row["ParentId"] != DBNull.Value ? row["ParentId"].ToString() : "0",
                                ParentName = row.Table.Columns.Contains("ParentName") && row["ParentName"] != DBNull.Value ? row["ParentName"].ToString() : "Root / Top Level",
                                CreatedBy = row.Table.Columns.Contains("CreatedBy") && row["CreatedBy"] != DBNull.Value ? row["CreatedBy"].ToString() : null,
                                CreatedDate = row.Table.Columns.Contains("CreatedDate") && row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : (DateTime?)null,
                                UpdatedBy = row.Table.Columns.Contains("UpdatedBy") && row["UpdatedBy"] != DBNull.Value ? row["UpdatedBy"].ToString() : null,
                                UpdatedDate = row.Table.Columns.Contains("UpdatedDate") && row["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedDate"]) : (DateTime?)null,
                                IPAddress = row.Table.Columns.Contains("IPAddress") && row["IPAddress"] != DBNull.Value ? row["IPAddress"].ToString() : null,
                                HostName = row.Table.Columns.Contains("HostName") && row["HostName"] != DBNull.Value ? row["HostName"].ToString() : null,
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

        // GET: /WBSMaster/GetById?id=1
        [HttpGet("GetById")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "FETCHBYID");
                    cmd.Parameters.AddWithValue("@WBSId", id);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            var row = dt.Rows[0];
                            var model = new WBSMasterModel
                            {
                                WBSId = row["WBSId"] != DBNull.Value ? Convert.ToInt32(row["WBSId"]) : (int?)null,
                                WBSName = row["WBSName"] != DBNull.Value ? row["WBSName"].ToString() : "",
                                ParentId = row["ParentId"] != DBNull.Value ? row["ParentId"].ToString() : "0",
                                ParentName = row.Table.Columns.Contains("ParentName") && row["ParentName"] != DBNull.Value ? row["ParentName"].ToString() : "Root / Top Level",
                                CreatedBy = row.Table.Columns.Contains("CreatedBy") && row["CreatedBy"] != DBNull.Value ? row["CreatedBy"].ToString() : null,
                                CreatedDate = row.Table.Columns.Contains("CreatedDate") && row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : (DateTime?)null,
                                UpdatedBy = row.Table.Columns.Contains("UpdatedBy") && row["UpdatedBy"] != DBNull.Value ? row["UpdatedBy"].ToString() : null,
                                UpdatedDate = row.Table.Columns.Contains("UpdatedDate") && row["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedDate"]) : (DateTime?)null,
                                IPAddress = row.Table.Columns.Contains("IPAddress") && row["IPAddress"] != DBNull.Value ? row["IPAddress"].ToString() : null,
                                HostName = row.Table.Columns.Contains("HostName") && row["HostName"] != DBNull.Value ? row["HostName"].ToString() : null,
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

        // POST: /WBSMaster/Save
        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] WBSMasterModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired. Please log in again." });

                if (req == null || string.IsNullOrWhiteSpace(req.WBSName))
                    return Json(new { success = false, message = "WBS Name is required." });

                string ipAddress = SessionHelper.GetClientIPAddress(Request);
                string hostName = SessionHelper.GetClientHostName(ipAddress);

                bool isInsert = !req.WBSId.HasValue || req.WBSId.Value <= 0;
                string flag = isInsert ? "INSERT" : "UPDATE";

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", flag);
                    cmd.Parameters.AddWithValue("@WBSId", (object)(req.WBSId ?? 0));
                    cmd.Parameters.AddWithValue("@WBSName", req.WBSName.Trim());
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
                            int savedId = dr["WBSId"] != DBNull.Value ? Convert.ToInt32(dr["WBSId"]) : 0;

                            if (result.Equals("INSERTED", StringComparison.OrdinalIgnoreCase) ||
                                result.Equals("UPDATED", StringComparison.OrdinalIgnoreCase))
                            {
                                return Json(new
                                {
                                    success = true,
                                    message = isInsert ? "WBS created successfully!" : "WBS updated successfully!",
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

        // POST: /WBSMaster/Delete
        [HttpPost("Delete")]
        public async Task<IActionResult> Delete([FromBody] WBSMasterModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired." });

                if (req == null || !req.WBSId.HasValue)
                    return Json(new { success = false, message = "Invalid WBS ID." });

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "DELETE");
                    cmd.Parameters.AddWithValue("@WBSId", req.WBSId.Value);
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
                                message = isDeleted ? "WBS deleted successfully." : result
                            });
                        }
                    }

                    return Json(new { success = true, message = "WBS deleted successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /WBSMaster/GetParentLookups
        [HttpGet("GetParentLookups")]
        public async Task<IActionResult> GetParentLookups()
        {
            var list = new List<object>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "LOOKUP_PARENTS");
                    cmd.Parameters.AddWithValue("@WBSId", DBNull.Value);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(new
                            {
                                WBSId = row["WBSId"] != DBNull.Value ? row["WBSId"].ToString() : "",
                                WBSName = row["WBSName"] != DBNull.Value ? row["WBSName"].ToString() : "",
                                ParentId = row["ParentId"] != DBNull.Value ? row["ParentId"].ToString() : "0"
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

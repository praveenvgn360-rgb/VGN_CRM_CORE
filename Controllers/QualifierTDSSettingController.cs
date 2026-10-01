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
    public class QualifierTDSSettingController : Controller
    {
        private readonly string _connPROJ;
        private const string SP_NAME = "sp_QualifierTDSSetting";

        public QualifierTDSSettingController(IConfiguration configuration)
        {
            _connPROJ = configuration.GetActiveConnectionString("connPROJ")
                        ?? configuration.GetConnectionString("ConnPROJ")
                        ?? configuration.GetConnectionString("connPROJ");
        }

        // GET: /QualifierTDSSetting/Index
        [HttpGet]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.Title = "Qualifier TDS Setting — VGN ERP";
            ViewBag.UserId = user.UserId;
            ViewBag.UserName = user.UserName;
            return View();
        }

        // GET: /QualifierTDSSetting/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = new List<QualifierTDSSettingModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "GetAll");

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(new QualifierTDSSettingModel
                            {
                                TDSTypeId = row["TDSTypeId"] != DBNull.Value ? Convert.ToInt32(row["TDSTypeId"]) : (int?)null,
                                TDSType = row["TDSType"] != DBNull.Value ? row["TDSType"].ToString() : "",
                                SectionId = row["SectionId"] != DBNull.Value ? row["SectionId"].ToString() : "0",
                                SectionCode = row.Table.Columns.Contains("SectionCode") && row["SectionCode"] != DBNull.Value 
                                    ? row["SectionCode"].ToString() 
                                    : (row["SectionId"] != DBNull.Value ? row["SectionId"].ToString() : "None"),
                                SectionDescription = row.Table.Columns.Contains("SectionDescription") && row["SectionDescription"] != DBNull.Value 
                                    ? row["SectionDescription"].ToString() 
                                    : "",
                                Status = row["Status"] != DBNull.Value ? row["Status"].ToString() : "1"
                            });
                        }
                    }
                }

                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading TDS settings: " + ex.Message, data = list });
            }
        }

        // GET: /QualifierTDSSetting/GetById?id=1
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "GetById");
                    cmd.Parameters.AddWithValue("@TDSTypeId", id);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            var row = dt.Rows[0];
                            var model = new QualifierTDSSettingModel
                            {
                                TDSTypeId = row["TDSTypeId"] != DBNull.Value ? Convert.ToInt32(row["TDSTypeId"]) : (int?)null,
                                TDSType = row["TDSType"] != DBNull.Value ? row["TDSType"].ToString() : "",
                                SectionId = row["SectionId"] != DBNull.Value ? row["SectionId"].ToString() : "0",
                                SectionCode = row.Table.Columns.Contains("SectionCode") && row["SectionCode"] != DBNull.Value 
                                    ? row["SectionCode"].ToString() 
                                    : (row["SectionId"] != DBNull.Value ? row["SectionId"].ToString() : "None"),
                                SectionDescription = row.Table.Columns.Contains("SectionDescription") && row["SectionDescription"] != DBNull.Value 
                                    ? row["SectionDescription"].ToString() 
                                    : "",
                                Status = row["Status"] != DBNull.Value ? row["Status"].ToString() : "1"
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

        // GET: /QualifierTDSSetting/GetSections
        [HttpGet]
        public async Task<IActionResult> GetSections()
        {
            var list = new List<TDSSectionDropdownModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "GetSections");

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(new TDSSectionDropdownModel
                            {
                                SectionId = row["SectionId"] != DBNull.Value ? Convert.ToInt32(row["SectionId"]) : 0,
                                SectionCode = row["SectionCode"] != DBNull.Value ? row["SectionCode"].ToString() : "",
                                Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : ""
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

        // POST: /QualifierTDSSetting/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] QualifierTDSSettingModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired. Please log in again." });

                if (req == null || string.IsNullOrWhiteSpace(req.TDSType))
                    return Json(new { success = false, message = "TDS Qualifier Type Name is required." });

                bool isInsert = !req.TDSTypeId.HasValue || req.TDSTypeId.Value <= 0;
                string flag = isInsert ? "Insert" : "Update";

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", flag);
                    cmd.Parameters.AddWithValue("@TDSTypeId", (object)(req.TDSTypeId ?? 0));
                    cmd.Parameters.AddWithValue("@TDSType", req.TDSType.Trim());
                    cmd.Parameters.AddWithValue("@SectionId", (object)(string.IsNullOrWhiteSpace(req.SectionId) ? "0" : req.SectionId.Trim()));
                    cmd.Parameters.AddWithValue("@Status", (object)(req.Status == "0" ? "0" : "1"));

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            string result = dr["Result"] != DBNull.Value ? dr["Result"].ToString() : "";
                            int savedId = dr["TDSTypeId"] != DBNull.Value ? Convert.ToInt32(dr["TDSTypeId"]) : 0;
                            string msg = dr["Message"] != DBNull.Value ? dr["Message"].ToString() : "";

                            if (result.Equals("INSERTED", StringComparison.OrdinalIgnoreCase) ||
                                result.Equals("UPDATED", StringComparison.OrdinalIgnoreCase))
                            {
                                return Json(new
                                {
                                    success = true,
                                    message = isInsert 
                                        ? "TDS Qualifier Setting created successfully!" 
                                        : "TDS Qualifier Setting updated successfully!",
                                    id = savedId
                                });
                            }
                            else if (result.Equals("EXISTS", StringComparison.OrdinalIgnoreCase))
                            {
                                return Json(new { success = false, message = msg });
                            }
                            else
                            {
                                return Json(new { success = false, message = msg });
                            }
                        }
                    }
                }

                return Json(new { success = false, message = "No response received from database." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving TDS setting: " + ex.Message });
            }
        }

        // POST: /QualifierTDSSetting/ToggleStatus
        [HttpPost]
        public async Task<IActionResult> ToggleStatus([FromQuery] int id)
        {
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "ToggleStatus");
                    cmd.Parameters.AddWithValue("@TDSTypeId", id);

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            string msg = dr["Message"] != DBNull.Value ? dr["Message"].ToString() : "Status updated.";
                            return Json(new { success = true, message = msg });
                        }
                    }
                }

                return Json(new { success = true, message = "Status toggled successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /QualifierTDSSetting/Delete
        [HttpPost]
        public async Task<IActionResult> Delete([FromQuery] int id, [FromQuery] bool hardDelete = false)
        {
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", hardDelete ? "HardDelete" : "Delete");
                    cmd.Parameters.AddWithValue("@TDSTypeId", id);

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            string msg = dr["Message"] != DBNull.Value ? dr["Message"].ToString() : "Record processed.";
                            return Json(new { success = true, message = msg });
                        }
                    }
                }

                return Json(new { success = true, message = "Record deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

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
    public class MeasurementTemplateMasterController : Controller
    {
        private readonly string _connPROJ;
        private const string SP_NAME = "Web_SaveMeasurementTemplateMaster";

        public MeasurementTemplateMasterController(IConfiguration configuration)
        {
            _connPROJ = configuration.GetActiveConnectionString("connPROJ")
                        ?? configuration.GetConnectionString("ConnPROJ")
                        ?? configuration.GetConnectionString("connPROJ");
        }

        // GET: /MeasurementTemplateMaster/Index
        [HttpGet]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.Title = "Measurement Template Master — VGN ERP";
            ViewBag.ActiveMenu = "Measurement Template Master";
            ViewBag.UserId = user.UserId;
            ViewBag.UserName = user.UserName;
            return View();
        }

        // GET: /MeasurementTemplateMaster/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = new List<MeasurementTemplateMasterModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "FETCHBYALL");

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(MapRowToModel(row));
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

        // GET: /MeasurementTemplateMaster/GetById?id=1
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "FETCHBYID");
                    cmd.Parameters.AddWithValue("@TemplateId", id);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            var model = MapRowToModel(dt.Rows[0]);
                            return Json(new { success = true, data = model });
                        }
                    }
                }

                return Json(new { success = false, message = "Template not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /MeasurementTemplateMaster/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] MeasurementTemplateMasterModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired. Please log in again." });

                if (req == null || string.IsNullOrWhiteSpace(req.TemplateName))
                    return Json(new { success = false, message = "Template Name is required." });

                if (string.IsNullOrWhiteSpace(req.SelectedColumns))
                    return Json(new { success = false, message = "Please choose at least one summation column." });

                if (string.IsNullOrWhiteSpace(req.CellName))
                    return Json(new { success = false, message = "Summation column is required." });

                string ipAddress = SessionHelper.GetClientIPAddress(Request);
                string hostName = SessionHelper.GetClientHostName(ipAddress);

                bool isInsert = !req.TemplateId.HasValue || req.TemplateId.Value <= 0;
                string flag = isInsert ? "INSERT" : "UPDATE";

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", flag);
                    cmd.Parameters.AddWithValue("@TemplateId", (object)(req.TemplateId ?? 0));
                    cmd.Parameters.AddWithValue("@TemplateName", req.TemplateName.Trim());
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(req.Description) ? "[[]]" : req.Description);
                    cmd.Parameters.AddWithValue("@CellName", req.CellName.Trim());
                    cmd.Parameters.AddWithValue("@SelectedColumns", req.SelectedColumns.Trim());
                    cmd.Parameters.AddWithValue("@TypeId", req.TypeId);
                    cmd.Parameters.AddWithValue("@Remarks", (object)(req.Remarks?.Trim() ?? string.Empty));
                    cmd.Parameters.AddWithValue("@Description1", (object)(req.Description1?.Trim() ?? string.Empty));

                    cmd.Parameters.AddWithValue("@CreatedBy", isInsert ? (object)(user.UserId ?? "User") : DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedDate", isInsert ? (object)DateTime.Now : DBNull.Value);
                    cmd.Parameters.AddWithValue("@UpdatedBy", !isInsert ? (object)(user.UserId ?? "User") : DBNull.Value);
                    cmd.Parameters.AddWithValue("@UpdatedDate", !isInsert ? (object)DateTime.Now : DBNull.Value);

                    cmd.Parameters.AddWithValue("@IpAddress", (object)ipAddress ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HostName", (object)hostName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", (object)(req.Status ?? "1"));

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            string result = dr["Result"] != DBNull.Value ? dr["Result"].ToString() : "";
                            int savedId = dr["TemplateId"] != DBNull.Value ? Convert.ToInt32(dr["TemplateId"]) : 0;

                            if (result.Equals("INSERTED", StringComparison.OrdinalIgnoreCase) ||
                                result.Equals("UPDATED", StringComparison.OrdinalIgnoreCase))
                            {
                                return Json(new
                                {
                                    success = true,
                                    message = isInsert ? "Measurement Template created successfully!" : "Measurement Template updated successfully!",
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

                return Json(new { success = false, message = "Unable to process template save." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /MeasurementTemplateMaster/Delete
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] MeasurementTemplateMasterModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired. Please log in again." });

                if (req == null || !req.TemplateId.HasValue || req.TemplateId.Value <= 0)
                    return Json(new { success = false, message = "Valid Template ID is required." });

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "DELETE");
                    cmd.Parameters.AddWithValue("@TemplateId", req.TemplateId.Value);
                    cmd.Parameters.AddWithValue("@Remarks", (object)(req.Remarks?.Trim() ?? "Deleted by user"));
                    cmd.Parameters.AddWithValue("@UpdatedBy", (object)(user.UserId ?? "User"));
                    cmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            string result = dr["Result"] != DBNull.Value ? dr["Result"].ToString() : "";
                            if (result.Equals("DELETED", StringComparison.OrdinalIgnoreCase))
                            {
                                return Json(new { success = true, message = "Measurement Template deleted successfully!" });
                            }
                            else
                            {
                                return Json(new { success = false, message = result });
                            }
                        }
                    }
                }

                return Json(new { success = false, message = "Unable to delete template." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /MeasurementTemplateMaster/SeedDefaults
        [HttpPost]
        public async Task<IActionResult> SeedDefaults()
        {
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "SEED_DEFAULTS");

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }

                return Json(new { success = true, message = "Default templates seeded successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper mapper
        private static MeasurementTemplateMasterModel MapRowToModel(DataRow row)
        {
            return new MeasurementTemplateMasterModel
            {
                TemplateId = row["TemplateId"] != DBNull.Value ? Convert.ToInt32(row["TemplateId"]) : (int?)null,
                TemplateName = row["TemplateName"] != DBNull.Value ? row["TemplateName"].ToString() : "",
                Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : "[[]]",
                CellName = row["CellName"] != DBNull.Value ? row["CellName"].ToString() : "",
                SelectedColumns = row["SelectedColumns"] != DBNull.Value ? row["SelectedColumns"].ToString() : "",
                DeleteFlag = row.Table.Columns.Contains("DeleteFlag") && row["DeleteFlag"] != DBNull.Value && Convert.ToBoolean(row["DeleteFlag"]),
                DeletedOn = row.Table.Columns.Contains("DeletedOn") && row["DeletedOn"] != DBNull.Value ? Convert.ToDateTime(row["DeletedOn"]) : (DateTime?)null,
                Remarks = row.Table.Columns.Contains("Remarks") && row["Remarks"] != DBNull.Value ? row["Remarks"].ToString() : "",
                TypeId = row.Table.Columns.Contains("TypeId") && row["TypeId"] != DBNull.Value ? Convert.ToInt32(row["TypeId"]) : 0,
                Description1 = row.Table.Columns.Contains("Description1") && row["Description1"] != DBNull.Value ? row["Description1"].ToString() : "",
                CreatedBy = row.Table.Columns.Contains("CreatedBy") && row["CreatedBy"] != DBNull.Value ? row["CreatedBy"].ToString() : null,
                CreatedDate = row.Table.Columns.Contains("CreatedDate") && row["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(row["CreatedDate"]) : (DateTime?)null,
                UpdatedBy = row.Table.Columns.Contains("UpdatedBy") && row["UpdatedBy"] != DBNull.Value ? row["UpdatedBy"].ToString() : null,
                UpdatedDate = row.Table.Columns.Contains("UpdatedDate") && row["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedDate"]) : (DateTime?)null,
                IpAddress = row.Table.Columns.Contains("IpAddress") && row["IpAddress"] != DBNull.Value ? row["IpAddress"].ToString() : null,
                HostName = row.Table.Columns.Contains("HostName") && row["HostName"] != DBNull.Value ? row["HostName"].ToString() : null,
                Status = row.Table.Columns.Contains("Status") && row["Status"] != DBNull.Value ? row["Status"].ToString() : "1"
            };
        }
    }
}

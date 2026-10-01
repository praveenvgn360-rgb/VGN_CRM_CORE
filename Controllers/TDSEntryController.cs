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
    public class TDSEntryController : Controller
    {
        private readonly string _connPROJ;
        private const string SP_NAME = "sp_TDSEntry";

        public TDSEntryController(IConfiguration configuration)
        {
            _connPROJ = configuration.GetActiveConnectionString("connPROJ")
                        ?? configuration.GetConnectionString("ConnPROJ")
                        ?? configuration.GetConnectionString("connPROJ");
        }

        // GET: /TDSEntry/Index
        [HttpGet]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.Title = "TDS Entry — VGN ERP";
            ViewBag.UserId = user.UserId;
            ViewBag.UserName = user.UserName;
            return View();
        }

        // GET: /TDSEntry/GetAllTypes
        [HttpGet]
        public async Task<IActionResult> GetAllTypes()
        {
            var list = new List<TDSTypeAccordionModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "GetAllTypes");

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(new TDSTypeAccordionModel
                            {
                                TDSTypeId = row["TDSTypeId"] != DBNull.Value ? Convert.ToInt32(row["TDSTypeId"]) : 0,
                                TDSType = row["TDSType"] != DBNull.Value ? row["TDSType"].ToString() : "",
                                SectionId = row["SectionId"] != DBNull.Value ? row["SectionId"].ToString() : "0",
                                Status = row["Status"] != DBNull.Value ? row["Status"].ToString() : "1"
                            });
                        }
                    }
                }

                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading TDS types: " + ex.Message, data = list });
            }
        }

        // GET: /TDSEntry/GetSettingsByType?tdsTypeId=2
        [HttpGet]
        public async Task<IActionResult> GetSettingsByType(string tdsTypeId)
        {
            var list = new List<TDSEntrySettingModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "GetSettingsByType");
                    cmd.Parameters.AddWithValue("@TDSTypeId", tdsTypeId ?? "0");

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(MapSettingRow(row));
                        }
                    }
                }

                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading settings: " + ex.Message, data = list });
            }
        }

        // GET: /TDSEntry/GetAllSettings
        [HttpGet]
        public async Task<IActionResult> GetAllSettings()
        {
            var list = new List<TDSEntrySettingModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "GetAllSettings");

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(MapSettingRow(row));
                        }
                    }
                }

                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading all settings: " + ex.Message, data = list });
            }
        }

        // GET: /TDSEntry/GetSections
        [HttpGet]
        public async Task<IActionResult> GetSections()
        {
            var list = new List<TDSSectionLookupModel>();

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
                            list.Add(new TDSSectionLookupModel
                            {
                                SectionId = row["SectionId"] != DBNull.Value ? Convert.ToInt32(row["SectionId"]) : 0,
                                Section = row["Section"] != DBNull.Value ? row["Section"].ToString() : "",
                                Code = row["Code"] != DBNull.Value ? row["Code"].ToString() : "",
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

        // POST: /TDSEntry/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] TDSEntrySettingModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired. Please log in again." });

                if (req == null || string.IsNullOrWhiteSpace(req.TDSTypeId) || req.TDSTypeId == "0")
                    return Json(new { success = false, message = "TDS Type is required." });

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "Save");
                    cmd.Parameters.AddWithValue("@SettingId", req.SettingId);
                    cmd.Parameters.AddWithValue("@TDSTypeId", req.TDSTypeId ?? "0");
                    cmd.Parameters.AddWithValue("@SectionId", string.IsNullOrWhiteSpace(req.SectionId) ? "0" : req.SectionId.Trim());
                    cmd.Parameters.AddWithValue("@TaxablePer", (object)req.TaxablePer ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TaxPer", (object)req.TaxPer ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CessPer", (object)req.CessPer ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EDCess", (object)req.EDCess ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HEDCess", (object)req.HEDCess ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NetTax", (object)req.NetTax ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Limit", (object)req.Limit ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", req.Status == "0" ? "0" : "1");

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            string result = dr["Result"] != DBNull.Value ? dr["Result"].ToString() : "";
                            int savedId = dr["SettingId"] != DBNull.Value ? Convert.ToInt32(dr["SettingId"]) : 0;
                            string msg = dr["Message"] != DBNull.Value ? dr["Message"].ToString() : "";

                            if (result.Equals("INSERTED", StringComparison.OrdinalIgnoreCase) ||
                                result.Equals("UPDATED", StringComparison.OrdinalIgnoreCase))
                            {
                                return Json(new
                                {
                                    success = true,
                                    message = msg,
                                    id = savedId
                                });
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
                return Json(new { success = false, message = "Error saving TDS entry: " + ex.Message });
            }
        }

        // POST: /TDSEntry/Delete?settingId=1
        [HttpPost]
        public async Task<IActionResult> Delete([FromQuery] int settingId)
        {
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "Delete");
                    cmd.Parameters.AddWithValue("@SettingId", settingId);

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            string msg = dr["Message"] != DBNull.Value ? dr["Message"].ToString() : "Record deleted.";
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

        /// <summary>
        /// Helper to map a DataRow to TDSEntrySettingModel
        /// </summary>
        private TDSEntrySettingModel MapSettingRow(DataRow row)
        {
            return new TDSEntrySettingModel
            {
                SettingId = row.Table.Columns.Contains("SettingId") && row["SettingId"] != DBNull.Value
                    ? Convert.ToInt32(row["SettingId"]) : 0,
                TDSTypeId = row.Table.Columns.Contains("TDSTypeId") && row["TDSTypeId"] != DBNull.Value
                    ? row["TDSTypeId"].ToString() : "0",
                TDSType = row.Table.Columns.Contains("TDSType") && row["TDSType"] != DBNull.Value
                    ? row["TDSType"].ToString() : "",
                SectionId = row.Table.Columns.Contains("SectionId") && row["SectionId"] != DBNull.Value
                    ? row["SectionId"].ToString() : "0",
                SectionName = row.Table.Columns.Contains("SectionName") && row["SectionName"] != DBNull.Value
                    ? row["SectionName"].ToString() : "",
                TaxablePer = row.Table.Columns.Contains("TaxablePer") && row["TaxablePer"] != DBNull.Value
                    ? row["TaxablePer"].ToString() : "",
                TaxPer = row.Table.Columns.Contains("TaxPer") && row["TaxPer"] != DBNull.Value
                    ? row["TaxPer"].ToString() : "",
                CessPer = row.Table.Columns.Contains("CessPer") && row["CessPer"] != DBNull.Value
                    ? row["CessPer"].ToString() : "",
                EDCess = row.Table.Columns.Contains("EDCess") && row["EDCess"] != DBNull.Value
                    ? row["EDCess"].ToString() : "",
                HEDCess = row.Table.Columns.Contains("HEDCess") && row["HEDCess"] != DBNull.Value
                    ? row["HEDCess"].ToString() : "",
                NetTax = row.Table.Columns.Contains("NetTax") && row["NetTax"] != DBNull.Value
                    ? row["NetTax"].ToString() : "",
                Limit = row.Table.Columns.Contains("Limit") && row["Limit"] != DBNull.Value
                    ? row["Limit"].ToString() : "",
                Status = row.Table.Columns.Contains("Status") && row["Status"] != DBNull.Value
                    ? row["Status"].ToString() : "1"
            };
        }
    }
}

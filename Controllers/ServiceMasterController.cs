using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;
using VGN_CRM_CORE.Models;

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    public class ServiceMasterController : Controller
    {
        private readonly string _connPROJ;
        private const string SP_NAME = "Web_SaveProjectServiceMas";
        private const string SP_SERVICE_TYPE = "Web_SaveProjectServiceTypeMas";

        public ServiceMasterController(IConfiguration configuration)
        {
            _connPROJ = configuration.GetActiveConnectionString("connPROJ")
                        ?? configuration.GetConnectionString("ConnPROJ")
                        ?? configuration.GetConnectionString("connPROJ");
        }

        // GET: /ServiceMaster or /ServiceMaster/Index
        [HttpGet]
        [Route("/ServiceMaster")]
        [Route("/ServiceMaster/Index")]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.Title = "Service Master — VGN ERP";
            ViewBag.ActiveMenu = "Service Master";
            ViewBag.UserId = user.UserId;
            ViewBag.UserName = user.UserName;

            // Ensure Menu Registration in Session
            var menus = SessionHelper.GetMenuList(HttpContext.Session);
            if (menus != null && !menus.Any(m => m.ControllerName == "ServiceMaster"))
            {
                var boqMenu = menus.FirstOrDefault(m => m.ControllerName == "ProjectIOW");
                menus.Add(new MenuModel
                {
                    Department = boqMenu != null ? boqMenu.Department : "PROJECTS",
                    ModuleType = "MASTER",
                    ModuleCaptionName = "Service Master",
                    ControllerName = "ServiceMaster",
                    ActionName = "Index"
                });
                SessionHelper.SetMenuList(HttpContext.Session, menus);
            }

            return View();
        }

        // GET: /ServiceMaster/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = new List<ServiceModel>();
            var uomDict = await GetUomDictionary();
            var serviceTypeDict = await GetServiceTypeDictionary();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Action", "Fetch");

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow r in dt.Rows)
                        {
                            string uId = r.Table.Columns.Contains("UnitId") && r["UnitId"] != DBNull.Value ? r["UnitId"].ToString() : "";
                            string uName = "";
                            if (!string.IsNullOrEmpty(uId) && uomDict.ContainsKey(uId))
                                uName = uomDict[uId];
                            else if (!string.IsNullOrEmpty(uId))
                                uName = "Unit #" + uId;

                            string stId = r.Table.Columns.Contains("ServiceTypeId") && r["ServiceTypeId"] != DBNull.Value ? r["ServiceTypeId"].ToString() : "1";
                            string stName = "";
                            if (!string.IsNullOrEmpty(stId) && serviceTypeDict.ContainsKey(stId))
                                stName = serviceTypeDict[stId];
                            else if (!string.IsNullOrEmpty(stId))
                                stName = "Type #" + stId;

                            list.Add(new ServiceModel
                            {
                                ServiceId = r["ServiceId"] != DBNull.Value ? Convert.ToInt32(r["ServiceId"]) : 0,
                                Servicecode = r["Servicecode"] != DBNull.Value ? r["Servicecode"].ToString() : "",
                                ServiceName = r["ServiceName"] != DBNull.Value ? r["ServiceName"].ToString() : "",
                                ServiceTypeId = stId,
                                ServiceTypeName = stName,
                                UnitId = uId,
                                UnitName = uName,
                                Status = r["Status"] != DBNull.Value ? r["Status"].ToString() : "1"
                            });
                        }
                    }
                }

                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, data = new List<ServiceModel>() });
            }
        }

        // GET: /ServiceMaster/GetById?id=1
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var serviceTypeDict = await GetServiceTypeDictionary();

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Action", "FetchById");
                    cmd.Parameters.AddWithValue("@ServiceId", id);

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            string stId = dr["ServiceTypeId"] != DBNull.Value ? dr["ServiceTypeId"].ToString() : "1";
                            string stName = serviceTypeDict.ContainsKey(stId) ? serviceTypeDict[stId] : ("Type #" + stId);

                            var item = new ServiceModel
                            {
                                ServiceId = dr["ServiceId"] != DBNull.Value ? Convert.ToInt32(dr["ServiceId"]) : 0,
                                Servicecode = dr["Servicecode"] != DBNull.Value ? dr["Servicecode"].ToString() : "",
                                ServiceName = dr["ServiceName"] != DBNull.Value ? dr["ServiceName"].ToString() : "",
                                ServiceTypeId = stId,
                                ServiceTypeName = stName,
                                UnitId = dr["UnitId"] != DBNull.Value ? dr["UnitId"].ToString() : "",
                                Status = dr["Status"] != DBNull.Value ? dr["Status"].ToString() : "1"
                            };
                            return Json(new { success = true, data = item });
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

        // POST: /ServiceMaster/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] ServiceModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired. Please log in again." });

                if (string.IsNullOrWhiteSpace(req?.ServiceName))
                    return Json(new { success = false, message = "Service Name / Description is required." });

                bool isInsert = !req.ServiceId.HasValue || req.ServiceId.Value <= 0;
                string action = isInsert ? "Insert" : "Update";

                string code = !string.IsNullOrWhiteSpace(req.Servicecode)
                    ? req.Servicecode.Trim()
                    : "SVC-" + DateTime.Now.Ticks.ToString().Substring(11);

                string ipAddress = SessionHelper.GetClientIPAddress(Request);
                string hostName = SessionHelper.GetClientHostName(ipAddress);

                int result = 0;
                string message = "";
                int savedId = 0;

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Action", action);
                    cmd.Parameters.AddWithValue("@ServiceId", isInsert ? (object)DBNull.Value : req.ServiceId.Value);
                    cmd.Parameters.AddWithValue("@Servicecode", code);
                    cmd.Parameters.AddWithValue("@ServiceName", req.ServiceName.Trim());
                    cmd.Parameters.AddWithValue("@ServiceTypeId", string.IsNullOrWhiteSpace(req.ServiceTypeId) ? "1" : req.ServiceTypeId);
                    cmd.Parameters.AddWithValue("@UnitId", string.IsNullOrWhiteSpace(req.UnitId) ? "4" : req.UnitId);

                    if (isInsert)
                    {
                        cmd.Parameters.AddWithValue("@CreatedBy", user.UserId ?? "User");
                        cmd.Parameters.AddWithValue("@UpdatedBy", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@CreatedBy", DBNull.Value);
                        cmd.Parameters.AddWithValue("@UpdatedBy", user.UserId ?? "User");
                    }

                    cmd.Parameters.AddWithValue("@IPAddress", (object)ipAddress ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HostName", (object)hostName ?? DBNull.Value);
                    string statusVal = (req.Status == "0" || req.Status == "Inactive") ? "0" : "1";
                    cmd.Parameters.AddWithValue("@Status", statusVal);

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            result = dr["Result"] != DBNull.Value ? Convert.ToInt32(dr["Result"]) : 0;
                            message = dr["Message"] != DBNull.Value ? dr["Message"].ToString() : "";

                            for (int i = 0; i < dr.FieldCount; i++)
                            {
                                if (string.Equals(dr.GetName(i), "ServiceId", StringComparison.OrdinalIgnoreCase) && dr[i] != DBNull.Value)
                                {
                                    int.TryParse(dr[i].ToString(), out savedId);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (result == 1)
                {
                    return Json(new
                    {
                        success = true,
                        serviceId = savedId > 0 ? savedId : req.ServiceId,
                        serviceCode = code,
                        serviceName = req.ServiceName.Trim(),
                        message = message
                    });
                }
                else
                {
                    return Json(new { success = false, message = !string.IsNullOrEmpty(message) ? message : "Failed to save service." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /ServiceMaster/Delete?id=1
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired. Please log in again." });

                if (id <= 0)
                    return Json(new { success = false, message = "Invalid Service ID." });

                string ipAddress = SessionHelper.GetClientIPAddress(Request);
                string hostName = SessionHelper.GetClientHostName(ipAddress);

                int result = 0;
                string message = "";

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Action", "Delete");
                    cmd.Parameters.AddWithValue("@ServiceId", id);
                    cmd.Parameters.AddWithValue("@UpdatedBy", user.UserId ?? "User");
                    cmd.Parameters.AddWithValue("@IPAddress", (object)ipAddress ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HostName", (object)hostName ?? DBNull.Value);

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            result = dr["Result"] != DBNull.Value ? Convert.ToInt32(dr["Result"]) : 0;
                            message = dr["Message"] != DBNull.Value ? dr["Message"].ToString() : "";
                        }
                    }
                }

                return Json(new { success = result == 1, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /ServiceMaster/GetUOMList
        // Always load units from UOM table via stored procedure dbo.Web_GetUOMList
        [HttpGet]
        public async Task<IActionResult> GetUOMList()
        {
            var list = new List<UOMModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("dbo.Web_GetUOMList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    await con.OpenAsync();
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        foreach (DataRow r in dt.Rows)
                        {
                            list.Add(new UOMModel
                            {
                                UnitId = r["UnitId"] != DBNull.Value ? Convert.ToInt32(r["UnitId"]) : 0,
                                UnitName = r["UnitName"] != DBNull.Value ? r["UnitName"].ToString() : ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, data = new List<UOMModel>() });
            }

            return Json(new { success = true, data = list });
        }

        // GET: /ServiceMaster/GetNextCode
        // Generate next sequence code via stored procedure Web_SaveProjectServiceMas @Action = 'GetNextCode'
        [HttpGet]
        public async Task<IActionResult> GetNextCode()
        {
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_NAME, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Action", "GetNextCode");

                    await con.OpenAsync();
                    var res = await cmd.ExecuteScalarAsync();
                    string code = res != null && res != DBNull.Value ? res.ToString() : "SVC-0001";
                    return Json(new { success = true, nextCode = code });
                }
            }
            catch
            {
                return Json(new { success = true, nextCode = "SVC-" + DateTime.Now.Ticks.ToString().Substring(11) });
            }
        }

        private async Task<Dictionary<string, string>> GetUomDictionary()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("dbo.Web_GetUOMList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    await con.OpenAsync();
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            string id = dr["UnitId"] != DBNull.Value ? dr["UnitId"].ToString() : "";
                            string name = dr["UnitName"] != DBNull.Value ? dr["UnitName"].ToString() : "";
                            if (!string.IsNullOrEmpty(id) && !dict.ContainsKey(id))
                                dict[id] = name;
                        }
                    }
                }
            }
            catch
            {
                // Leave dict empty if SP fails; will display Unit #id
            }

            return dict;
        }

        // GET: /ServiceMaster/GetServiceTypeList
        // Load service types via stored procedure dbo.Web_SaveProjectServiceTypeMas @Action = 'FETCH'
        [HttpGet]
        public async Task<IActionResult> GetServiceTypeList()
        {
            var list = new List<ServiceTypeMasterModel>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_SERVICE_TYPE, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Action", "FETCH");

                    await con.OpenAsync();
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        foreach (DataRow r in dt.Rows)
                        {
                            list.Add(new ServiceTypeMasterModel
                            {
                                ServiceTypeId = r["ServiceTypeId"] != DBNull.Value ? Convert.ToInt32(r["ServiceTypeId"]) : 0,
                                ServiceTypeName = r["ServiceTypeName"] != DBNull.Value ? r["ServiceTypeName"].ToString() : "",
                                Status = r["Status"] != DBNull.Value ? r["Status"].ToString() : "1"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, data = new List<ServiceTypeMasterModel>() });
            }

            return Json(new { success = true, data = list });
        }

        // POST: /ServiceMaster/SaveServiceType
        // Add new service type via stored procedure dbo.Web_SaveProjectServiceTypeMas @Action = 'INSERT'
        [HttpPost]
        public async Task<IActionResult> SaveServiceType([FromBody] ServiceTypeMasterModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { success = false, message = "Session expired. Please log in again." });

                if (string.IsNullOrWhiteSpace(req?.ServiceTypeName))
                    return Json(new { success = false, message = "Service Type Name cannot be blank." });

                string ipAddress = SessionHelper.GetClientIPAddress(Request);
                string hostName = SessionHelper.GetClientHostName(ipAddress);

                int result = 0;
                string message = "";
                int savedId = 0;

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_SERVICE_TYPE, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Action", "INSERT");
                    cmd.Parameters.AddWithValue("@ServiceTypeName", req.ServiceTypeName.Trim());
                    cmd.Parameters.AddWithValue("@CreatedBy", user.UserId ?? "User");
                    cmd.Parameters.AddWithValue("@IPAddress", (object)ipAddress ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HostName", (object)hostName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", "1");

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            result = dr["Result"] != DBNull.Value ? Convert.ToInt32(dr["Result"]) : 0;
                            message = dr["Message"] != DBNull.Value ? dr["Message"].ToString() : "";
                            for (int i = 0; i < dr.FieldCount; i++)
                            {
                                if (string.Equals(dr.GetName(i), "ServiceTypeId", StringComparison.OrdinalIgnoreCase) && dr[i] != DBNull.Value)
                                {
                                    int.TryParse(dr[i].ToString(), out savedId);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (result == 1)
                {
                    return Json(new
                    {
                        success = true,
                        serviceTypeId = savedId,
                        serviceTypeName = req.ServiceTypeName.Trim(),
                        message = message
                    });
                }
                else
                {
                    return Json(new { success = false, message = !string.IsNullOrEmpty(message) ? message : "Failed to save service type." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<Dictionary<string, string>> GetServiceTypeDictionary()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand(SP_SERVICE_TYPE, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Action", "FETCH");
                    await con.OpenAsync();
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            string id = dr["ServiceTypeId"] != DBNull.Value ? dr["ServiceTypeId"].ToString() : "";
                            string name = dr["ServiceTypeName"] != DBNull.Value ? dr["ServiceTypeName"].ToString() : "";
                            if (!string.IsNullOrEmpty(id) && !dict.ContainsKey(id))
                                dict[id] = name;
                        }
                    }
                }
            }
            catch
            {
                // Leave dict empty if SP fails; will display Type #id
            }

            return dict;
        }
    }
}

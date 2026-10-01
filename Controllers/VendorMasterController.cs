using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;
using VGN_CRM_CORE.Models;

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    public class VendorMasterController : Controller
    {
        private readonly string _connPROJ;
        private readonly IConfiguration _configuration;

        public VendorMasterController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connPROJ = configuration.GetActiveConnectionString("connPROJ")
                        ?? configuration.GetConnectionString("ConnPROJ")
                        ?? configuration.GetConnectionString("connPROJ");
        }

        // GET: /VendorMaster, /VendorCreation
        [HttpGet]
        [Route("/VendorMaster")]
        [Route("/VendorMaster/Index")]
        [Route("/VendorCreation")]
        [Route("/VendorCreation/Index")]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.Title = "Vendor Creation — VGN ERP";
            ViewBag.ActiveMenu = "Vendor Creation";
            ViewBag.UserId = user.UserId;
            ViewBag.UserName = user.UserName;

            // Ensure Menu Registration in Session
            var menus = SessionHelper.GetMenuList(HttpContext.Session);
            if (menus != null)
            {
                var boqMenu = menus.FirstOrDefault(m => m.ControllerName == "ProjectIOW");
                var vmItem = menus.FirstOrDefault(m => m.ControllerName == "VendorMaster");
                if (vmItem != null)
                {
                    vmItem.ModuleCaptionName = "Vendor Creation";
                    vmItem.Department = boqMenu != null ? boqMenu.Department : "PROJECTS";
                    vmItem.ModuleType = boqMenu != null ? boqMenu.ModuleType : "MASTER";
                }
                else
                {
                    menus.Add(new MenuModel
                    {
                        Department = boqMenu != null ? boqMenu.Department : "PROJECTS",
                        ModuleType = boqMenu != null ? boqMenu.ModuleType : "MASTER",
                        ModuleCaptionName = "Vendor Creation",
                        ControllerName = "VendorMaster",
                        ActionName = "Index"
                    });
                }
                SessionHelper.SetMenuList(HttpContext.Session, menus);
            }

            return View();
        }

        // GET: /VendorMaster/GetVendors or /VendorMaster/GetVendorList
        [HttpGet]
        [Route("/VendorMaster/GetVendors")]
        [Route("/VendorMaster/GetVendorList")]
        public async Task<IActionResult> GetVendors(string search, string searchTerm, string vendorTypeId, string vendorCategoryId, string status)
        {
            var vendors = new List<VendorMasterModel>();
            var s = !string.IsNullOrWhiteSpace(search) ? search : searchTerm;

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("sp_VendorMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GET_ALL");
                    cmd.Parameters.AddWithValue("@SearchTerm", (object)s ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@VendorTypeId", (object)vendorTypeId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@VendorCategoryId", (object)vendorCategoryId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FilterStatus", (object)status ?? DBNull.Value);

                    await con.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            vendors.Add(MapVendorSummary(reader));
                        }
                    }
                }

                return Json(new { success = true, data = vendors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading vendors: " + ex.Message, data = vendors });
            }
        }

        // GET: /VendorMaster/GetVendorById?id=1
        [HttpGet]
        public async Task<IActionResult> GetVendorById(long id)
        {
            if (id <= 0)
                return Json(new { success = false, message = "Invalid Vendor ID" });

            try
            {
                VendorMasterModel vendor = null;

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("sp_VendorMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GET_BY_ID");
                    cmd.Parameters.AddWithValue("@VendorId", id);

                    await con.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // Result Set 1: Vendor Details
                        if (await reader.ReadAsync())
                        {
                            vendor = MapFullVendor(reader);
                        }

                        // Result Set 2: Multiple Bank Accounts
                        if (vendor != null && await reader.NextResultAsync())
                        {
                            vendor.Banks = new List<VendorBankModel>();
                            while (await reader.ReadAsync())
                            {
                                vendor.Banks.Add(new VendorBankModel
                                {
                                    VendorBankId = Convert.ToInt64(reader["VendorBankId"]),
                                    VendorId = Convert.ToInt64(reader["VendorId"]),
                                    AccountHolderName = reader["AccountHolderName"] != DBNull.Value ? reader["AccountHolderName"].ToString() : "",
                                    BankName = reader["BankName"] != DBNull.Value ? reader["BankName"].ToString() : "",
                                    AccountNo = reader["AccountNo"] != DBNull.Value ? reader["AccountNo"].ToString() : "",
                                    AccountType = reader["AccountType"] != DBNull.Value ? reader["AccountType"].ToString() : "Current A/c",
                                    IFSCCode = reader["IFSCCode"] != DBNull.Value ? reader["IFSCCode"].ToString() : "",
                                    BranchName = reader["BranchName"] != DBNull.Value ? reader["BranchName"].ToString() : "",
                                    BranchCode = reader["BranchCode"] != DBNull.Value ? reader["BranchCode"].ToString() : "",
                                    MICRCode = reader["MICRCode"] != DBNull.Value ? reader["MICRCode"].ToString() : "",
                                    DefaultBank = reader["DefaultBank"] != DBNull.Value ? reader["DefaultBank"].ToString() : "No",
                                    Status = (reader["Status"] != DBNull.Value && (reader["Status"].ToString() == "0" || reader["Status"].ToString() == "Inactive")) ? "0" : "1"
                                });
                            }
                        }

                        // Result Set 3: Multiple Branches
                        if (vendor != null && await reader.NextResultAsync())
                        {
                            vendor.Branches = new List<VendorBranchModel>();
                            while (await reader.ReadAsync())
                            {
                                vendor.Branches.Add(new VendorBranchModel
                                {
                                    VendorBranchId = Convert.ToInt64(reader["VendorBranchId"]),
                                    VendorId = Convert.ToInt64(reader["VendorId"]),
                                    BranchName = reader["BranchName"] != DBNull.Value ? reader["BranchName"].ToString() : "",
                                    Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : "",
                                    City = reader["City"] != DBNull.Value ? reader["City"].ToString() : "",
                                    Pincode = reader["Pincode"] != DBNull.Value ? reader["Pincode"].ToString() : "",
                                    PhoneNo = reader["PhoneNo"] != DBNull.Value ? reader["PhoneNo"].ToString() : "",
                                    TINNo = reader["TINNo"] != DBNull.Value ? reader["TINNo"].ToString() : "",
                                    ChequeNo = reader["ChequeNo"] != DBNull.Value ? reader["ChequeNo"].ToString() : "",
                                    GSTIn = reader["GSTIn"] != DBNull.Value ? reader["GSTIn"].ToString() : "",
                                    Status = (reader["Status"] != DBNull.Value && (reader["Status"].ToString() == "0" || reader["Status"].ToString() == "Inactive")) ? "0" : "1"
                                });
                            }
                        }
                    }
                }

                if (vendor == null)
                    return Json(new { success = false, message = "Vendor not found" });

                return Json(new { success = true, data = vendor });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading vendor details: " + ex.Message });
            }
        }

        // POST: /VendorMaster/SaveVendor
        [HttpPost]
        public async Task<IActionResult> SaveVendor([FromBody] VendorMasterModel model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid vendor data payload" });

            if (string.IsNullOrWhiteSpace(model.VendorName))
                return Json(new { success = false, message = "Vendor Name is required" });

            if (string.IsNullOrWhiteSpace(model.EmailId))
                return Json(new { success = false, message = "Primary Official Email is required" });

            if (string.IsNullOrWhiteSpace(model.PanNo))
                return Json(new { success = false, message = "PAN Number is required" });

            if (string.IsNullOrWhiteSpace(model.PanType))
                return Json(new { success = false, message = "PAN Type is required" });

            if (string.IsNullOrWhiteSpace(model.ContactPerson))
                return Json(new { success = false, message = "Contact Person Name is required" });

            if (string.IsNullOrWhiteSpace(model.ContactNo))
                return Json(new { success = false, message = "Contact Phone Number is required" });

            var user = SessionHelper.GetUserSession(HttpContext.Session);
            var userName = user != null ? user.UserName : "System";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var hostName = Environment.MachineName;

            try
            {
                var statusVal = (model.Status == "0" || model.Status?.ToLower() == "inactive") ? "0" : "1";
                model.Status = statusVal;

                if (model.Banks != null)
                {
                    foreach (var b in model.Banks)
                    {
                        b.Status = (b.Status == "0" || b.Status?.ToLower() == "inactive") ? "0" : "1";
                    }
                }

                if (model.Branches != null)
                {
                    foreach (var br in model.Branches)
                    {
                        br.Status = (br.Status == "0" || br.Status?.ToLower() == "inactive") ? "0" : "1";
                    }
                }

                var banksJson = model.Banks != null && model.Banks.Count > 0
                    ? JsonSerializer.Serialize(model.Banks)
                    : null;

                var branchesJson = model.Branches != null && model.Branches.Count > 0
                    ? JsonSerializer.Serialize(model.Branches)
                    : null;

                long returnedVendorId = 0;
                string returnedVendorCode = "";

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("sp_VendorMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "SAVE");
                    cmd.Parameters.AddWithValue("@VendorId", model.VendorId);
                    cmd.Parameters.AddWithValue("@VendorCode", (object)model.VendorCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@VendorName", model.VendorName.Trim());
                    cmd.Parameters.AddWithValue("@VendorShortName", (object)model.VendorShortName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@VendorTypeId", (object)model.VendorTypeId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@VendorCategoryId", (object)model.VendorCategoryId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@WorkNature", (object)model.WorkNature ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SupplyType", (object)model.SupplyType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ConsultantType", (object)model.ConsultantType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CompnayType", (object)model.CompnayType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PanNo", (object)model.PanNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PanType", (object)model.PanType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AadharNo", (object)model.AadharNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmailId", (object)model.EmailId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MSME", (object)model.MSME ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MSMENo", (object)model.MSMENo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Website", (object)model.Website ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", (object)model.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@City", (object)model.City ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@State", (object)model.State ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Country", string.IsNullOrWhiteSpace(model.Country) ? "India" : model.Country);
                    cmd.Parameters.AddWithValue("@Pincode", (object)model.Pincode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PhoneNumber", (object)model.PhoneNumber ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ChequeName", (object)model.ChequeName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Category", (object)model.Category ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OrgType", (object)model.OrgType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ContactPerson", (object)model.ContactPerson ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ContactNo", (object)model.ContactNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ContactAddress", (object)model.ContactAddress ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Designation", (object)model.Designation ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@YearofEstablishment", (object)model.YearofEstablishment ?? "0");
                    cmd.Parameters.AddWithValue("@ESINo", (object)model.ESINo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EPFNo", (object)model.EPFNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@GSTIn", (object)model.GSTIn ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ARNNo", (object)model.ARNNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@VendorType", (object)model.VendorType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@GSTFiling", (object)model.GSTFiling ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShopEstablishmentNo", (object)model.ShopEstablishmentNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LWF", (object)model.LWF ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PTaxRegNo", (object)model.PTaxRegNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TDSAccNo", (object)model.TDSAccNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ITPANNo", (object)model.ITPANNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DutyFree", string.IsNullOrWhiteSpace(model.DutyFree) ? "No" : model.DutyFree);
                    cmd.Parameters.AddWithValue("@VATNo", (object)model.VATNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@VATRenewalNo", (object)model.VATRenewalNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CRNo", (object)model.CRNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OCCINo", (object)model.OCCINo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ApprovalStatus", string.IsNullOrWhiteSpace(model.ApprovalStatus) ? "Approved" : model.ApprovalStatus);
                    cmd.Parameters.AddWithValue("@CreatedBy", userName);
                    cmd.Parameters.AddWithValue("@UpdatedBy", userName);
                    cmd.Parameters.AddWithValue("@IPAddress", ipAddress);
                    cmd.Parameters.AddWithValue("@HostName", hostName);
                    cmd.Parameters.AddWithValue("@Status", statusVal);
                    cmd.Parameters.AddWithValue("@BanksJson", (object)banksJson ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BranchesJson", (object)branchesJson ?? DBNull.Value);

                    await con.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            returnedVendorId = Convert.ToInt64(reader["VendorId"]);
                            returnedVendorCode = reader["VendorCode"] != DBNull.Value ? reader["VendorCode"].ToString() : "";
                        }
                    }
                }

                return Json(new
                {
                    success = true,
                    vendorId = returnedVendorId,
                    vendorCode = returnedVendorCode,
                    message = model.VendorId == 0 ? "Vendor created successfully!" : "Vendor updated successfully!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving vendor: " + ex.Message });
            }
        }

        // POST: /VendorMaster/DeleteVendor?id=1
        [HttpPost]
        public async Task<IActionResult> DeleteVendor(long id)
        {
            if (id <= 0)
                return Json(new { success = false, message = "Invalid Vendor ID" });

            var user = SessionHelper.GetUserSession(HttpContext.Session);
            var userName = user != null ? user.UserName : "System";

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("sp_VendorMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "DELETE");
                    cmd.Parameters.AddWithValue("@VendorId", id);
                    cmd.Parameters.AddWithValue("@UpdatedBy", userName);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }

                return Json(new { success = true, message = "Vendor deactivated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deactivating vendor: " + ex.Message });
            }
        }

        // GET: /VendorMaster/GetNextVendorCode
        [HttpGet]
        public async Task<IActionResult> GetNextVendorCode()
        {
            try
            {
                string code = "VND-0001";
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("sp_VendorMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GET_NEXT_CODE");

                    await con.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        code = result.ToString();
                    }
                }
                return Json(new { success = true, code });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, code = "VND-0001", message = ex.Message });
            }
        }

        // GET: /VendorMaster/GetVendorTypes
        [HttpGet]
        public async Task<IActionResult> GetVendorTypes()
        {
            var list = new List<VendorTypeMasterModel>();
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("sp_VendorMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GET_TYPES");

                    await con.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new VendorTypeMasterModel
                            {
                                VendorTypeId = Convert.ToInt32(reader["VendorTypeId"]),
                                VendorTypeCode = reader["VendorTypeCode"] != DBNull.Value ? reader["VendorTypeCode"].ToString() : "",
                                VendorTypeName = reader["VendorTypeName"] != DBNull.Value ? reader["VendorTypeName"].ToString() : ""
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

        // GET: /VendorMaster/GetVendorCategories
        [HttpGet]
        public async Task<IActionResult> GetVendorCategories()
        {
            var list = new List<VendorCategoryMasterModel>();
            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("sp_VendorMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GET_CATEGORIES");

                    await con.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new VendorCategoryMasterModel
                            {
                                VendorCategoryId = Convert.ToInt32(reader["VendorCategoryId"]),
                                VendorCategoryCode = reader["VendorCategoryCode"] != DBNull.Value ? reader["VendorCategoryCode"].ToString() : "",
                                VendorCategoryName = reader["VendorCategoryName"] != DBNull.Value ? reader["VendorCategoryName"].ToString() : ""
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

        // POST: /VendorMaster/QuickAddType
        [HttpPost]
        public async Task<IActionResult> QuickAddType([FromBody] VendorTypeMasterModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.VendorTypeName))
                return Json(new { success = false, message = "Vendor Type Name is required" });

            var user = SessionHelper.GetUserSession(HttpContext.Session);
            var userName = user != null ? user.UserName : "System";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            try
            {
                int newId = 0;
                string newCode = "";
                string newName = "";

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("sp_VendorMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "QUICK_ADD_TYPE");
                    cmd.Parameters.AddWithValue("@VendorTypeName", model.VendorTypeName.Trim());
                    cmd.Parameters.AddWithValue("@VendorTypeCode", (object)model.VendorTypeCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedBy", userName);
                    cmd.Parameters.AddWithValue("@IPAddress", ip);
                    cmd.Parameters.AddWithValue("@HostName", Environment.MachineName);

                    await con.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            newId = Convert.ToInt32(reader["VendorTypeId"]);
                            newCode = reader["VendorTypeCode"] != DBNull.Value ? reader["VendorTypeCode"].ToString() : "";
                            newName = reader["VendorTypeName"] != DBNull.Value ? reader["VendorTypeName"].ToString() : "";
                        }
                    }
                }

                return Json(new { success = true, vendorTypeId = newId, vendorTypeCode = newCode, vendorTypeName = newName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /VendorMaster/QuickAddCategory
        [HttpPost]
        public async Task<IActionResult> QuickAddCategory([FromBody] VendorCategoryMasterModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.VendorCategoryName))
                return Json(new { success = false, message = "Vendor Category Name is required" });

            var user = SessionHelper.GetUserSession(HttpContext.Session);
            var userName = user != null ? user.UserName : "System";

            try
            {
                int newId = 0;
                string newCode = "";
                string newName = "";

                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("sp_VendorMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "QUICK_ADD_CATEGORY");
                    cmd.Parameters.AddWithValue("@VendorCategoryName", model.VendorCategoryName.Trim());
                    cmd.Parameters.AddWithValue("@VendorCategoryCode", (object)model.VendorCategoryCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedBy", userName);

                    await con.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            newId = Convert.ToInt32(reader["VendorCategoryId"]);
                            newCode = reader["VendorCategoryCode"] != DBNull.Value ? reader["VendorCategoryCode"].ToString() : "";
                            newName = reader["VendorCategoryName"] != DBNull.Value ? reader["VendorCategoryName"].ToString() : "";
                        }
                    }
                }

                return Json(new { success = true, vendorCategoryId = newId, vendorCategoryCode = newCode, vendorCategoryName = newName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #region Mappers


        private static string GetColString(SqlDataReader r, string colName, string def = "")
        {
            try
            {
                int idx = r.GetOrdinal(colName);
                return !r.IsDBNull(idx) ? r.GetValue(idx).ToString() : def;
            }
            catch
            {
                return def;
            }
        }

        private static DateTime? GetColDateTime(SqlDataReader r, string colName)
        {
            try
            {
                int idx = r.GetOrdinal(colName);
                return !r.IsDBNull(idx) ? (DateTime?)Convert.ToDateTime(r.GetValue(idx)) : null;
            }
            catch
            {
                return null;
            }
        }

        private static int GetColInt(SqlDataReader r, string colName, int def = 0)
        {
            try
            {
                int idx = r.GetOrdinal(colName);
                return !r.IsDBNull(idx) ? Convert.ToInt32(r.GetValue(idx)) : def;
            }
            catch
            {
                return def;
            }
        }

        private static long GetColLong(SqlDataReader r, string colName, long def = 0)
        {
            try
            {
                int idx = r.GetOrdinal(colName);
                return !r.IsDBNull(idx) ? Convert.ToInt64(r.GetValue(idx)) : def;
            }
            catch
            {
                return def;
            }
        }

        private VendorMasterModel MapVendorSummary(SqlDataReader r)
        {
            var rawStatus = GetColString(r, "Status", "1");
            return new VendorMasterModel
            {
                VendorId = GetColLong(r, "VendorId"),
                VendorCode = GetColString(r, "VendorCode"),
                VendorName = GetColString(r, "VendorName"),
                VendorShortName = GetColString(r, "VendorShortName"),
                VendorTypeId = GetColString(r, "VendorTypeId"),
                VendorTypeName = GetColString(r, "VendorTypeName"),
                VendorCategoryId = GetColString(r, "VendorCategoryId"),
                VendorCategoryName = GetColString(r, "VendorCategoryName"),
                WorkNature = GetColString(r, "WorkNature"),
                SupplyType = GetColString(r, "SupplyType"),
                City = GetColString(r, "City"),
                State = GetColString(r, "State"),
                PhoneNumber = GetColString(r, "PhoneNumber"),
                EmailId = GetColString(r, "EmailId"),
                GSTIn = GetColString(r, "GSTIn"),
                PanNo = GetColString(r, "PanNo"),
                ContactPerson = GetColString(r, "ContactPerson"),
                Status = (rawStatus == "0" || rawStatus.ToLower() == "inactive") ? "0" : "1",
                BranchCount = GetColInt(r, "BranchCount"),
                BankCount = GetColInt(r, "BankCount"),
                PrimaryBankName = GetColString(r, "PrimaryBankName"),
                PrimaryAccountNo = GetColString(r, "PrimaryAccountNo")
            };
        }

        private VendorMasterModel MapFullVendor(SqlDataReader r)
        {
            var rawStatus = GetColString(r, "Status", "1");
            return new VendorMasterModel
            {
                VendorId = GetColLong(r, "VendorId"),
                VendorCode = GetColString(r, "VendorCode"),
                VendorName = GetColString(r, "VendorName"),
                VendorShortName = GetColString(r, "VendorShortName"),
                VendorTypeId = GetColString(r, "VendorTypeId"),
                VendorTypeName = GetColString(r, "VendorTypeName"),
                VendorCategoryId = GetColString(r, "VendorCategoryId"),
                VendorCategoryName = GetColString(r, "VendorCategoryName"),
                WorkNature = GetColString(r, "WorkNature"),
                SupplyType = GetColString(r, "SupplyType"),
                ConsultantType = GetColString(r, "ConsultantType"),
                PanNo = GetColString(r, "PanNo"),
                PanType = GetColString(r, "PanType"),
                AadharNo = GetColString(r, "AadharNo"),
                CompnayType = GetColString(r, "CompnayType"),
                EmailId = GetColString(r, "EmailId"),
                MSME = GetColString(r, "MSME", "No"),
                MSMENo = GetColString(r, "MSMENo"),
                Website = GetColString(r, "Website"),

                Address = GetColString(r, "Address"),
                City = GetColString(r, "City"),
                State = GetColString(r, "State"),
                Country = GetColString(r, "Country", "India"),
                Pincode = GetColString(r, "Pincode"),
                PhoneNumber = GetColString(r, "PhoneNumber"),
                ChequeName = GetColString(r, "ChequeName"),
                Category = GetColString(r, "Category"),
                OrgType = GetColString(r, "OrgType"),

                ContactPerson = GetColString(r, "ContactPerson"),
                ContactNo = GetColString(r, "ContactNo"),
                ContactAddress = GetColString(r, "ContactAddress"),
                Designation = GetColString(r, "Designation"),

                YearofEstablishment = GetColString(r, "YearofEstablishment", "0"),
                ESINo = GetColString(r, "ESINo"),
                EPFNo = GetColString(r, "EPFNo"),
                GSTIn = GetColString(r, "GSTIn"),
                ARNNo = GetColString(r, "ARNNo"),
                VendorType = GetColString(r, "VendorType"),
                GSTFiling = GetColString(r, "GSTFiling"),
                ShopEstablishmentNo = GetColString(r, "ShopEstablishmentNo"),
                LWF = GetColString(r, "LWF"),
                PTaxRegNo = GetColString(r, "PTaxRegNo"),
                TDSAccNo = GetColString(r, "TDSAccNo"),
                ITPANNo = GetColString(r, "ITPANNo"),
                DutyFree = GetColString(r, "DutyFree", "No"),
                VATNo = GetColString(r, "VATNo"),
                VATRenewalNo = GetColString(r, "VATRenewalNo"),
                CRNo = GetColString(r, "CRNo"),
                OCCINo = GetColString(r, "OCCINo"),

                ApprovalStatus = GetColString(r, "ApprovalStatus", "Approved"),
                CreatedBy = GetColString(r, "CreatedBy"),
                CreatedDate = GetColDateTime(r, "CreatedDate"),
                UpdatedBy = GetColString(r, "UpdatedBy"),
                UpdatedDate = GetColDateTime(r, "UpdatedDate"),
                IPAddress = GetColString(r, "IPAddress"),
                HostName = GetColString(r, "HostName"),
                Status = (rawStatus == "0" || rawStatus.ToLower() == "inactive") ? "0" : "1"
            };
        }

        #endregion
    }
}

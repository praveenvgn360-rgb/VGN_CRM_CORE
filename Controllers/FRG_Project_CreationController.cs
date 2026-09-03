using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    public class FRG_Project_CreationController : Controller
    {
        private readonly string _connPROJ;

        public FRG_Project_CreationController(IConfiguration configuration)
        {
            _connPROJ = configuration.GetActiveConnectionString("connPROJ")
                        ?? configuration.GetConnectionString("ConnPROJ")
                        ?? configuration.GetConnectionString("connPROJ");
        }

        public IActionResult Index(int? id = null, string mode = null)
        {
            // If editing an existing project OR explicitly requested create mode, open Form screen
            if ((id.HasValue && id.Value > 0) || string.Equals(mode, "create", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ProjectKickoffId = id;
                return View("Index");
            }

            // Default first-time load: Show Projects Directory List Page
            return View("ProjectView");
        }

        [HttpGet]
        public IActionResult Create(int? id = null)
        {
            ViewBag.ProjectKickoffId = id;
            return View("Index");
        }

        public IActionResult ProjectView()
        {
            return View("ProjectView");
        }

        public IActionResult ViewProjects()
        {
            return View("ProjectView");
        }

        public IActionResult List()
        {
            return View("ProjectView");
        }

        #region Lookup Endpoints (Stored Procedure: Web_LoadProjectKickoff)

        [HttpGet]
        public async Task<IActionResult> LoadCompany()
        {
            var companyList = new List<object>();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("Web_LoadProjectKickoff", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", "0");
                    cmd.Parameters.AddWithValue("@Flag", "Company_Name");

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            companyList.Add(new
                            {
                                CompanyId = dr["CompanyId"].ToString(),
                                CompanyName = dr["CompanyName"].ToString()
                            });
                        }
                    }
                }

                return Json(companyList);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoadBusinessType()
        {
            var dt = new DataTable();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("Web_LoadProjectKickoff", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", "0");
                    cmd.Parameters.AddWithValue("@Flag", "Business_Type");

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                var list = dt.AsEnumerable().Select(x => new
                {
                    BusinessTypeId = x["BusinessTypeId"].ToString(),
                    BusinessTypeName = x["BusinessTypeName"].ToString()
                }).ToList();

                return Json(list);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoadProjectType()
        {
            var dt = new DataTable();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("Web_LoadProjectKickoff", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", "0");
                    cmd.Parameters.AddWithValue("@Flag", "Project_Type");

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                var list = dt.AsEnumerable().Select(x => new
                {
                    ProjectTypeId = x["ProjectTypeId"].ToString(),
                    ProjectTypeName = x["ProjectTypeName"].ToString()
                }).ToList();

                return Json(list);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoadAreaIn()
        {
            var dt = new DataTable();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("Web_LoadProjectKickoff", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", "0");
                    cmd.Parameters.AddWithValue("@Flag", "Area_In");

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                var list = dt.AsEnumerable().Select(x => new
                {
                    UnitId = x["UnitId"].ToString(),
                    UnitName = x["UnitName"].ToString()
                }).ToList();

                return Json(list);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoadSoilType()
        {
            var dt = new DataTable();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("Web_LoadProjectKickoff", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", "0");
                    cmd.Parameters.AddWithValue("@Flag", "Soil_Type");

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                var list = dt.AsEnumerable().Select(x => new
                {
                    SoilTypeId = x["SoilTypeId"].ToString(),
                    SoilType = x["SoilType"].ToString()
                }).ToList();

                return Json(list);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoadUsers()
        {
            var dt = new DataTable();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("Web_LoadProjectKickoff", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", "0");
                    cmd.Parameters.AddWithValue("@Flag", "User");

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                var list = dt.AsEnumerable().Select(x => new
                {
                    UserId = x["UserId"].ToString(),
                    UserText = x["UserName"].ToString() + " ( " + x["EmployeeName"].ToString() + " )"
                }).ToList();

                return Json(list);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Fetch Project Endpoint (Stored Procedure: Web_SaveProjectKickOffMas with @Action='FETCH')

        [HttpGet]
        public async Task<IActionResult> FetchProjects(int? id = null)
        {
            var dt = new DataTable();

            try
            {
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("Web_SaveProjectKickOffMas", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "FETCH");
                    cmd.Parameters.AddWithValue("@ProjectKickoffId", (object)id ?? DBNull.Value);

                    await con.OpenAsync();

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                // If id is provided, return single project object
                if (id.HasValue && id.Value > 0)
                {
                    if (dt.Rows.Count > 0)
                    {
                        var row = dt.Rows[0];
                        var dict = new Dictionary<string, object>();
                        foreach (DataColumn col in dt.Columns)
                        {
                            dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                        }

                        // Parse allocated Users array from LogUserId
                        if (dict.ContainsKey("LogUserId") && dict["LogUserId"] != null)
                        {
                            var rawUsers = dict["LogUserId"].ToString();
                            dict["Users"] = rawUsers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(u => u.Trim())
                                                    .ToList();
                        }
                        else
                        {
                            dict["Users"] = new List<string>();
                        }

                        return Content(JsonConvert.SerializeObject(new { success = true, data = dict }), "application/json");
                    }
                    return Content(JsonConvert.SerializeObject(new { success = false, message = "Project not found." }), "application/json");
                }

                // Return full list for dxDataGrid with enriched Company and Project Type names
                var companyDict = new Dictionary<string, string>();
                var projectTypeDict = new Dictionary<string, string>();

                try
                {
                    using (var conLookup = new SqlConnection(_connPROJ))
                    {
                        await conLookup.OpenAsync();

                        using (var cmdComp = new SqlCommand("Web_LoadProjectKickoff", conLookup))
                        {
                            cmdComp.CommandType = CommandType.StoredProcedure;
                            cmdComp.Parameters.AddWithValue("@CompanyId", "0");
                            cmdComp.Parameters.AddWithValue("@Flag", "Company_Name");
                            using (var daComp = new SqlDataAdapter(cmdComp))
                            {
                                var dtComp = new DataTable();
                                daComp.Fill(dtComp);
                                foreach (DataRow r in dtComp.Rows)
                                {
                                    var cId = r["CompanyId"].ToString().Trim();
                                    var cName = r["CompanyName"].ToString().Trim();
                                    if (cId != "0" && !companyDict.ContainsKey(cId))
                                    {
                                        companyDict[cId] = cName;
                                    }
                                }
                            }
                        }

                        using (var cmdPt = new SqlCommand("Web_LoadProjectKickoff", conLookup))
                        {
                            cmdPt.CommandType = CommandType.StoredProcedure;
                            cmdPt.Parameters.AddWithValue("@CompanyId", "0");
                            cmdPt.Parameters.AddWithValue("@Flag", "Project_Type");
                            using (var daPt = new SqlDataAdapter(cmdPt))
                            {
                                var dtPt = new DataTable();
                                daPt.Fill(dtPt);
                                foreach (DataRow r in dtPt.Rows)
                                {
                                    var ptId = r["ProjectTypeId"].ToString().Trim();
                                    var ptName = r["ProjectTypeName"].ToString().Trim();
                                    if (ptId != "0" && !projectTypeDict.ContainsKey(ptId))
                                    {
                                        projectTypeDict[ptId] = ptName;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Fallback to raw IDs if lookups fail
                }

                var list = new List<Dictionary<string, object>>();
                foreach (DataRow row in dt.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                    }

                    var cId = row["CompId"] != DBNull.Value ? row["CompId"].ToString().Trim() : "";
                    dict["CompanyName"] = companyDict.ContainsKey(cId) ? companyDict[cId] : (string.IsNullOrWhiteSpace(cId) ? "—" : cId);

                    var ptId = row["ProjectType"] != DBNull.Value ? row["ProjectType"].ToString().Trim() : "";
                    dict["ProjectTypeName"] = projectTypeDict.ContainsKey(ptId) ? projectTypeDict[ptId] : (string.IsNullOrWhiteSpace(ptId) ? "—" : ptId);

                    list.Add(dict);
                }

                return Content(JsonConvert.SerializeObject(list), "application/json");
            }
            catch (Exception ex)
            {
                return Content(JsonConvert.SerializeObject(new { success = false, message = ex.Message }), "application/json");
            }
        }

        #endregion

        #region Save Project Endpoint (Stored Procedure: Web_SaveProjectKickOffMas)

        [HttpPost]
        public async Task<IActionResult> SaveProject([FromForm] ProjectSaveRequest model)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                string logUserId = user != null ? user.UserId : "1";
                string ipAddress = user != null && !string.IsNullOrEmpty(user.IPAddress)
                    ? user.IPAddress
                    : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                string hostName = user != null && !string.IsNullOrEmpty(user.HostName)
                    ? user.HostName
                    : Environment.MachineName;

                // 1. Upload Drawing File if attached
                string drawingFilePath = null;
                string finalDrawingName = model.DrawingName;
                if (model.DrawingFile != null && model.DrawingFile.Length > 0)
                {
                    drawingFilePath = UploadFileToFTP(model.DrawingFile);
                    if (string.IsNullOrWhiteSpace(finalDrawingName))
                    {
                        finalDrawingName = Path.GetFileName(model.DrawingFile.FileName);
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.DrawingDescription))
                {
                    finalDrawingName = string.IsNullOrWhiteSpace(finalDrawingName)
                        ? model.DrawingDescription
                        : $"{finalDrawingName} - {model.DrawingDescription}";
                }

                // 2. Upload Document File if attached
                string docFilePath = null;
                string finalDocName = model.DocName;
                if (model.DocFile != null && model.DocFile.Length > 0)
                {
                    docFilePath = UploadFileToFTP(model.DocFile);
                    if (string.IsNullOrWhiteSpace(finalDocName))
                    {
                        finalDocName = Path.GetFileName(model.DocFile.FileName);
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.DocDescription))
                {
                    finalDocName = string.IsNullOrWhiteSpace(finalDocName)
                        ? model.DocDescription
                        : $"{finalDocName} - {model.DocDescription}";
                }

                // 3. Save to Database via Web_SaveProjectKickOffMas
                using (var con = new SqlConnection(_connPROJ))
                using (var cmd = new SqlCommand("Web_SaveProjectKickOffMas", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Action", string.IsNullOrWhiteSpace(model.Action) ? "INSERT" : model.Action.ToUpper());
                    cmd.Parameters.AddWithValue("@ProjectKickoffId", (object)model.ProjectKickoffId ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@ProjectName", (object)model.ProjectName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProjectSiteName", (object)model.LandName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CompId", (object)model.CompanyId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProjectType", (object)model.ProjectTypeId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProjectLevelType", (object)model.PropertyType ?? DBNull.Value);

                    string addressToSave = !string.IsNullOrWhiteSpace(model.ProjectAddress) ? model.ProjectAddress : model.Address;
                    string cityToSave = !string.IsNullOrWhiteSpace(model.ProjectCity) ? model.ProjectCity : model.City;
                    string stateToSave = !string.IsNullOrWhiteSpace(model.ProjectState) ? model.ProjectState : model.State;
                    string pincodeToSave = !string.IsNullOrWhiteSpace(model.ProjectPincode) ? model.ProjectPincode : model.Pincode;

                    cmd.Parameters.AddWithValue("@Address", (object)addressToSave ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@City", (object)cityToSave ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@State", (object)stateToSave ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Pincode", (object)pincodeToSave ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@AvaGroundWaterLeavel", (object)model.GroundWater ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AvaGroundWaterSupply", (object)model.GovtWaterSupply ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AvaElectricity", (object)model.Electricity ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@AreaIn", (object)model.AreaIn ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LandArea", (object)model.LandArea ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FSI", (object)model.FSI ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PremiumFSI", (object)model.PremiumFSI ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExpanFSI", (object)model.ExpandableFSI ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@NoofFloors", (object)model.NoofFloors ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@GuidelineValue", (object)model.GuidelineValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BuiltupArea", (object)model.BuiltupArea ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SaleableArea", (object)model.SaleableArea ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeasableArea", (object)model.LeasableArea ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BasementArea", (object)model.BasementArea ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SuperBuiltupArea", (object)model.SuperBuiltupArea ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NoofCarParking", (object)model.NoofCarParking ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ParkingAreaPerCar", (object)model.ParkingAreaPerCar ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@ProjectSpecification", (object)model.ProjectSpecification ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@DrawingName", (object)finalDrawingName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DrawingFilePath", (object)drawingFilePath ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@DocName", (object)finalDocName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DocFilePath", (object)docFilePath ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@ConsultName", DBNull.Value);
                    cmd.Parameters.AddWithValue("@ConsultType", DBNull.Value);
                    cmd.Parameters.AddWithValue("@FeesPer", DBNull.Value);
                    cmd.Parameters.AddWithValue("@FeesAmount", DBNull.Value);

                    cmd.Parameters.AddWithValue("@CostDescription", DBNull.Value);
                    cmd.Parameters.AddWithValue("@CostType", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Amount", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Duration", DBNull.Value);

                    cmd.Parameters.AddWithValue("@StartDate", DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndDate", DBNull.Value);

                    cmd.Parameters.AddWithValue("@WBSRequirement", (object)model.WBSRequirement ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@MaterialConsumption", (object)model.MaterialConsumption ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ItemwiseIssueRequire", (object)model.ItemwiseIssueRequire ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CCwiseAssetIssue", (object)model.CCwiseAssetIssue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@VehicleProduction", (object)model.VehicleProduction ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IssueRateBasedOn", (object)model.IssueRateBasedOn ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IssueRateDate", DBNull.Value);

                    cmd.Parameters.AddWithValue("@CostCentreId", (object)model.BusinessTypeId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProjectId", DBNull.Value);

                    cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                    string userIdsToSave = (model.Users != null && model.Users.Count > 0)
                        ? string.Join(",", model.Users)
                        : logUserId;
                    cmd.Parameters.AddWithValue("@LogUserId", userIdsToSave);

                    cmd.Parameters.AddWithValue("@IPAddress", ipAddress);
                    cmd.Parameters.AddWithValue("@HostName", hostName);
                    cmd.Parameters.AddWithValue("@Status", "1");

                    await con.OpenAsync();

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            int result = dr["Result"] != DBNull.Value ? Convert.ToInt32(dr["Result"]) : 0;
                            string message = dr["Message"] != DBNull.Value ? dr["Message"].ToString() : "";
                            int kickoffId = 0;
                            if (dr.FieldCount > 2 && dr["ProjectKickoffId"] != DBNull.Value)
                            {
                                int.TryParse(dr["ProjectKickoffId"].ToString(), out kickoffId);
                            }

                            return Json(new
                            {
                                result = result,
                                success = result == 1,
                                message = message,
                                projectKickoffId = kickoffId
                            });
                        }
                    }
                }

                return Json(new { result = 0, success = false, message = "No response from database procedure." });
            }
            catch (Exception ex)
            {
                return Json(new { result = 0, success = false, message = ex.Message });
            }
        }

        private string UploadFileToFTP(IFormFile file)
        {
            string ftpHost = "103.91.186.166";
            string ftpUserName = "vgn360co";
            string ftpPassword = "Zc_fq50Yf1xiJm%t";
            string remoteFolderPath = "/httpdocs/VGN360_PROJECT/PROJECT_CREATION/";

            if (file == null || file.Length == 0)
                return null;

            string originalFileName = Path.GetFileName(file.FileName);
            string fileExtension = Path.GetExtension(originalFileName).ToLower();

            string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
            string ftpUrl = $"ftp://{ftpHost}{remoteFolderPath}{uniqueFileName}";

            try
            {
                CreateFTPDirectoryIfNotExists(ftpHost, ftpUserName, ftpPassword, remoteFolderPath);

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = false;

                using (Stream fileStream = file.OpenReadStream())
                {
                    request.ContentLength = file.Length;
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        fileStream.CopyTo(requestStream);
                    }
                }

                return $"/httpdocs/VGN360_PROJECT/PROJECT_CREATION/{uniqueFileName}";
            }
            catch (WebException ex)
            {
                throw new Exception($"File upload failed: {ex.Message}");
            }
        }

        private void CreateFTPDirectoryIfNotExists(string ftpHost, string userName, string password, string directoryPath)
        {
            try
            {
                string[] subDirs = directoryPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                string currentPath = $"ftp://{ftpHost}";

                foreach (string subDir in subDirs)
                {
                    if (!string.IsNullOrEmpty(subDir))
                    {
                        currentPath += "/" + subDir;

                        try
                        {
                            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(currentPath);
                            request.Method = WebRequestMethods.Ftp.MakeDirectory;
                            request.Credentials = new NetworkCredential(userName, password);
                            request.UsePassive = true;
                            request.UseBinary = true;
                            request.KeepAlive = false;

                            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                            {
                                // Directory created successfully
                            }
                        }
                        catch (WebException ex)
                        {
                            FtpWebResponse response = (FtpWebResponse)ex.Response;
                            if (response != null && response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                            {
                                // Log other errors silently
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently log directory creation errors
            }
        }

        #region Download File Endpoint

        [HttpGet]
        public async Task<IActionResult> DownloadFile(string filePath, string fileName = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return BadRequest("File path is required.");
            }

            string ftpHost = "103.91.186.166";
            string ftpUserName = "vgn360co";
            string ftpPassword = "Zc_fq50Yf1xiJm%t";

            string cleanPath = filePath.Trim();
            if (!cleanPath.StartsWith("/"))
            {
                cleanPath = "/" + cleanPath;
            }

            string ftpUrl = $"ftp://{ftpHost}{cleanPath}";

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
                request.UseBinary = true;
                request.UsePassive = true;

                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                using (Stream responseStream = response.GetResponseStream())
                using (MemoryStream ms = new MemoryStream())
                {
                    await responseStream.CopyToAsync(ms);

                    string ext = Path.GetExtension(cleanPath).ToLower();
                    string downloadName = !string.IsNullOrWhiteSpace(fileName)
                        ? fileName.Trim()
                        : Path.GetFileName(cleanPath);

                    if (!downloadName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                    {
                        downloadName += ext;
                    }

                    string contentType = GetContentType(ext);
                    return File(ms.ToArray(), contentType, downloadName);
                }
            }
            catch (WebException ex)
            {
                return NotFound($"File could not be retrieved from storage: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while downloading the file: {ex.Message}");
            }
        }

        private string GetContentType(string extension)
        {
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".dwg" => "application/acad",
                ".dxf" => "application/dxf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".zip" => "application/zip",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        #endregion

        #endregion
    }

    #region Request Model

    public class ProjectSaveRequest
    {
        public string Action { get; set; } = "INSERT";
        public int? ProjectKickoffId { get; set; }

        public string ProjectName { get; set; }
        public string LandName { get; set; }

        public string ProjectAddress { get; set; }
        public string Address { get; set; }

        public string ProjectCity { get; set; }
        public string City { get; set; }

        public string ProjectState { get; set; }
        public string State { get; set; }

        public string ProjectPincode { get; set; }
        public string Pincode { get; set; }

        public string CompanyId { get; set; }
        public string BusinessTypeId { get; set; }
        public string PropertyType { get; set; }
        public string ProjectTypeId { get; set; }

        public string SoilTypeId { get; set; }
        public string GroundWater { get; set; }
        public string GovtWaterSupply { get; set; }
        public string Electricity { get; set; }

        public string AreaIn { get; set; }
        public string LandArea { get; set; }
        public string FSI { get; set; }
        public string PremiumFSI { get; set; }
        public string ExpandableFSI { get; set; }

        public double? NoofFloors { get; set; }
        public double? GuidelineValue { get; set; }
        public double? BuiltupArea { get; set; }
        public double? SaleableArea { get; set; }
        public double? LeasableArea { get; set; }
        public double? BasementArea { get; set; }
        public double? SuperBuiltupArea { get; set; }
        public double? NoofCarParking { get; set; }
        public double? ParkingAreaPerCar { get; set; }

        public string ProjectSpecification { get; set; }

        public string DrawingName { get; set; }
        public string DrawingDescription { get; set; }
        public IFormFile DrawingFile { get; set; }

        public string DocName { get; set; }
        public string DocDescription { get; set; }
        public IFormFile DocFile { get; set; }

        public string WBSRequirement { get; set; }

        public string MaterialConsumption { get; set; }
        public string ItemwiseIssueRequire { get; set; }
        public string CCwiseAssetIssue { get; set; }
        public string VehicleProduction { get; set; }
        public string IssueRateBasedOn { get; set; }

        public List<string> Users { get; set; }
    }

    #endregion
}
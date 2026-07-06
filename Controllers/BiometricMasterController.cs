using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    [TrackPageVisit]
    public class BiometricMasterController : Controller
    {
        private readonly string _connLI;   // TM_VGN_LOGIN_DB  (biometric store)
        private readonly string _connDB;   // TM_VGN_DB_E      (employee master)
        private readonly string _connMIS;  // TM_VGN_MIS       (Load_Emp_With_Department SP)

        public BiometricMasterController(IConfiguration config)
        {
            _connLI  = config.GetActiveConnectionString("ConnLI");
            _connDB  = config.GetActiveConnectionString("ConnDB");
            _connMIS = config.GetActiveConnectionString("ConnMIS");
        }

        // ──────────────────────────────────────────────────────
        // GET: /BiometricMaster/Index
        // ──────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewBag.Title    = "Biometric Master";
            ViewBag.UserId   = user.UserId;
            ViewBag.UserName = user.UserName;
            return View();
        }



        // ──────────────────────────────────────────────────────
        // POST: /BiometricMaster/SaveFingerprint
        // Saves or updates a fingerprint template via SP.
        // ──────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult SaveFingerprint([FromBody] BiometricSaveRequest req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired" });

                if (req == null || string.IsNullOrWhiteSpace(req.UserId))
                    return Json(new { status = false, msg = "Invalid request data" });

                if (string.IsNullOrWhiteSpace(req.FingerprintData))
                    return Json(new { status = false, msg = "No fingerprint data received" });

                var outParam = new SqlParameter("@OutResult", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                using (var con = new SqlConnection(_connLI))
                using (var cmd = new SqlCommand("usp_BiometricMaster", con))
                {
                    cmd.CommandType    = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Flag",            "SAVE");
                    cmd.Parameters.AddWithValue("@UserId",          req.UserId);
                    cmd.Parameters.AddWithValue("@UserName",        (object)req.UserName      ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentId",    (object)req.DepartmentId  ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentName",  (object)req.DepartmentName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FingerIndex",     req.FingerIndex);
                    cmd.Parameters.AddWithValue("@FingerLabel",     (object)req.FingerLabel   ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FingerprintData", req.FingerprintData);
                    cmd.Parameters.AddWithValue("@ScanQuality",     req.ScanQuality > 0 ? (object)req.ScanQuality : DBNull.Value);
                    cmd.Parameters.AddWithValue("@DeviceType",      (object)req.DeviceType    ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DeviceName",      (object)req.DeviceName    ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EnrolledBy",      user.UserId);
                    cmd.Parameters.Add(outParam);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                int result = outParam.Value != DBNull.Value ? Convert.ToInt32(outParam.Value) : 0;
                return result switch
                {
                    1 => Json(new { status = true,  msg = "Fingerprint enrolled successfully!" }),
                    2 => Json(new { status = true,  msg = "Fingerprint template updated successfully!" }),
                    _ => Json(new { status = false, msg = "Save failed. Please try again." })
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BiometricMaster SaveFingerprint] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────
        // POST: /BiometricMaster/LoadAllBiometrics
        // Admin grid – all enrolled fingerprints.
        // ──────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult LoadAllBiometrics()
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired" });

                var outParam = new SqlParameter("@OutResult", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                var dt = new DataTable();
                using (var con = new SqlConnection(_connLI))
                using (var cmd = new SqlCommand("usp_BiometricMaster", con))
                {
                    cmd.CommandType    = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Flag", "LOAD_ALL");
                    cmd.Parameters.Add(outParam);
                    con.Open();
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                if (dt.Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new { status = true, msg = "Success", data = dt });
                    return Content(json, "application/json");
                }
                return Json(new { status = false, msg = "No records found" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BiometricMaster LoadAllBiometrics] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────
        // POST: /BiometricMaster/LoadUserBiometrics
        // Load enrolled fingers for a specific user.
        // ──────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult LoadUserBiometrics([FromBody] BiometricUserRequest req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired" });

                var outParam = new SqlParameter("@OutResult", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                var dt = new DataTable();
                using (var con = new SqlConnection(_connLI))
                using (var cmd = new SqlCommand("usp_BiometricMaster", con))
                {
                    cmd.CommandType    = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Flag",   "LOAD_USER");
                    cmd.Parameters.AddWithValue("@UserId", req?.UserId ?? user.UserId);
                    cmd.Parameters.Add(outParam);
                    con.Open();
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                if (dt.Rows.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(new { status = true, msg = "Success", data = dt });
                    return Content(json, "application/json");
                }
                return Json(new { status = false, msg = "No biometrics enrolled for this user" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BiometricMaster LoadUserBiometrics] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────
        // POST: /BiometricMaster/DeleteBiometric
        // Soft-deletes a single fingerprint record.
        // ──────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult DeleteBiometric([FromBody] BiometricDeleteRequest req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null)
                    return Json(new { status = false, msg = "Session expired" });

                if (req == null || req.BiometricId <= 0)
                    return Json(new { status = false, msg = "Invalid BiometricId" });

                var outParam = new SqlParameter("@OutResult", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                using (var con = new SqlConnection(_connLI))
                using (var cmd = new SqlCommand("usp_BiometricMaster", con))
                {
                    cmd.CommandType    = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@Flag",        "DELETE");
                    cmd.Parameters.AddWithValue("@BiometricId", req.BiometricId);
                    cmd.Parameters.AddWithValue("@EnrolledBy",  user.UserId);
                    cmd.Parameters.Add(outParam);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                return Json(new { status = true, msg = "Biometric record deleted successfully." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BiometricMaster DeleteBiometric] {ex.Message}");
                return Json(new { status = false, msg = ex.Message });
            }
        }
    }

    // ── Request Models ─────────────────────────────────────────
    public class BiometricEmpRequest
    {
        public string Flag { get; set; }   // e.g. "Emp_With_Department"
    }

    public class BiometricSaveRequest
    {
        public string UserId          { get; set; }
        public string UserName        { get; set; }
        public string DepartmentId    { get; set; }
        public string DepartmentName  { get; set; }
        public int    FingerIndex     { get; set; }
        public string FingerLabel     { get; set; }
        public string FingerprintData { get; set; }  // Base-64 template / credential
        public int    ScanQuality     { get; set; }
        public string DeviceType      { get; set; }  // "eSSL" | "WebAuthn-Platform" | "Other"
        public string DeviceName      { get; set; }
    }

    public class BiometricUserRequest
    {
        public string UserId { get; set; }
    }

    public class BiometricDeleteRequest
    {
        public int BiometricId { get; set; }
    }
}

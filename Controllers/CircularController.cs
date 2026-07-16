using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;
using VGN_CRM_CORE.Models;
using System.Threading.Tasks;

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    public class CircularController : Controller
    {
        private readonly string _connLI;
        private readonly IHubContext<VGN_CRM_CORE.Hubs.ChatHub> _hubContext;

        public CircularController(IConfiguration config, IHubContext<VGN_CRM_CORE.Hubs.ChatHub> hubContext)
        {
            _connLI = config.GetActiveConnectionString("ConnLI");
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpGet]
        public IActionResult GetCirculars(bool all = false)
        {
            try
            {
                var dt = new DataTable();
                using (var con = new SqlConnection(_connLI))
                using (var cmd = new SqlCommand("usp_GetCirculars", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                var list = new List<CircularModel>();
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new CircularModel
                    {
                        CircularId = Convert.ToInt32(row["CircularId"]),
                        Title = row["Title"].ToString(),
                        Description = row["Description"].ToString(),
                        PostedBy = row["MergeNameIntial"].ToString(),
                        PostedDate = Convert.ToDateTime(row["PostedDate"]).ToString("dd MMM yyyy, hh:mm tt"),
                        ExpiryDate = row["ExpiryDate"] != DBNull.Value ? Convert.ToDateTime(row["ExpiryDate"]).ToString("yyyy-MM-dd") : null,
                        IsActive = row["IsActive"] != DBNull.Value ? Convert.ToBoolean(row["IsActive"]) : true,
                        DeptId = row["Deptname"] != DBNull.Value ? row["Deptname"].ToString() : null,
                        PostedByEmpId = row["PostedByEmpId"] != DBNull.Value ? row["PostedByEmpId"].ToString() : null
                    });
                }

                if (!all)
                {
                    list = list.Where(c => c.IsActive && 
                        (string.IsNullOrEmpty(c.ExpiryDate) || DateTime.Parse(c.ExpiryDate) >= DateTime.Today)).ToList();
                }

                return Json(new { status = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCircular([FromBody] CircularModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null) return Json(new { status = false, msg = "Session expired" });

                if (string.IsNullOrWhiteSpace(req.Title) || string.IsNullOrWhiteSpace(req.Description))
                {
                    return Json(new { status = false, msg = "Title and Description are required." });
                }

                using (var con = new SqlConnection(_connLI))
                using (var cmd = new SqlCommand("usp_AddCircular", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Title", req.Title);
                    cmd.Parameters.AddWithValue("@Description", req.Description);
                    cmd.Parameters.AddWithValue("@PostedBy", user.UserName ?? user.UserId);
                    if (!string.IsNullOrEmpty(req.ExpiryDate))
                    {
                        cmd.Parameters.AddWithValue("@ExpiryDate", Convert.ToDateTime(req.ExpiryDate));
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@ExpiryDate", DBNull.Value);
                    }
                    
                    cmd.Parameters.AddWithValue("@DeptId", string.IsNullOrEmpty(user.DepartmentId) ? (user.Role ?? (object)DBNull.Value) : user.DepartmentId);
                    cmd.Parameters.AddWithValue("@PostedByEmpId", user.UserId ?? (object)DBNull.Value);
                    
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                await _hubContext.Clients.All.SendAsync("NewCircular");

                return Json(new { status = true, msg = "Circular posted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateCircularStatus([FromBody] CircularModel req)
        {
            try
            {
                var user = SessionHelper.GetUserSession(HttpContext.Session);
                if (user == null) return Json(new { status = false, msg = "Session expired" });

                using (var con = new SqlConnection(_connLI))
                using (var cmd = new SqlCommand("usp_UpdateCircularStatus", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CircularId", req.CircularId);
                    cmd.Parameters.AddWithValue("@IsActive", req.IsActive);
                    cmd.Parameters.AddWithValue("@LastUpdatedBy", user.UserId ?? (object)DBNull.Value);
                    
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                return Json(new { status = true, msg = "Circular status updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, msg = ex.Message });
            }
        }
    }
}

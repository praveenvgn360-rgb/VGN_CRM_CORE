using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;
using VGN_CRM_CORE.Models;

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    public class CircularController : Controller
    {
        private readonly string _connLI;

        public CircularController(IConfiguration config)
        {
            _connLI = config.GetActiveConnectionString("ConnLI");
        }

        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            if (user == null) return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpGet]
        public IActionResult GetCirculars()
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
                        PostedBy = row["PostedBy"].ToString(),
                        PostedDate = Convert.ToDateTime(row["PostedDate"]).ToString("dd MMM yyyy, hh:mm tt"),
                        ExpiryDate = row["ExpiryDate"] != DBNull.Value ? Convert.ToDateTime(row["ExpiryDate"]).ToString("yyyy-MM-dd") : null
                    });
                }

                return Json(new { status = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult AddCircular([FromBody] CircularModel req)
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
                    
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                return Json(new { status = true, msg = "Circular posted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, msg = ex.Message });
            }
        }
    }
}

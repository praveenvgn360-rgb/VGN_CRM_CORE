using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Filters;
using VGN_CRM_CORE.Models;

namespace VGN_CRM_CORE.Controllers
{
    [AuthorizeSession]
    [TrackPageVisit]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            ViewBag.UserName  = user?.UserName;
            ViewBag.LoginTime = user?.LoginTime.ToString("dd-MMM-yyyy hh:mm tt");
            ViewBag.IPAddress = user?.IPAddress;
            ViewBag.HostName  = user?.HostName;
            ViewBag.UserRole  = user?.Role;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // ══════════════════════════════════════════════════════════
    // SETTINGS CONTROLLER
    // ══════════════════════════════════════════════════════════
    [AuthorizeSession]
    public class SettingsController : Controller
    {
        // GET  /Settings/GetPreferences  (called via AJAX on page load)
        [HttpGet]
        public IActionResult GetPreferences()
        {
            var user = SessionHelper.GetUserSession(HttpContext.Session);
            var pref = AuditLogger.GetPreferences(user.UserId);
            return Json(pref);
        }

        // POST /Settings/Save  (called via AJAX when user changes a setting)
        [HttpPost]
        public IActionResult Save([FromBody] UserPreferences pref)
        {
            var user    = SessionHelper.GetUserSession(HttpContext.Session);
            pref.UserId = user.UserId;
            AuditLogger.SavePreferences(pref);
            return Json(new { success = true });
        }
    }
}

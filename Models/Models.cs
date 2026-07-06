using System;
using System.ComponentModel.DataAnnotations;

namespace VGN_CRM_CORE.Models
{
    // ──────────────────────────────────────────────────────────
    // Session / Auth Models
    // ──────────────────────────────────────────────────────────

    public class LoginViewModel
    {
        [Required(ErrorMessage = "User ID is required")]
        [Display(Name = "User ID")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
        
        public string LoginType { get; set; }
    }

    public class UserSession
    {
        public string UserId    { get; set; }
        public string UserName  { get; set; }
        public string Email     { get; set; }
        public string MobileNo  { get; set; }
        public string Role      { get; set; }
        public string Designation { get; set; }
        public string SessionId { get; set; }
        public DateTime LoginTime { get; set; }
        public string IPAddress { get; set; }
        public string HostName  { get; set; }
        public string DepartmentId { get; set; }
        public string Department { get; set; }
        public string RptEmpName { get; set; }
        public string ReprotingMailId { get; set; }
        public string ReportingMobileNum { get; set; }
        public string LoginType { get; set; }  // "CORPORATE" or "CHANNELPARTNER"
    }

    // ──────────────────────────────────────────────────────────
    // Audit Models
    // ──────────────────────────────────────────────────────────

    public class UserLoginAudit
    {
        public int AuditId          { get; set; }
        public string UserId        { get; set; }
        public string UserName      { get; set; }
        public string IPAddress     { get; set; }
        public string HostName      { get; set; }
        public string BrowserInfo   { get; set; }
        public DateTime LoginTime   { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string SessionId     { get; set; }
        public bool IsActive        { get; set; }
    }

    public class PageVisitLog
    {
        public int LogId               { get; set; }
        public string UserId           { get; set; }
        public string SessionId        { get; set; }
        public string ControllerName   { get; set; }
        public string ActionName       { get; set; }
        public string PageUrl          { get; set; }
        public string PageTitle        { get; set; }
        public DateTime VisitTime      { get; set; }
        public string IPAddress        { get; set; }
        public int? TimeTakenMs        { get; set; }
    }

    // ──────────────────────────────────────────────────────────
    // User Preferences (Theme / Layout Settings)
    // ──────────────────────────────────────────────────────────

    public class UserPreferences
    {
        public string UserId      { get; set; }
        public string ThemeMode   { get; set; } = "light";    // light | dark | auto
        public string ThemeColor  { get; set; } = "#4680FF";  // hex
        public string SidebarType { get; set; } = "full";     // full | mini | hidden
        public string NavLayout   { get; set; } = "vertical"; // vertical | horizontal
        public string Contrast    { get; set; } = "default";  // default | high
        public string FontSize    { get; set; } = "14px";     // 12px | 14px | 16px | 18px
        public string FontFamily  { get; set; } = "system";   // system | Inter | Roboto | …
    }

    // ──────────────────────────────────────────────────────────
    // DB Entity
    // ──────────────────────────────────────────────────────────

    public class AppUser
    {
        public string UserId   { get; set; }
        public string UserName { get; set; }
        public string Email    { get; set; }
        public string MobileNo { get; set; }
        public string Password { get; set; }
        public string Role     { get; set; }
        public string Designation { get; set; }
        public string DepartmentId { get; set; }
        public string Department { get; set; }
        public string RptEmpName { get; set; }
        public string ReprotingMailId { get; set; }
        public string ReportingMobileNum { get; set; }
        public bool IsActive   { get; set; }
    }
}

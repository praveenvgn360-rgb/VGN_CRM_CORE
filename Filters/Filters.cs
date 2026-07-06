using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using VGN_CRM_CORE.CommonFunctions;
using VGN_CRM_CORE.Models;

namespace VGN_CRM_CORE.Filters
{
    // ══════════════════════════════════════════════════════════
    // [AuthorizeSession]
    // Apply to controllers / actions that require login.
    // Redirects unauthenticated users to /Account/Login.
    // ══════════════════════════════════════════════════════════
    public class AuthorizeSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            if (!SessionHelper.IsLoggedIn(session))
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectResult(
                    $"/Account/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
                return;
            }

            var user = SessionHelper.GetUserSession(session);
            if (user != null)
            {
                // Check session validity against DB:
                //   SessionCheckResult.Valid       → allow through
                //   SessionCheckResult.Invalidated → session was force-logged out / timed out → show error
                //   SessionCheckResult.NotFound    → no DB record at all (DB restart, new install,
                //                                    or user "md" with no prior login record)
                //                                    → silently clear cookie, redirect to plain login
                var result = AuditLogger.CheckSessionStatus(user.SessionId);

                if (result == SessionCheckResult.Invalidated)
                {
                    // Genuine forced logout or timeout from another device.
                    // Use TempData (not query string) so the message cannot be
                    // triggered by a browser refresh or a bookmarked URL.
                    SessionHelper.ClearSession(session);
                    var tempData = context.HttpContext.RequestServices
                        .GetService(typeof(ITempDataDictionaryFactory)) as ITempDataDictionaryFactory;
                    var td = tempData?.GetTempData(context.HttpContext);
                    if (td != null) td["SessionExpired"] = "true";
                    context.Result = new RedirectResult("/Account/Login");
                    return;
                }

                if (result == SessionCheckResult.NotFound)
                {
                    // No DB record — stale cookie or missing entry; clear quietly, plain login
                    SessionHelper.ClearSession(session);
                    var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                    context.Result = new RedirectResult(
                        $"/Account/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }

    // ══════════════════════════════════════════════════════════
    // [TrackPageVisit]
    // Automatically logs every GET request to tbl_PageVisitLog.
    // Apply globally in Startup or per-controller.
    // ══════════════════════════════════════════════════════════
    public class TrackPageVisitAttribute : ActionFilterAttribute
    {
        private Stopwatch _sw;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _sw = Stopwatch.StartNew();
            base.OnActionExecuting(context);
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            _sw?.Stop();

            // Only track GET requests
            if (!string.Equals(context.HttpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
                return;

            var session = context.HttpContext.Session;
            var user    = SessionHelper.GetUserSession(session);
            if (user == null) return;

            var routeData  = context.RouteData;
            var controller = routeData.Values["controller"]?.ToString();
            var action     = routeData.Values["action"]?.ToString();

            // Only track the main page view (typically "Index"), ignore data-fetching functions
            if (!string.Equals(action, "Index", StringComparison.OrdinalIgnoreCase))
                return;

            AuditLogger.LogPageVisit(new PageVisitLog
            {
                UserId         = user.UserId,
                SessionId      = user.SessionId,
                ControllerName = controller,
                ActionName     = action,
                PageUrl        = context.HttpContext.Request.Path.Value,
                PageTitle      = $"{controller} / {action}",
                VisitTime      = DateTime.Now,
                IPAddress      = user.IPAddress,
                TimeTakenMs    = (int)(_sw?.ElapsedMilliseconds ?? 0)
            });

            base.OnResultExecuted(context);
        }
    }

    // ══════════════════════════════════════════════════════════
    // [NoCache]
    // Prevents browser caching on any controller/action.
    // ══════════════════════════════════════════════════════════
    public class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext context)
        {
            var response = context.HttpContext.Response;
            response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            response.Headers["Pragma"]        = "no-cache";
            response.Headers["Expires"]       = "0";
            base.OnResultExecuted(context);
        }
    }
}

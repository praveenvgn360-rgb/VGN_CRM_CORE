using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VGN_CRM_CORE.CommonFunctions;

namespace VGN_CRM_CORE
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            // Give AuditLogger access to connection strings without DI
            AuditLogger.Initialize(configuration);
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
            });

            // Required for session to work
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            // Allows controllers/filters to access HttpContext
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSignalR();

            services.AddControllersWithViews(options =>
            {
                // Force browser to always fetch fresh page responses — no more Ctrl+Shift+R needed.
                options.Filters.Add(new Microsoft.AspNetCore.Mvc.ResponseCacheAttribute
                {
                    NoStore = true,
                    Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.None
                });
            });
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)

        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCookiePolicy();
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<VGN_CRM_CORE.Hubs.ChatHub>("/chatHub");
                // Default route → login page
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");
            });
        }
    }
}

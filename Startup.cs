using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using VGN_CRM_CORE.CommonFunctions;

namespace VGN_CRM_CORE
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // Seed static helpers that need config before DI is ready
            AuditLogger.Initialize(configuration);
            SessionHelper.Initialize(configuration);
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // ── Cookie policy ─────────────────────────────────
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded  = _ => false;
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
            });

            // ── Keep ASP.NET Session for TempData (SessionExpired flag) ─
            // We NO LONGER store user identity in ISession.
            // ISession is only used by TempDataProvider for one-shot flags.
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout        = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly    = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite    = SameSiteMode.Lax;
            });

            // ── JWT authentication (used for API-style validation if needed) ─
            var jwtSecret = Configuration["Jwt:Secret"]
                ?? "VGN360_SuperSecret_JWT_Key_2024!@#$%^&*_MustBe256BitsLong_RandomString";

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
                        ValidateIssuer           = false,
                        ValidateAudience         = false,
                        ClockSkew                = TimeSpan.Zero
                    };

                    // Allow JWT from HttpOnly cookie (MVC controllers use cookie-based JWT)
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = ctx =>
                        {
                            ctx.Token = ctx.Request.Cookies["vgn_jwt"];
                            return System.Threading.Tasks.Task.CompletedTask;
                        }
                    };
                });

            // ── HTTP context accessor ─────────────────────────
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // ── SignalR ───────────────────────────────────────
            services.AddSignalR();

            // ── MVC ───────────────────────────────────────────
            services.AddControllersWithViews(options =>
            {
                // Force no-cache on all MVC responses
                options.Filters.Add(new ResponseCacheAttribute
                {
                    NoStore  = true,
                    Location = ResponseCacheLocation.None
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Inject IHttpContextAccessor into SessionHelper so it can read/write cookies
            SessionHelper.HttpContextAccessor =
                app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseCookiePolicy();
            app.UseSession();           // Required for TempData only
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<VGN_CRM_CORE.Hubs.ChatHub>("/chatHub");

                endpoints.MapControllerRoute(
                    name:    "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");
            });
        }
    }
}

using System.IdentityModel.Tokens.Jwt;
using MagicMedia.AspNetCore;
using MagicMedia.Identity.Data.Mongo;
using MagicMedia.Identity.Data.Mongo.Seeding;
using MagicMedia.Identity.Messaging;
using MagicMedia.Identity.Services;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MagicMedia.Identity
{
    public class Startup
    {
        public Startup(
            IConfiguration configuration,
            IWebHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            HostEnvironment = hostEnvironment;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment HostEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            IIdentityServerBuilder builder = services.AddIdentityServer(Configuration, HostEnvironment);

            builder.AddDeveloperSigningCredential();

            services.AddDataAccess(Configuration);
            services.AddIdentityCore(Configuration);

            services.ConfigureSameSiteCookies();

            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();

            services.AddMessaging(Configuration);
            services.AddMassTransitHostedService();

            services.AddSingleton<IDemoUserService>(s => new DemoUserService(
                HostEnvironment.IsDemo(),
                Configuration));
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            DataSeeder dataSeeder)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            if (env.IsDevelopment() || env.IsDemo())
            {
                dataSeeder.SeedIntialDataAsync(default).GetAwaiter().GetResult();
            }

            app.UseSerilogRequestLogging();

            app.UseDefaultForwardedHeaders();
            app.UseCookiePolicy();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

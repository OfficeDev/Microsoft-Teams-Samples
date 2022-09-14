// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Web
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Identity.Web;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.Teams.Samples.MeetingSigning.Domain;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Data;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.GraphService;
    using Microsoft.Teams.Samples.MeetingSigning.Web.Authorization;
    using Microsoft.Teams.Samples.MeetingSigning.Web.Middleware;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services
                .AddAuthentication()
                .AddMicrosoftIdentityWebApi(Configuration, "MsaAuth", AuthenticationScheme.Msa, true)
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddMicrosoftGraph(Configuration.GetSection("MsaGraph"))
                .AddInMemoryTokenCaches();

            services
                .AddAuthentication(AuthenticationScheme.Aad)
                .AddMicrosoftIdentityWebApi(Configuration, "AzureAd", AuthenticationScheme.Aad)
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddMicrosoftGraph(Configuration.GetSection("Graph"))
                .AddInMemoryTokenCaches();

            services.AddHttpContextAccessor();
            services.AddCustomAuthorization();
            services.AddControllersWithViews();

            services.AddDomainServices();
            services.AddGraphServices();

            if (Configuration.GetValue<bool>("Data:UseInMemoryDataStore"))
            {
                services.AddInMemoryDataStorage();
            }
            else
            {
                services.AddEntityFrameworkDataStorage();
            }

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts(); 
            }
            else
            {
                IdentityModelEventSource.ShowPII = true;
            }

            app.UseMiddleware<JsonExceptionHandler>();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.Options.StartupTimeout = TimeSpan.FromSeconds(120);
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}

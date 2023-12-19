// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabRequestApproval
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authentication.AzureAD.UI;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;
    using TabActivityFeed.Providers;
    using TabRequestApproval.Helpers;
    using TabRequestApproval.Model;

    /// <summary>
    /// Startup class.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Represents the appsettings.json file details.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration settings from the appsettings.json file.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Adds services at run time.
        /// </summary>
        /// <param name="services">Represents the collection of services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromMinutes(60);
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMemoryCache();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMvc().AddSessionStateTempDataProvider();
            services.AddSingleton<ConcurrentDictionary<string, List<RequestInfo>>>();
            services.AddSingleton<IAuthProvider, AuthProvider>();
            services.AddSingleton<ISubscriptionProvider, SubscriptionProvider>();
            services.AddSingleton<IChangeNotificationProvider, ChangeNotificationProvider>();
            services.AddSingleton<IContainerProvider, ContainerProvider>();
            services.AddSingleton<IContainerPermissionProvider, ContainerPermissionProvider>();
            services.AddSingleton<IDriveProvider, DriveProvider>();
            services.AddSingleton<IDriveItemProvider, DriveItemProvider>();
            services.AddSingleton<IRequestProvider, RequestProvider>();

            services.AddControllersWithViews();
            services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));
            services.AddHttpContextAccessor();
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    var azureAdOptions = new AzureADOptions();
#pragma warning restore CS0618 // Type or member is obsolete
                    this.Configuration.Bind("AzureAd", azureAdOptions);
                    options.Authority = $"{azureAdOptions.Instance}{azureAdOptions.TenantId}/v2.0";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudiences = AuthHelper.GetValidAudiences(this.Configuration),
                        ValidIssuers = AuthHelper.GetValidIssuers(this.Configuration),
                        AudienceValidator = AuthHelper.AudienceValidator,
                    };
                });
        }

        /// <summary>
        /// Configures the HTTP request pipeline.
        /// It gets called at runtime.
        /// </summary>
        /// <param name="app">Represents the application builder.</param>
        /// <param name="env">Represents the webhost environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseSession()
                .UseCookiePolicy()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()

                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                });
        }
    }
}
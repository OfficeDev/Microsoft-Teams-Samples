// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabRequestApproval
{
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using TabActivityFeed.Providers;
    using TabRequestApproval.Helpers;
    using TabRequestApproval.Model;

    /// <summary>
    /// Main program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main function.
        /// </summary>
        /// <param name="args">Represents the arguments.</param>
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            // Configure logging
            builder.Logging.AddDebug().AddConsole();

            // Add services
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromMinutes(60);
            });

            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<ConcurrentDictionary<string, List<RequestInfo>>>();
            builder.Services.AddSingleton<IAuthProvider, AuthProvider>();
            builder.Services.AddSingleton<ISubscriptionProvider, SubscriptionProvider>();
            builder.Services.AddSingleton<IChangeNotificationProvider, ChangeNotificationProvider>();
            builder.Services.AddSingleton<IContainerProvider, ContainerProvider>();
            builder.Services.AddSingleton<IContainerPermissionProvider, ContainerPermissionProvider>();
            builder.Services.AddSingleton<IDriveProvider, DriveProvider>();
            builder.Services.AddSingleton<IDriveItemProvider, DriveItemProvider>();
            builder.Services.AddSingleton<IRequestProvider, RequestProvider>();

            builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider();
            builder.Services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));

            // Configure authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    var azureAdConfig = configuration.GetSection("AzureAd");
                    var instance = azureAdConfig["Instance"] ?? "https://login.microsoftonline.com/";
                    var tenantId = azureAdConfig["TenantId"];

                    options.Authority = $"{instance}{tenantId}/v2.0";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudiences = AuthHelper.GetValidAudiences(configuration),
                        ValidIssuers = AuthHelper.GetValidIssuers(configuration),
                        AudienceValidator = AuthHelper.AudienceValidator,
                    };
                });

            var app = builder.Build();

            // Configure middleware
            if (app.Environment.IsDevelopment())
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
                .UseAuthorization();

            app.MapControllers();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            await app.RunAsync();
        }
    }
}
using AppInstallation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AppInstallation.Controllers
{
    public class HomeController : BaseController
    {
        public static string TenantId;
        public static string TeamId;
        public static string UserId;

        public HomeController(IConfiguration configuration) : base(configuration)
        {
        }
        public IActionResult Index()
        {
            return View();
        }

        [Route("ConfigureTab")]
        public IActionResult ConfigureTab()
        {
            return View();
        }

        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index (string tenantId, string teamId)
        {
            TenantId = tenantId;
            TeamId = teamId;

            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);
            var model = await GetApps(teamId, graphClient);

            return View(model);
        }

        [HttpGet]
        [Route("GetAppInfo")]
        public async Task<List<string>> GetAppInfo(string appId)
        {
            string token = await GetToken(TenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);
            var teamsAppInstallation = await graphClient.Teams[TeamId]
                .InstalledApps[appId]
                .Request()
                .Expand("teamsAppDefinition")
                .GetAsync();
            return new List<string>
            {
                teamsAppInstallation.TeamsAppDefinition.DisplayName,
                teamsAppInstallation.TeamsAppDefinition.Description
            };
        }

        private async Task<AppViewModel> GetApps(string teamId, GraphServiceClient graphClient)
        {
            AppViewModel viewModel = new AppViewModel();
            List<AppModel> list = new List<AppModel>();
            
            var installedApps = await graphClient.Teams[teamId].InstalledApps
                .Request()
                .Expand("teamsAppDefinition, teamsApp")
                .GetAsync();
            foreach (var res in installedApps.ToList())
            {
                var channelModel = new AppModel();
                channelModel.AppId = res.Id;
                channelModel.AppName = res.TeamsAppDefinition.DisplayName;
                channelModel.AppDistributionMethod = res.TeamsApp.DistributionMethod.ToString();
                list.Add(channelModel);
            }

            viewModel.appList = list;
            viewModel.IsUserList = false;
            return viewModel;
        }
        [HttpDelete]
        [Route("DeleteApp")]
        public async Task<string> DeleteApp (string appId)
        {
            string token = await GetToken(TenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);
            await graphClient.Teams[TeamId].InstalledApps[appId]
                .Request()
                .DeleteAsync();
            return "Deleted Successfully";
        }

        [HttpPost]
        [Route("UpgradeApp")]
        public async Task<string> UpgradeApp(string appId)
        {
            string token = await GetToken(TenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);
            await graphClient.Teams[TeamId].InstalledApps[appId]
                .Upgrade()
                .Request()
                .PostAsync();
            return "Updated Successfully";
        }

        [HttpPost]
        [Route("AddApp")]
        public async Task<string> AddApp()
        {
            string token = await GetToken(TenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);

            string PollyAppId = "1542629c-01b3-4a6d-8f76-1938b779e48d";
            var teamsAppInstallation = new TeamsAppInstallation
            {
                AdditionalData = new Dictionary<string, object>()
                {
                    {"teamsApp@odata.bind", "https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/" + PollyAppId}
                }
            };
            // Adding Polly App
            await graphClient.Teams[TeamId].InstalledApps
                .Request()
                .AddAsync(teamsAppInstallation);
            return "Added Successfully";
        }

        [HttpGet]
        [Route("UserIndex")]
        public async Task<IActionResult> UserIndex(string userID)
        {
            UserId = userID;
            string token = await GetToken(TenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);
            var installedApps = await graphClient.Users[UserId].Teamwork.InstalledApps
                            .Request()
                            .Expand("teamsAppDefinition, teamsApp")
                            .GetAsync();
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            foreach (var res in installedApps.ToList())
            {
                Dictionary<string, string> model = new Dictionary<string, string> {
                    { "Name", res.TeamsAppDefinition.DisplayName},
                    {"Id", res.Id },
                    {"DistributionMethod", res.TeamsApp.DistributionMethod.ToString() }
                };
                list.Add(model);
            }

            return Json(list);
        }
        [HttpGet]
        [Route("GetUserAppInfo")]
        public async Task<List<string>> GetUserAppInfo(string appId)
       {
            string token = await GetToken(TenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);
            var teamsAppInstallation = await graphClient.Users[UserId].Teamwork
                .InstalledApps[appId]
                .Request()
                .Expand("teamsAppDefinition")
                .GetAsync();
            return new List<string>
            {
                teamsAppInstallation.TeamsAppDefinition.DisplayName,
                teamsAppInstallation.TeamsAppDefinition.Description
            };
        }
        [HttpDelete]
        [Route("DeleteUserApp")]
        public async Task<string> DeleteUserApp(string appId)
        {
            string token = await GetToken(TenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);
            await graphClient.Users[UserId].Teamwork.InstalledApps[appId]
                .Request()
                .DeleteAsync();
            return "Deleted Successfully";
        }

        [HttpPost]
        [Route("UpgradeUserApp")]
        public async Task<string> UpgradeUserApp(string appId)
        {
            string token = await GetToken(TenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);
            await graphClient.Users[UserId].Teamwork.InstalledApps[appId]
                .Upgrade()
                .Request()
                .PostAsync();
            return "Updated Successfully";
        }
    }
}

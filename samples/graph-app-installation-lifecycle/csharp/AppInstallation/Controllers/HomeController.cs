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
            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);
            var model = await GetApps(tenantId, teamId, graphClient);

            return View(model);
        }

        [HttpGet]
        [Route("GetAppInfo")]
        public async Task<List<string>> GetAppInfo(string tenantId, string teamId, string appId)
        {
            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);
            var teamsAppInstallation = await graphClient.Teams[teamId]
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

        private async Task<AppViewModel> GetApps(string tenantId, string teamId, GraphServiceClient graphClient)
        {
            AppViewModel viewModel = new AppViewModel();
            List<AppModel> list = new List<AppModel>();
            var installedApps = await graphClient.Teams[teamId].InstalledApps
                .Request()
                .Expand("teamsAppDefinition")
                .GetAsync();
            foreach (var res in installedApps.ToList().Take(10))
            {
                var channelModel = new AppModel();
                channelModel.AppId = res.Id;
                channelModel.AppName = res.TeamsAppDefinition.DisplayName;
                list.Add(channelModel);
            }

            viewModel.appList = list;
            viewModel.IsUserList = false;
            return viewModel;
        }
        [HttpDelete]
        [Route("DeleteApp")]
        public async Task<string> DeleteApp(string tenantId, string teamId, string appId)
        {
            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);
            await graphClient.Teams[teamId].InstalledApps[appId]
                .Request()
                .DeleteAsync();
            return "Deleted Successfully";
        }

        [HttpPost]
        [Route("UpgradeApp")]
        public async Task<string> UpgradeApp(string tenantId, string teamId, string appId)
        {
            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);
            await graphClient.Teams[teamId].InstalledApps[appId]
                .Upgrade()
                .Request()
                .PostAsync();
            return "Updated Successfully";
        }

        [HttpPost]
        [Route("AddApp")]
        public async Task<string> AddApp(string tenantId, string teamId)
        {
            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);

            var appsInstalled = await graphClient.Teams[teamId].InstalledApps.Request().Expand("teamsAppDefinition").GetAsync();
            var teamsAppId = appsInstalled.Where(o => o.TeamsAppDefinition.DisplayName == "App Installation").Select(o => o.TeamsAppDefinition.TeamsAppId).FirstOrDefault();
            var teamsAppInstallation = new TeamsAppInstallation
            {
                AdditionalData = new Dictionary<string, object>()
                {
                    {"teamsApp@odata.bind", "https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/" + teamsAppId}
                }
            };

            await graphClient.Teams[teamId].InstalledApps
                .Request()
                .AddAsync(teamsAppInstallation);
            return "Added Successfully";
        }

        [HttpGet]
        [Route("UserIndex")]
        public async Task<IActionResult> UserIndex(string tenantId, string userID)
        {
            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);
            var installedApps = await graphClient.Users[userID].Teamwork.InstalledApps
                            .Request()
                            .Expand("teamsAppDefinition")
                            .GetAsync();
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            foreach (var res in installedApps.ToList().Take(10))
            {
                Dictionary<string, string> model = new Dictionary<string, string> {
                    { "Name", res.TeamsAppDefinition.DisplayName},
                    {"Id", res.Id }
                };
                list.Add(model);
            }

            return Json(list);
        }
        [HttpGet]
        [Route("GetUserAppInfo")]
        public async Task<List<string>> GetUserAppInfo(string tenantId, string appId, string userID)
       {
            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);
            var teamsAppInstallation = await graphClient.Users[userID].Teamwork
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
        public async Task<string> DeleteUserApp(string tenantId, string appId, string userID)
        {
            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);
            await graphClient.Users[userID].Teamwork.InstalledApps[appId]
                .Request()
                .DeleteAsync();
            return "Deleted Successfully";
        }

        [HttpPost]
        [Route("UpgradeUserApp")]
        public async Task<string> UpgradeUserApp(string tenantId, string appId, string userID)
        {
            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);
            await graphClient.Users[userID].Teamwork.InstalledApps[appId]
                .Upgrade()
                .Request()
                .PostAsync();
            return "Updated Successfully";
        }
    }
}

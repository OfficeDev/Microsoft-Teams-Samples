using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using RSCWithGraphAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RSCWithGraphAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public string appId;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// RSC Setup
        /// </summary>
        [Route("Demo")]
        public async Task<ActionResult> Demo(string tenantId, string groupId)
        {
            GraphServiceClient graphClient = await GetAuthenticatedClient(tenantId);
            var viewModel = new DemoViewModel()
            {
                Channels = await GetChannelsList(graphClient, tenantId, groupId),
                Permissions = await GetPermissionGrants(graphClient, tenantId, groupId)
            };
            return View(viewModel);
        }

        /// <summary>
        /// Configure Tab
        /// </summary>
        [Route("ConfigureTab")]
        public IActionResult ConfigureTab()
        {
            return View();
        }

        /// <summary>
        /// Send Notification Tab
        /// </summary>
        [Route("SendNotification")]
        public IActionResult SendNotification()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        private async Task<List<string>> GetChannelsList(GraphServiceClient graphClient, string tenantId, string groupId)
        {
            var result = await graphClient.Teams[groupId].Channels.Request()
                .GetAsync();

            return result.Select(r => r.DisplayName).ToList();
        }

        private async Task<List<string>> GetPermissionGrants(GraphServiceClient graphClient, string tenantId, string groupId)
        {
            var result = await graphClient.Groups[groupId].PermissionGrants.Request()
                .GetAsync();

            return result.Select(r => r.Permission).ToList();
        }


        /// <summary>
        ///Get Authenticated Client
        /// </summary>
        private async Task<GraphServiceClient> GetAuthenticatedClient(string tenantId)
        {
            var accessToken = await GetToken();
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                        return Task.CompletedTask;
                    }));

            return graphClient;
        }

        [HttpPost]
        [Route("GetInstalledAppList")]
        public async Task<JsonResult> GetInstalledAppList(string reciepientUserId)
        {
            // Replace with your Graph API endpoint and access token
            string graphApiEndpoint = $"https://graph.microsoft.com/v1.0/users/{reciepientUserId}/teamwork/installedApps/?$expand=teamsAppDefinition";

            var accessToken = await GetToken();

            // Create an HttpClient instance
            using (HttpClient client = new HttpClient())
            {
                // Set the base address for the Graph API
                client.BaseAddress = new Uri(graphApiEndpoint);

                // Set the authorization header with the access token
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                try
                {
                    // Make a GET request to retrieve user data
                    HttpResponseMessage response = await client.GetAsync(graphApiEndpoint);

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read and display the response content
                        string responseBody = await response.Content.ReadAsStringAsync();

                        var responseData = JsonConvert.DeserializeObject<ResponseData>(responseBody);
                        var installedAppList = responseData.Value;

                        foreach(AppData element in installedAppList)
                        {
                            if (element.TeamsAppDefinition.DisplayName == "RSC feed")
                            {
                                appId = element.Id;
                            }
                        };

                        if(appId != null)
                        {
                            await SendNotification(reciepientUserId, appId);
                            return Json("Message sent successfully");
                        }
                        else
                        {
                            return Json("App not installed for the user");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        return Json("Error occured");
                    }
                }
                catch (Exception ex)
                {          
                    Console.WriteLine($"Error: {ex.Message}");
                    return Json("Error occured"+ ex.Message);
                }
            }
        }

        /// <summary>
        /// Send activity feed notification to user.
        /// </summary>
        /// <param name="reciepientUserId"> Id of the user whom notification is to be sent</param>
        /// <param name="appId">App id for rsc app.</param>
        /// <returns></returns>
        public async Task<string> SendNotification(string reciepientUserId, string appId)
        {

            // Set your Graph API endpoint and access token
            string graphApiEndpoint = $"https://graph.microsoft.com/beta/users/{reciepientUserId}/teamwork/sendActivityNotification";

            var accessToken = await GetToken();

            // Create a JSON payload for the activity feed notification
            string jsonPayload = @"{
            ""topic"": {
                ""source"": ""entityUrl"",
                ""value"": ""https://graph.microsoft.com/beta/users/" + reciepientUserId + "/teamwork/installedApps/" + appId + @"""
            },
            ""activityType"": ""taskCreated"",
            ""previewText"": {
                ""content"": ""New Task Created""
            },
            ""templateParameters"": [{
                ""name"": ""taskName"",
                ""value"": ""test""
                }]
            }";

            using (HttpClient httpClient = new HttpClient())
            {
                // Set the base URL for the Graph API
                httpClient.BaseAddress = new Uri(graphApiEndpoint);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                try
                {
                    // Create a POST request with the JSON payload
                    HttpResponseMessage response = await httpClient.PostAsync(graphApiEndpoint, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Parse and print the response content
                        string content = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(content);
                        return "true";
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        return "Notification sent";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    return "Notification failed";
                }
            }
        }

        /// <summary>
        /// Get Token for given tenant.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public async Task<string> GetToken()
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(_configuration["ClientId"])
                                                  .WithClientSecret(_configuration["ClientSecret"])
                                                  .WithAuthority($"https://login.microsoftonline.com/{_configuration["TenantId"]}")
                                                  .WithRedirectUri("https://daemon")
                                                  .Build();

            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return result.AccessToken;
        }
    }
}
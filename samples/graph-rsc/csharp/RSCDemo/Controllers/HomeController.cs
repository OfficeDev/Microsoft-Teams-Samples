// <copyright file="HomeController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using Newtonsoft.Json;
using RSCWithGraphAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RSCDemo.Helper.GraphClient;

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
            GraphServiceClient graphClient = await GetAuthenticatedClient();
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
            var result = await graphClient.Teams[groupId].Channels.GetAsync();

            return result.Value.Select(r => r.DisplayName).ToList();
        }

        private async Task<List<string>> GetPermissionGrants(GraphServiceClient graphClient, string tenantId, string groupId)
        {
            var result = await graphClient.Groups[groupId].PermissionGrants.GetAsync();

            return result.Value.Select(r => r.Permission).ToList();
        }

        /// <summary>
        /// Get Authenticated Graph Client (fixed for Graph SDK 5+)
        /// </summary>
        private async Task<GraphServiceClient> GetAuthenticatedClient()
        {
            var accessToken = await GetToken();

            var accessTokenProvider = new SimpleAccessTokenProvider(accessToken);

            var authProvider = new BaseBearerTokenAuthenticationProvider(accessTokenProvider);

            var graphClient = new GraphServiceClient(authProvider);

            return graphClient;
        }

        /// <summary>
        /// Simple Access Token Provider for Graph SDK
        /// </summary>
        public class SimpleAccessTokenProvider : IAccessTokenProvider
        {
            private readonly string _accessToken;

            public SimpleAccessTokenProvider(string accessToken)
            {
                _accessToken = accessToken;
            }

            public AllowedHostsValidator AllowedHostsValidator { get; }

            public async Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object> additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
            {
                return _accessToken;
            }
        }

        // New helper class to return AccessToken
        private class SimpleAccessTokenCredential : TokenCredential
        {
            private readonly string _accessToken;

            public SimpleAccessTokenCredential(string accessToken)
            {
                _accessToken = accessToken;
            }

            public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
            {
                return new AccessToken(_accessToken, DateTimeOffset.UtcNow.AddHours(1));
            }

            public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
            {
                return new ValueTask<AccessToken>(new AccessToken(_accessToken, DateTimeOffset.UtcNow.AddHours(1)));
            }
        }

        /// <summary>
        /// Get the list of installed app for the user.
        /// </summary>
        /// <param name="reciepientUserId"> Id of the user whom notification is to be sent</param>
        /// <returns>Status which indicates notification sent of failed</returns>
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

                        foreach (AppData element in installedAppList)
                        {
                            if (element.TeamsAppDefinition.DisplayName == "RSC-GraphAPI ")
                            {
                                appId = element.Id;
                            }
                        }

                        if (appId != null)
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
                        return Json("Error occurred");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return Json("Error occurred: " + ex.Message);
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
                        return "Notification failed";
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
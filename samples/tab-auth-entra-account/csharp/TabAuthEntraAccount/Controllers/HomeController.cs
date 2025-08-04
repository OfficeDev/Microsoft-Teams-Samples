// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All Rights Reserved.
// </copyright>

namespace TabAuthEntraAccount.Controllers
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="configuration">IConfiguration instance.</param>
        /// <param name="httpClientFactory">IHttpClientFactory instance.</param>
        /// <param name="httpContextAccessor">IHttpContextAccessor instance.</param>
        public HomeController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            ViewBag.AzureClientId = _configuration["AzureAd:ClientId"];
            return View();
        }

        /// <summary>
        /// Retrieve access token and fetches user profile
        /// </summary>
        /// <returns>Returns user profile details</returns>
        [HttpPost]
        [Route("getGoogleAccessToken")]
        public async Task<JsonResult> GetGoogleAccessToken(string idToken)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage();
                request.RequestUri = new Uri("https://login.microsoftonline.com/common/oauth2/v2.0/token");
                request.Method = HttpMethod.Post;

                request.Headers.Add("Accept", "*/*");
                request.Headers.Add("User-Agent", "Thunder Client (https://www.thunderclient.com)");

                var formList = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("client_id", _configuration["AzureAd:ClientId"]),
                        new KeyValuePair<string, string>("client_secret", _configuration["AzureAd:ClientSecret"]),
                        new KeyValuePair<string, string>("code", idToken),
                        new KeyValuePair<string, string>("redirect_uri", _configuration["AzureAd:RedirectUri"]),
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/User.Read")
                    };

                request.Content = new FormUrlEncodedContent(formList);

                var response = await client.SendAsync(request);
                var tokenResult = await response.Content.ReadAsStringAsync();
                
                using var doc = JsonDocument.Parse(tokenResult);
                var accessToken = doc.RootElement.GetProperty("access_token").GetString();

                // Call Microsoft Graph
                var graphClient = new HttpClient();
                graphClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var graphResponse = await graphClient.GetAsync("https://graph.microsoft.com/v1.0/me");
                var graphContent = await graphResponse.Content.ReadAsStringAsync();

                using var graphDoc = JsonDocument.Parse(graphContent);
                
                // Return only basic personal information
                var result = new
                {
                    // Personal Information only
                    id = graphDoc.RootElement.TryGetProperty("id", out var idProp) ? idProp.GetString() : null,
                    displayName = graphDoc.RootElement.TryGetProperty("displayName", out var displayNameProp) ? displayNameProp.GetString() : null,
                    givenName = graphDoc.RootElement.TryGetProperty("givenName", out var givenNameProp) ? givenNameProp.GetString() : null,
                    surname = graphDoc.RootElement.TryGetProperty("surname", out var surnameProp) ? surnameProp.GetString() : null,
                    userPrincipalName = graphDoc.RootElement.TryGetProperty("userPrincipalName", out var upnProp) ? upnProp.GetString() : null,
                    
                    // For backward compatibility (legacy code support)
                    name = graphDoc.RootElement.TryGetProperty("displayName", out var nameProp) ? nameProp.GetString() : null,
                    email = graphDoc.RootElement.TryGetProperty("mail", out var emailProp) ? emailProp.GetString() : 
                            graphDoc.RootElement.TryGetProperty("userPrincipalName", out var emailFallbackProp) ? emailFallbackProp.GetString() : null
                };

                var googleUserDetailString = JsonConvert.SerializeObject(result);

                return Json(googleUserDetailString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        // Handle error route
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
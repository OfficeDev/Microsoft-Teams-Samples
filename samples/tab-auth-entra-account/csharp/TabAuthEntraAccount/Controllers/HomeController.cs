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

    /// <summary>
    /// HomeController handles the main application functionality including authentication
    /// and user profile retrieval for the Microsoft Teams tab application.
    /// This controller manages the OAuth 2.0 flow and Microsoft Graph API interactions.
    /// </summary>
    public class HomeController : Controller
    {
        // Configuration service for accessing appsettings.json values
        private readonly IConfiguration _configuration;
        
        // HTTP client factory for creating HTTP clients (not currently used but available for future use)
        private readonly IHttpClientFactory _httpClientFactory;
        
        // HTTP context accessor for accessing current HTTP context (not currently used but available for future use)
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

        /// <summary>
        /// Main entry point for the Teams tab application.
        /// Sets up the Azure AD Client ID for use in the JavaScript authentication flow.
        /// </summary>
        /// <returns>The Index view with Azure AD Client ID configured in ViewBag</returns>
        public IActionResult Index()
        {
            // Pass the Azure AD Client ID to the view for JavaScript authentication
            ViewBag.AzureClientId = _configuration["AzureAd:ClientId"];
            return View();
        }

        /// <summary>
        /// Exchanges an authorization code for an access token and retrieves user profile information.
        /// This method implements the OAuth 2.0 authorization code flow and calls Microsoft Graph API
        /// to fetch user profile data for display in the Teams tab.
        /// </summary>
        /// <param name="idToken">The authorization code received from the OAuth 2.0 authorization flow</param>
        /// <returns>JSON result containing user profile information from Microsoft Graph</returns>
        [HttpPost]
        [Route("getAuthAccessToken")]
        public async Task<JsonResult> GetAuthAccessToken(string idToken)
        {
            try
            {
                // Step 1: Exchange authorization code for access token
                var client = new HttpClient();
                var request = new HttpRequestMessage();
                request.RequestUri = new Uri("https://login.microsoftonline.com/common/oauth2/v2.0/token");
                request.Method = HttpMethod.Post;

                // Set required headers for the token request
                request.Headers.Add("Accept", "*/*");
                request.Headers.Add("User-Agent", "Thunder Client (https://www.thunderclient.com)");

                // Prepare the form data for the token exchange request
                var formList = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("client_id", _configuration["AzureAd:ClientId"]),
                        new KeyValuePair<string, string>("client_secret", _configuration["AzureAd:ClientSecret"]),
                        new KeyValuePair<string, string>("code", idToken), // Authorization code from client
                        new KeyValuePair<string, string>("redirect_uri", _configuration["AzureAd:RedirectUri"]),
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/User.Read") // Request permission to read user profile
                    };

                request.Content = new FormUrlEncodedContent(formList);

                // Send the token request to Microsoft Identity Platform
                var response = await client.SendAsync(request);
                var tokenResult = await response.Content.ReadAsStringAsync();
                
                // Extract the access token from the response
                using var doc = JsonDocument.Parse(tokenResult);
                var accessToken = doc.RootElement.GetProperty("access_token").GetString();

                // Step 2: Use access token to call Microsoft Graph API
                var graphClient = new HttpClient();
                graphClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var graphResponse = await graphClient.GetAsync("https://graph.microsoft.com/v1.0/me");
                var graphContent = await graphResponse.Content.ReadAsStringAsync();

                using var graphDoc = JsonDocument.Parse(graphContent);
                
                // Step 3: Extract and format user profile information
                // Return only basic personal information to protect user privacy
                var result = new
                {
                    // Core user identification
                    id = graphDoc.RootElement.TryGetProperty("id", out var idProp) ? idProp.GetString() : null,
                    displayName = graphDoc.RootElement.TryGetProperty("displayName", out var displayNameProp) ? displayNameProp.GetString() : null,
                    givenName = graphDoc.RootElement.TryGetProperty("givenName", out var givenNameProp) ? givenNameProp.GetString() : null,
                    // Basic user name information
                    surname = graphDoc.RootElement.TryGetProperty("surname", out var surnameProp) ? surnameProp.GetString() : null,
                    userPrincipalName = graphDoc.RootElement.TryGetProperty("userPrincipalName", out var upnProp) ? upnProp.GetString() : null,
                    
                    // Legacy properties maintained for backward compatibility with existing client code
                    name = graphDoc.RootElement.TryGetProperty("displayName", out var nameProp) ? nameProp.GetString() : null,
                    // Try to get email from 'mail' property first, fallback to userPrincipalName if not available
                    email = graphDoc.RootElement.TryGetProperty("mail", out var emailProp) ? emailProp.GetString() : 
                            graphDoc.RootElement.TryGetProperty("userPrincipalName", out var emailFallbackProp) ? emailFallbackProp.GetString() : null
                };

                // Serialize the user profile data to JSON string for client consumption
                var UserDetailString = JsonConvert.SerializeObject(result);

                return Json(UserDetailString);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine(ex);
                return null; // Return null on error - client should handle this case
            }
        }

        /// <summary>
        /// Handles application errors and displays the error page.
        /// This action is called when an unhandled exception occurs in the application.
        /// It provides debugging information in development environments.
        /// </summary>
        /// <returns>Error view with diagnostic information</returns>
        // Disable response caching for error pages to ensure fresh error information
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Create error view model with current request/activity ID for correlation
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
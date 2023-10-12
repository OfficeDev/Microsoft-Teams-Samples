// <copyright file="AuthHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net;
using System.Net.Http.Headers;

namespace MeetingTranscriptRecording.Helper
{
    public class AuthHelper
    {

        public static async Task<string> GetApiData(string url, string accessToken)
        {
            try
            {
                var results = string.Empty;
                // Create an HttpClient instance
                using (HttpClient client = new HttpClient())
                {
                    // Replace with your Graph API endpoint and access token
                    string graphApiEndpoint = url;

                    // Set the base address for the Graph API
                    client.BaseAddress = new Uri(graphApiEndpoint);

                    // Set the authorization header with the access token
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    // Make a GET request to retrieve user data
                    HttpResponseMessage response = await client.GetAsync(graphApiEndpoint);

                    // Check the status code
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        // Check if the request was successful
                        if (response.IsSuccessStatusCode)
                        {
                            results =  await response.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            throw new HttpRequestException($"HTTP request failed with status code: {response.StatusCode}");
                        }
                    }
                    return results;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Error making POST request: {ex.Message}");
            }
        }


        /// <summary>
        /// Get token using client credentials flow
        /// </summary>
        /// <param name="configuration">IConfiguration instance.</param>
        /// <param name="httpClientFactory">IHttpClientFactory instance.</param>
        /// <param name="httpContextAccessor">IHttpContextAccessor instance.</param>
        /// <returns>App access token on behalf of user.</returns>
        public static async Task<string> GetAccessTokenOnBehalfUserAsync(IConfiguration configuration, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            var tenantId = configuration["AzureAd:TenantId"];
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(configuration["AzureAd:MicrosoftAppId"])
                                                .WithClientSecret(configuration["AzureAd:MicrosoftAppPassword"])
                                                .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                                                .Build();

            try
            {
                var httpContext = httpContextAccessor.HttpContext;
                httpContext.Request.Headers.TryGetValue("Authorization", out StringValues assertion);

                var idToken = assertion.ToString().Split(" ")[1];
                UserAssertion assert = new UserAssertion(idToken);
                List<string> scopes = new List<string>
                {
                    "User.Read"
                };

                var responseToken = await app.AcquireTokenOnBehalfOf(scopes, assert).ExecuteAsync();

                return responseToken.AccessToken.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
    }
}
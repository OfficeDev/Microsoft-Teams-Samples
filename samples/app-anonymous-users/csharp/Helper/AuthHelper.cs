// <copyright file="AuthHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AnonymousUsers.Helper
{
    public class AuthHelper
    {
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
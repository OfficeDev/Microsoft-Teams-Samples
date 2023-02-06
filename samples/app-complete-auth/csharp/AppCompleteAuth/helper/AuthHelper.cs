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

namespace AppCompleteAuth.helper
{
    public class AuthHelper
    {
        /// <summary>
        /// Retrieve Valid Audiences.
        /// </summary>
        /// <param name="configuration">IConfiguration instance.</param>
        /// <returns>Valid Audiences.</returns>
        public static IEnumerable<string> GetValidAudiences(IConfiguration configuration)
        {
            var clientId = configuration["AzureAd:MicrosoftAppId"];
            var applicationIdUri = configuration["AzureAd:ApplicationIdURI"];
            var validAudiences = new List<string> { clientId, applicationIdUri.ToLower() };
            return validAudiences;
        }
        /// <summary>
        /// Retrieve Valid Issuers.
        /// </summary>
        /// <param name="configuration">IConfiguration instance.</param>
        /// <returns>Valid Issuers.</returns>
        public static IEnumerable<string> GetValidIssuers(IConfiguration configuration)
        {
            var tenantId = configuration["AzureAd:TenantId"];
            var validIssuers = GetSettings(configuration);
            validIssuers = validIssuers.Select(validIssuer => validIssuer.Replace("tenantId", tenantId));
            return validIssuers;
        }

        /// <summary>
        /// Audience Validator.
        /// </summary>
        /// <param name="tokenAudiences">Token audiences.</param>
        /// <param name="securityToken">Security token.</param>
        /// <param name="validationParameters">Validation parameters.</param>
        /// <returns>Audience validator status.</returns>
        public static bool AudienceValidator(
            IEnumerable<string> tokenAudiences,
            SecurityToken securityToken,
            TokenValidationParameters validationParameters)
        {
            if (tokenAudiences == null || tokenAudiences.Count() == 0)
            {
                throw new ApplicationException("No audience defined in token!");
            }

            var validAudiences = validationParameters.ValidAudiences;

            if (validAudiences == null || validAudiences.Count() == 0)
            {
                throw new ApplicationException("No valid audiences defined in validationParameters!");
            }

            foreach (var tokenAudience in tokenAudiences)
            {
                if (validAudiences.Any(validAudience => validAudience.Equals(tokenAudience, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
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

        /// <summary>
        /// Retrieve Settings.
        /// </summary>
        /// <param name="configuration">IConfiguration instance.</param>
        /// <returns>Configuration settings for valid issuers.</returns>
        private static IEnumerable<string> GetSettings(IConfiguration configuration)
        {
            var configurationSettingsValue = configuration["AzureAd:ValidIssuers"];
            var settings = configurationSettingsValue
                ?.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                ?.Select(p => p.Trim());

            if (settings == null)
            {
                throw new ApplicationException($"AzureAd:ValidIssuers does not contain a valid value in the configuration file.");
            }

            return settings;
        }
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StaggeredPermission.helper
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
            var clientId = configuration["AzureAd:ClientId"];
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
        /// Get token using on-behalf-of flow.
        /// </summary>
        /// <param name="configuration">IConfiguration instance.</param>
        /// <param name="idToken">The id token from the client.</param>
        /// <returns>App access token on behalf of user.</returns>
        public static async Task<string> GetAccessTokenOnBehalfUserAsync(IConfiguration configuration, string idToken)
        {
            var tenantId = configuration["AzureAd:TenantId"];
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(configuration["AzureAd:ClientId"])
                                                .WithClientSecret(configuration["AzureAd:AppSecret"])
                                                .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                                                .Build();

            try
            {
                var assert = new UserAssertion(idToken);
                var scopes = new List<string> { "https://graph.microsoft.com/User.Read" };
                var responseToken = await app.AcquireTokenOnBehalfOf(scopes, assert).ExecuteAsync();

                return responseToken.AccessToken;
            }
            catch (Exception ex)
            {
                return ex.Message;
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
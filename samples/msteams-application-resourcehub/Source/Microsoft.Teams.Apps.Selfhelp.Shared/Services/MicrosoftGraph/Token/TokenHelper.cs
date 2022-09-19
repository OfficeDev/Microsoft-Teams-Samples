namespace Microsoft.Teams.Apps.Selfhelp.Shared.Services.MicrosoftGraph.Token
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Identity.Client;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Configuration;
    using Newtonsoft.Json;

    /// <summary>
    /// Implements the methods that are defined in <see cref="ITokenHelper"/>.
    /// Helper class for token generation, validation and generate Azure Active Directory user access token for given resource, e.g. Microsoft Graph, SharePoint.
    /// </summary>
    public class TokenHelper : ITokenHelper
    {
        /// <summary>
        /// Login request base URL.
        /// </summary>
        private const string LoginRequestBaseUrl = "https://login.microsoftonline.com";

        /// <summary>
        /// Microsoft Graph API base url.
        /// </summary>
        private const string GraphAPIBaseURL = "https://graph.microsoft.com/";

        /// <summary>
        /// Provides a base class for sending HTTP requests and receiving HTTP responses from a resource identified by a URI.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// Http Context.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Instance to log details in application insights.
        /// </summary>
        private readonly ILogger<TokenHelper> logger;

        /// <summary>
        /// Confidentiality Application.
        /// </summary>
        private readonly IConfidentialClientApplication confidentialClientApplication;

        /// <summary>
        /// Bot settings.
        /// </summary>
        private IOptions<BotSettings> botSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenHelper"/> class.
        /// </summary>
        /// <param name="httpClient">Instance of HttpClient.</param>
        /// <param name="logger">Instance of ILogger.</param>
        public TokenHelper(
            HttpClient httpClient,
            IConfidentialClientApplication confidentialClientApplication,
            IHttpContextAccessor httpContextAccessor,
            IOptions<BotSettings> botSettings,
            ILogger<TokenHelper> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            this.confidentialClientApplication = confidentialClientApplication;
            this.botSettings = botSettings;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IGraphUtilityHelper"/> class.
        /// Gets the application token.
        /// </summary>
        /// <param name="tenantId">Unique id of tenant.</param>
        /// <param name="clientId">The application client id.</param>
        /// <param name="clientSecret">The application client secret.</param>
        /// <returns>The application token.</returns>
        public async Task<GraphTokenResponse> ObtainApplicationTokenAsync(string tenantId, string clientId, string clientSecret)
        {
            var requestUrl = $"{LoginRequestBaseUrl}/{tenantId}/oauth2/v2.0/token";
            var stringQuery = $"client_id={clientId}&scope={Uri.EscapeDataString($"{GraphAPIBaseURL}/.default")}&client_secret={Uri.EscapeDataString(clientSecret)}&grant_type=client_credentials";

            using (var httpContent = new StringContent(stringQuery, Encoding.UTF8, "application/x-www-form-urlencoded"))
            {
                var response = await this.httpClient.PostAsync(new Uri(requestUrl), httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var graphTokenResponse = JsonConvert.DeserializeObject<GraphTokenResponse>(responseContent);
                    this.logger.LogInformation($"Token received: {graphTokenResponse.AccessToken}");

                    return graphTokenResponse;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Method to generate token.
        /// </summary>
        /// <returns>Returns the token.</returns>
        public async Task<string> ObtainDelegatedGraphTokenAsync(StringValues encodedToken)
        {
            var token = this.GetJsonToken(encodedToken);
            var jwtToken = (JwtSecurityToken)token;
            var httpContext = this.httpContextAccessor.HttpContext;
            var accessToken = await httpContext.GetTokenAsync("access_token");
            var user = httpContext.User;
            string userName = user.FindFirstValue(ClaimTypes.Upn) ?? user.FindFirstValue(ClaimTypes.Email);
            var authority = $"https://login.microsoftonline.com/Common";
            var authContext = new AuthenticationContext(authority, true);
            var assertionType = "urn:ietf:params:oauth:grant-type:jwt-bearer";
            var userAssertion = new IdentityModel.Clients.ActiveDirectory.UserAssertion(jwtToken.RawData, assertionType, userName);
            var clientCredential = new IdentityModel.Clients.ActiveDirectory.ClientCredential(this.botSettings.Value.MicrosoftAppId, this.botSettings.Value.MicrosoftAppPassword);

            IdentityModel.Clients.ActiveDirectory.AuthenticationResult res = null;
            try
            {
                res = await authContext.AcquireTokenAsync("https://graph.microsoft.com", clientCredential, userAssertion);
            }
            catch (AdalServiceException ex)
            {
                Console.WriteLine(ex.ToString());
                if (ex.ErrorCode == "invalid_grant" || ex.ErrorCode == "interaction_required")
                {
                    // this.Response.StatusCode = 403;
                    return null;
                }
                else
                {
                    // this.Response.StatusCode = 500;
                    return null;
                }
            }

            accessToken = res.AccessToken;

            return accessToken;
        }

        /// <summary>
        /// Get Json token.
        /// </summary>
        /// <param name="encodedToken">Encoded token.</param>
        /// <returns>Returns the security token.</returns>
        private IdentityModel.Tokens.SecurityToken GetJsonToken(StringValues encodedToken)
        {
            var token = this.CleanToken(encodedToken);

            if (token == null)
            {
                return null;
            }

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token);
            return jsonToken;
        }

        /// <summary>
        /// Clean token.
        /// </summary>
        /// <param name="encodedToken">Encoded token.</param>
        /// <returns>Returns the string token.</returns>
        private string CleanToken(StringValues encodedToken)
        {
            var tokenSplitArray = encodedToken.ToString().Split(" ");

            if (!(tokenSplitArray.Length == 2 && tokenSplitArray[0] == "Bearer"))
            {
                return null;
            }

            return tokenSplitArray[1];
        }
    }
}
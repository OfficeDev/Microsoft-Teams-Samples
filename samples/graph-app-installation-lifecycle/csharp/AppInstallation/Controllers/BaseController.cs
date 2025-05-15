using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace AppInstallation.Controllers
{
    public class BaseController : Controller
    {
        private readonly IConfiguration _configuration;

        public BaseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public class SimpleAccessTokenProvider : IAccessTokenProvider
        {
            private readonly string _accessToken;

            public SimpleAccessTokenProvider(string accessToken)
            {
                _accessToken = accessToken;
            }

            public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object> context = null, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_accessToken);
            }

            public AllowedHostsValidator AllowedHostsValidator => new AllowedHostsValidator();
        }

        public GraphServiceClient GetAuthenticatedClient(string token)
        {
            var tokenProvider = new SimpleAccessTokenProvider(token);
            var authProvider = new BaseBearerTokenAuthenticationProvider(tokenProvider);

            return new GraphServiceClient(authProvider);
        }

        [HttpGet]
        [Route("GetToken")]
        public async Task<string> GetToken(string tenantId)
        {

            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(_configuration["ClientId"])
                                                  .WithClientSecret(_configuration["ClientSecret"])
                                                  .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                                                  .WithRedirectUri("https://daemon")
                                                  .Build();

            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return result.AccessToken;
        }
    }
}

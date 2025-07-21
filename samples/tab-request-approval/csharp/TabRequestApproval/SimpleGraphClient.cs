using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;

namespace TabRequestApproval
{
    public class SimpleGraphClient
    {
        public static GraphServiceClient GetGraphClient(string accessToken)
        {
            TokenProvider provider = new TokenProvider();
            provider.token = accessToken;
            var authenticationProvider = new BaseBearerTokenAuthenticationProvider(provider);
            var graphServiceClient = new GraphServiceClient(authenticationProvider);
            return graphServiceClient;
        }

    }

    public class TokenProvider : IAccessTokenProvider
    {
        public string token { get; set; }
        public AllowedHostsValidator AllowedHostsValidator => throw new NotImplementedException();

        public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(token);
        }
    }
}
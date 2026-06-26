using Microsoft.Graph;
using Azure.Core;
using System;
using System.Threading.Tasks;

namespace AppCatalogSample.Helper
{
    public class GraphClient
    {
        private readonly string _token;

        public GraphClient(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            _token = token;
        }

        public GraphServiceClient GetAuthenticatedClient()
        {
            var tokenCredential = new StaticTokenCredential(_token);

            return new GraphServiceClient(tokenCredential, new[] { "https://graph.microsoft.com/.default" });
        }
    }

    // Custom TokenCredential implementation for static tokens
    public class StaticTokenCredential : TokenCredential
    {
        private readonly string _token;

        public StaticTokenCredential(string token)
        {
            _token = token;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, System.Threading.CancellationToken cancellationToken)
        {
            return new AccessToken(_token, DateTimeOffset.UtcNow.AddHours(1));
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, System.Threading.CancellationToken cancellationToken)
        {
            return new ValueTask<AccessToken>(new AccessToken(_token, DateTimeOffset.UtcNow.AddHours(1)));
        }
    }
}

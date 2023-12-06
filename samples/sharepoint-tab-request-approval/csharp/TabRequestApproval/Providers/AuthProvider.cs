// <copyright file="AuthProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Providers
{
	using System;
	using System.Threading.Tasks;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Identity.Client;

	/// <summary>
	/// Authentication provider.
	/// </summary>
	public class AuthProvider : IAuthProvider
    {
        /// <summary>
        /// Represents the appsettings.json file.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthProvider"/> class.
        /// Creates an auth provider object.
        /// </summary>
        /// <param name="configuration">Represents the appsettings.json file details.</param>
        public AuthProvider(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Creates an access token for the Graph API with the Graph scope.
        /// </summary>
        /// <returns>A graph access token.</returns>
        public async Task<string> GetGraphAccessTokenAsync()
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(this.configuration["AzureAd:MicrosoftAppId"])
                                      .WithClientSecret(this.configuration["AzureAd:MicrosoftAppPassword"])
                                      .WithAuthority($"https://login.microsoftonline.com/{this.configuration["AzureAdConsumer:TenantId"]}")
                                      .WithRedirectUri("https://daemon")
                                      .Build();

            string[] scopes = new string[] { this.configuration["AccessTokenScopes:Graph"] };

            AuthenticationResult result = await app.AcquireTokenForClient(scopes).ExecuteAsync().ConfigureAwait(false);

            return result.AccessToken;
        }
    }
}

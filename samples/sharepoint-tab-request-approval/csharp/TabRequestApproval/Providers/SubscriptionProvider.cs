// <copyright file="SubscriptionProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Providers
{
	using System;
	using System.Security.Cryptography.X509Certificates;
	using System.Threading.Tasks;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Graph;
	using TabActivityFeed.Helpers;

	/// <summary>
	/// Subscription Provider.
	/// </summary>
	public class SubscriptionProvider : ISubscriptionProvider
    {
        /// <summary>
        /// Represents the resource to subscribe to in-order to obtain all messages in all teams in a tenant.
        /// </summary>
        private const string TeamSubscriptionResource = "/teams/getAllMessages";

        /// <summary>
        /// Represents the change events that each subscription should be listening for.
        /// </summary>
        private const string ChangeTypes = "created,deleted,updated";

        /// <summary>
        /// Represents the notification url for subscriptions.
        /// </summary>
        private const string SubscriptionNotificationUrl = "/Subscriptions/processNotification";

        /// <summary>
        /// Represents the certificate path.
        /// </summary>
        private const string CertificatePath = @".cer";

        /// <summary>
        /// Represents the appsettings.json file.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Represents the resource to subscribe to in-order to obtain all messages in a chat that a certain app is installed in.
        /// </summary>
        private readonly string chatSubscriptionResource;

        /// <summary>
        /// Represents the auth provider.
        /// </summary>
        private readonly IAuthProvider authProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionProvider"/> class.
        /// Creates a subscription provider object.
        /// </summary>
        /// <param name="configuration">Represents the appsettings details.</param>
        /// <param name="authProvider">Represents the auth provider.</param>
        public SubscriptionProvider(IConfiguration configuration, IAuthProvider authProvider)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            this.chatSubscriptionResource = $"/appCatalogs/teamsApps/{this.configuration["AzureAd:TeamsAppId"]}/installedToChats/getAllMessages";
            this.authProvider = authProvider ?? throw new ArgumentNullException(nameof(authProvider));
        }

        /// <summary>
        /// Creates a subscription at a certain chat-related resource.
        /// </summary>
        /// <returns>A subscription object for that chat-related resource.</returns>
        /// <exception cref="Exception">Represents an unexpected error.</exception>
        public async Task<Subscription> CreateChatSubscriptionAsync()
        {
            try
            {
                string graphAccessToken = await this.authProvider.GetGraphAccessTokenAsync().ConfigureAwait(false);

                GraphServiceClient graphServiceClient = ServiceClients.GetGraphServiceClient(graphAccessToken);

                X509Certificate2 cert = new X509Certificate2(SubscriptionProvider.CertificatePath);

                byte[] export = cert.Export(X509ContentType.Pfx);

                var subscription = new Subscription
                {
                    Resource = this.chatSubscriptionResource,
                    ChangeType = SubscriptionProvider.ChangeTypes,
                    NotificationUrl = $"{this.configuration["AzureAd:ApplicationUrl"]}{SubscriptionProvider.SubscriptionNotificationUrl}",
                    ExpirationDateTime = DateTimeOffset.UtcNow.AddMinutes(59), // less than 60 to avoid further security
                    IncludeResourceData = true,
                    EncryptionCertificate = Convert.ToBase64String(export),
                    EncryptionCertificateId = cert.Thumbprint,
                };

                Subscription result = await graphServiceClient.Subscriptions.Request().AddAsync(subscription).ConfigureAwait(false);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to create chat subscription. Reason: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a subscription at a certain team-related resource.
        /// </summary>
        /// <returns>A subscription object for that team-related resource.</returns>
        /// <exception cref="Exception">Represents an unexpected error.</exception>
        public async Task<Subscription> CreateTeamsSubscriptionAsync()
        {
            try
            {
                string graphAccessToken = await this.authProvider.GetGraphAccessTokenAsync().ConfigureAwait(false);
                GraphServiceClient graphServiceClient = ServiceClients.GetGraphServiceClient(graphAccessToken);

                X509Certificate2 cert = new X509Certificate2(SubscriptionProvider.CertificatePath);

                byte[] export = cert.Export(X509ContentType.Pfx);
                var subscription = new Subscription
                {
                    Resource = SubscriptionProvider.TeamSubscriptionResource,
                    ChangeType = SubscriptionProvider.ChangeTypes,
                    NotificationUrl = $"{this.configuration["AzureAd:ApplicationUrl"]}{SubscriptionProvider.SubscriptionNotificationUrl}",
                    ExpirationDateTime = DateTimeOffset.UtcNow.AddMinutes(59), // less than 60 to avoid further security
                    IncludeResourceData = true,
                    EncryptionCertificate = Convert.ToBase64String(export),
                    EncryptionCertificateId = cert.Thumbprint,
                };

                Subscription result = await graphServiceClient.Subscriptions.Request().AddAsync(subscription).ConfigureAwait(false);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to create team subscription. Reason: {ex.Message}");
            }
        }
    }
}

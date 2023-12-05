// <copyright file="SubscriptionProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Cryptography.Xml;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using Microsoft.Online.AggregatorService.Encryptor;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NuGet.Protocol;
    using TabActivityFeed.Helpers;
    using TabActivityFeed.Models;
    using TabRequestApproval.Helpers;
    using User = TabActivityFeed.Models.User;

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
        /// Represents the logger to use in this provider.
        /// </summary>
        private readonly ILogger<SubscriptionProvider> logger;

        /// <summary>
        /// Represents the resource to subscribe to in-order to obtain all messages in a chat that a certain app is installed in.
        /// </summary>
        private readonly string chatSubscriptionResource;

        /// <summary>
        /// Represents the auth provider.
        /// </summary>
        private readonly IAuthProvider authProvider;

        /// <summary>
        /// Represents the container provider.
        /// </summary>
        private readonly IContainerProvider containerProvider;

        /// <summary>
        /// Represents container permission provider.
        /// </summary>
        private readonly IContainerPermissionProvider containerPermissionProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionProvider"/> class.
        /// Creates a subscription provider object.
        /// </summary>
        /// <param name="logger">Represents the logger.</param>
        /// <param name="containerProvider">Represents the container provider.</param>
        /// <param name="configuration">Represents the appsettings details.</param>
        /// <param name="authProvider">Represents the auth provider.</param>
        /// <param name="containerPermissionProvider">Represents the container permission provider.</param>
        public SubscriptionProvider(IConfiguration configuration, IAuthProvider authProvider, ILogger<SubscriptionProvider> logger, IContainerProvider containerProvider, IContainerPermissionProvider containerPermissionProvider)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.chatSubscriptionResource = $"/appCatalogs/teamsApps/{this.configuration["AzureAd:TeamsAppId"]}/installedToChats/getAllMessages";
            this.authProvider = authProvider ?? throw new ArgumentNullException(nameof(authProvider));
            this.containerProvider = containerProvider ?? throw new ArgumentNullException(nameof(containerProvider));
            this.containerPermissionProvider = containerPermissionProvider ?? throw new ArgumentNullException(nameof(containerPermissionProvider));
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

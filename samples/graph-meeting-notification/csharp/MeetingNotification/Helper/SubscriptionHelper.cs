// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MeetingNotification.Helper
{
    using MeetingNotification.Model.Configuration;
    using MeetingNotification.Provider;
    using Microsoft.Bot.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    public class SubscriptionHelper
    {
        private const int SubscriptionExpirationTimeInMinutes = 60;

        private const int SubscriptionExpirationTimeInDays = 1;

        /// <summary>
        /// Stores the Bot configuration values.
        /// </summary>
        private readonly IOptions<BotConfiguration> botSettings;

        private readonly ILogger _logger;

        private readonly GraphBetaClient graphBetaClientProvider;

        public static readonly Dictionary<string, Subscription> Subscriptions = new Dictionary<string, Subscription>();

        public SubscriptionHelper(IOptions<BotConfiguration> botSettings, ILogger<SubscriptionHelper> logger, GraphBetaClient graphBetaClientProvider)
        {
            this.botSettings = botSettings;
            this.graphBetaClientProvider = graphBetaClientProvider;
            _logger = logger;
        }

        public async Task InitializeAllSubscription(string meetingJoinUrl)
        {
            _logger.LogWarning("InitializeAllSubscription-started");

            await CreateNewSubscription(meetingJoinUrl);

            await this.CheckSubscriptions().ConfigureAwait(false);
            _logger.LogWarning("InitializeAllSubscription-completed");
        }

        public async Task CheckSubscriptions()
        {
            _logger.LogWarning($"Checking subscriptions {DateTime.UtcNow.ToString("h:mm:ss.fff")}");

            foreach (var subscription in Subscriptions)
            {
                await RenewSubscription(subscription.Value);
            }
        }

        private async Task<Subscription> CreateNewSubscription(string meetingJoinUrl)
        {
            _logger.LogWarning($"CreateNewSubscription-start: {meetingJoinUrl}");

            if (string.IsNullOrEmpty(meetingJoinUrl))
                return null;

            var resource = $"/communications/onlineMeetings/?$filter=JoinWebUrl eq '{meetingJoinUrl}'";
            return await CreateSubscriptionWithResource(resource);
        }

        private async Task<Subscription> CreateSubscriptionWithResource(string resource)
        {
            if (string.IsNullOrEmpty(resource))
                return null;

            var graphServiceClient = graphBetaClientProvider.GetGraphClientforApp();

            if (Subscriptions.Any(s => s.Value.Resource == resource && s.Value.ExpirationDateTime < DateTime.UtcNow))
                return null;

            IGraphServiceSubscriptionsCollectionPage existingSubscriptions = null;
            try
            {
                existingSubscriptions = await graphServiceClient
                          .Subscriptions
                          .Request().
                          GetAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CreateNewSubscription-ExistingSubscriptions-Failed: {resource}");
                return null;
            }

            var notificationUrl = this.botSettings.Value.BaseUrl + "/api/notifications";

            var existingSubscription = existingSubscriptions.FirstOrDefault(s => s.Resource == resource);

            if (existingSubscription != null && existingSubscription.NotificationUrl != notificationUrl)
            {
                _logger.LogWarning($"CreateNewSubscription-ExistingSubscriptionFound: {resource}");
                await DeleteSubscription(existingSubscription);
                existingSubscription = null;
            }

            if (existingSubscription == null)
            {
                var sub = new Subscription
                {
                    Resource = resource,
                    EncryptionCertificate = this.botSettings.Value.Base64EncodedCertificate,
                    EncryptionCertificateId = this.botSettings.Value.EncryptionCertificateId,
                    IncludeResourceData = true,
                    ChangeType = "updated",
                    NotificationUrl = notificationUrl,
                    ClientState = "ClientState",
                    ExpirationDateTime = DateTime.UtcNow + new TimeSpan(days: SubscriptionExpirationTimeInDays, hours: 0, minutes: SubscriptionExpirationTimeInMinutes, seconds: 0)
                };

                try
                {
                    existingSubscription = await graphServiceClient
                              .Subscriptions
                              .Request()
                              .AddAsync(sub);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"CreateNewSubscription-Failed: {resource}");
                    return null;
                }
            }

            Subscriptions[existingSubscription.Id] = existingSubscription;

            _logger.LogWarning($"Subscription Created for TeamId: {resource}");

            return existingSubscription;
        }

        private async Task RenewSubscription(Subscription subscription)
        {
            _logger.LogWarning($"Current subscription: {subscription.Id}, Expiration: {subscription.ExpirationDateTime}");

            var graphServiceClient = graphBetaClientProvider.GetGraphClientforApp();

            var newSubscription = new Subscription
            {
                ExpirationDateTime = DateTime.UtcNow.AddDays(1)
            };

            try
            {
                await graphServiceClient
                     .Subscriptions[subscription.Id]
                     .Request()
                     .UpdateAsync(newSubscription);
                subscription.ExpirationDateTime = newSubscription.ExpirationDateTime;
                _logger.LogWarning($"Renewed subscription: {subscription.Id}, New Expiration: {subscription.ExpirationDateTime}");
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Subscriptions.Remove(subscription.Id);
                    _logger.LogError(ex, $"HttpStatusCode.NotFound : Creating new subscription : {subscription.Id}");

                    await CreateSubscriptionWithResource(subscription.Resource);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Update Subscription Failed: {subscription.Id}");
            }
        }

        private async Task DeleteSubscription(Subscription subscription)
        {
            _logger.LogWarning($"Current subscription: {subscription.Id}, Expiration: {subscription.ExpirationDateTime}");

            var graphServiceClient = graphBetaClientProvider.GetGraphClientforApp();

            try
            {
                await graphServiceClient
                     .Subscriptions[subscription.Id]
                     .Request()
                     .DeleteAsync();

                _logger.LogWarning($"Deleted subscription: {subscription.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Delete Subscription Failed: {subscription.Id}");
            }
        }
    }
}
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace ChangeNotification.Helper
{
    using ChangeNotification.Model.Configuration;
    using ChangeNotification.Provider;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class SubscriptionManager : BackgroundService
    {
        private const int SubscriptionExpirationTimeInMinutes = 60;
        private const int SubscriptionRenewTimeInMinutes = 15;

        /// <summary>
        /// Stores the Bot configuration values.
        /// </summary>
        private readonly IOptions<ApplicationConfiguration> botSettings;
        private readonly ILogger _logger;
        private readonly GraphClient graphBetaClientProvider;
        public static readonly Dictionary<string, Subscription> Subscriptions = new Dictionary<string, Subscription>();
        private string pageId;

        public SubscriptionManager(IOptions<ApplicationConfiguration> botSettings, ILogger<SubscriptionManager> logger, GraphClient graphBetaClientProvider)
        {
            this.botSettings = botSettings;
            this.graphBetaClientProvider = graphBetaClientProvider;
            _logger = logger;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitializeAllSubscription("", "");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(SubscriptionRenewTimeInMinutes), stoppingToken).ConfigureAwait(false);
                _logger.LogWarning("Renewal started.");
                await this.CheckSubscriptions().ConfigureAwait(false); ;
            }
        }

        public override async Task StartAsync(CancellationToken stoppingToken)
        {
            await InitializeAllSubscription("", "");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(SubscriptionRenewTimeInMinutes), stoppingToken).ConfigureAwait(false);
                _logger.LogWarning("Renewal started.");
                await this.CheckSubscriptions().ConfigureAwait(false); ;
            }
        }

        public async Task InitializeAllSubscription(string teamId, string pageId)
        {
            this.pageId = pageId;
            _logger.LogWarning("InitializeAllSubscription-started");

            if (pageId == "1")
            {
                await CreateNewSubscription(teamId);
                await this.CheckSubscriptions().ConfigureAwait(false);
            }
            else
            {
                await CreateNewSubscriptionForTeam(teamId);
                await this.CheckSubscriptionsForTeams().ConfigureAwait(false);
            }
            
            _logger.LogWarning("InitializeAllSubscription-completed");
        }

        /// <summary>
        /// Checking Subsciption for Channel
        /// </summary>
        /// <returns></returns>
        public async Task CheckSubscriptions()
        {
            _logger.LogWarning($"Checking subscriptions {DateTime.UtcNow.ToString("h:mm:ss.fff")}");

            foreach (var subscription in Subscriptions)
            {
                await RenewSubscription(subscription.Value);
            }
        }

        /// <summary>
        /// Checking Subsciption for Teams
        /// </summary>
        /// <returns></returns>
        public async Task CheckSubscriptionsForTeams()
        {
            _logger.LogWarning($"Checking subscriptions {DateTime.UtcNow.ToString("h:mm:ss.fff")}");

            foreach (var subscription in Subscriptions)
            {
                await RenewSubscriptionForTeams(subscription.Value);
            }
        }

        /// <summary>
        /// New Subsciption for using channel resource
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        private async Task<Subscription> CreateNewSubscription(string teamId)
        {
            _logger.LogWarning($"CreateNewSubscription-start: {teamId}");

            if (string.IsNullOrEmpty(teamId))
            return null;

            var resource = $"/teams/{teamId}/channels";
          
            return await CreateSubscriptionWithResource(resource);
        }

        /// <summary>
        /// New Subsciption for Team resource
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        private async Task<Subscription> CreateNewSubscriptionForTeam(string teamId)
        {
            _logger.LogWarning($"CreateNewSubscription-start: {teamId}");

            if (string.IsNullOrEmpty(teamId))
                return null;

            var resource = $"/teams/{teamId}";

            return await CreateSubscriptionWithResourceForTeams(resource);
        }

       /// <summary>
       /// Creating New Subsciption for Channel
       /// </summary>
       /// <param name="resource"></param>
       /// <returns></returns>
        private async Task<Subscription> CreateSubscriptionWithResource(string resource)
        {
            if (string.IsNullOrEmpty(resource))
                return null;

            var graphServiceClient = graphBetaClientProvider.GetGraphClientforApp();

            if (Subscriptions.Any(s => s.Value.Resource == resource && s.Value.ExpirationDateTime < DateTime.UtcNow))
                return null;
            
            IGraphServiceSubscriptionsCollectionPage channelexistingSubscriptions = null;

            try
            {
                channelexistingSubscriptions = await graphServiceClient
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
            var existingSubscriptionForChannel = channelexistingSubscriptions.FirstOrDefault(s => s.Resource == resource);
            if (existingSubscriptionForChannel != null && existingSubscriptionForChannel.NotificationUrl != notificationUrl)
            {
                _logger.LogWarning($"CreateNewSubscription-ExistingSubscriptionFound: {resource}");
                await DeleteSubscription(existingSubscriptionForChannel);
                existingSubscriptionForChannel = null;
            }
           
            if (existingSubscriptionForChannel == null)
            {
                var channelsub = new Subscription
                {
                    Resource = resource,
                    EncryptionCertificate = this.botSettings.Value.Base64EncodedCertificate,
                    EncryptionCertificateId = this.botSettings.Value.EncryptionCertificateId,
                    IncludeResourceData = true,
                    ChangeType = "created,deleted,updated",
                    NotificationUrl = notificationUrl,
                    ClientState = "ClientState",
                    ExpirationDateTime = DateTime.UtcNow + new TimeSpan(days: 0, hours: 0, minutes: SubscriptionExpirationTimeInMinutes, seconds: 0)
                };
                try
                {
                    existingSubscriptionForChannel = await graphServiceClient
                              .Subscriptions
                              .Request()
                              .AddAsync(channelsub);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"CreateNewSubscription-Failed: {resource}");
                    return null;
                }
            }

            Subscriptions[existingSubscriptionForChannel.Id] = existingSubscriptionForChannel;
            _logger.LogWarning($"Subscription Created for TeamId: {resource}");
            return existingSubscriptionForChannel;
        }

        /// <summary>
        /// Creating New Subscription For Teams
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        private async Task<Subscription> CreateSubscriptionWithResourceForTeams(string resource)
        {
            if (string.IsNullOrEmpty(resource))
                return null;

            var graphServiceClient = graphBetaClientProvider.GetGraphClientforApp();

            if (Subscriptions.Any(s => s.Value.Resource == resource && s.Value.ExpirationDateTime < DateTime.UtcNow))
                return null;
           
            IGraphServiceSubscriptionsCollectionPage teamexistingSubscriptions = null;
            try
            {
                teamexistingSubscriptions = await graphServiceClient
                          .Subscriptions
                          .Request().
                          GetAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CreateNewSubscription-ExistingSubscriptions-Failed: {resource}");
                return null;
            }
           
            var notificationUrlteams = this.botSettings.Value.BaseUrl + "/api/team";
            var existingSubscriptionForTeam = teamexistingSubscriptions.FirstOrDefault(s => s.Resource == resource);

            if (existingSubscriptionForTeam != null && existingSubscriptionForTeam.NotificationUrl != notificationUrlteams)
            {
                _logger.LogWarning($"CreateNewSubscription-ExistingSubscriptionFound: {resource}");
                await DeleteSubscription(existingSubscriptionForTeam);
                existingSubscriptionForTeam = null;
            }

            if (existingSubscriptionForTeam == null)
            {
                var sub = new Subscription
                {

                    Resource = resource,
                    EncryptionCertificate = this.botSettings.Value.Base64EncodedCertificate,
                    EncryptionCertificateId = this.botSettings.Value.EncryptionCertificateId,
                    IncludeResourceData = true,
                    ChangeType = "updated",
                    NotificationUrl = notificationUrlteams,
                    ClientState = "ClientState",
                    ExpirationDateTime = DateTime.UtcNow + new TimeSpan(days: 0, hours: 0, minutes: SubscriptionExpirationTimeInMinutes, seconds: 0)
                };
                try
                {
                    existingSubscriptionForTeam = await graphServiceClient
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

            Subscriptions[existingSubscriptionForTeam.Id] = existingSubscriptionForTeam;
            _logger.LogWarning($"Subscription Created for TeamId: {resource}");
            return existingSubscriptionForTeam;
        }

        /// <summary>
        /// Renew Subscription for Channel
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        private async Task RenewSubscription(Subscription subscription)
        {
            _logger.LogWarning($"Current subscription: {subscription.Id}, Expiration: {subscription.ExpirationDateTime}");

            var graphServiceClient = graphBetaClientProvider.GetGraphClientforApp();

            var newSubscription = new Subscription
            {
                ExpirationDateTime = DateTime.UtcNow.AddHours(1)
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
                    //Subscriptions.Remove(subscription.Id);
                    _logger.LogError(ex, $"HttpStatusCode.NotFound : Creating new subscription : {subscription.Id}");
                    // Try and create new resource.

                    await CreateSubscriptionWithResource(subscription.Resource);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Update Subscription Failed: {subscription.Id}");
            }
        }

        /// <summary>
        /// Renew Subscription for teams
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        private async Task RenewSubscriptionForTeams(Subscription subscription)
        {
            _logger.LogWarning($"Current subscription: {subscription.Id}, Expiration: {subscription.ExpirationDateTime}");
            var graphServiceClient = graphBetaClientProvider.GetGraphClientforApp();
            var newSubscription = new Subscription
            {
                ExpirationDateTime = DateTime.UtcNow.AddHours(1)
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
                    //Subscriptions.Remove(subscription.Id);
                    _logger.LogError(ex, $"HttpStatusCode.NotFound : Creating new subscription : {subscription.Id}");
                    // Try and create new resource.

                    await CreateSubscriptionWithResourceForTeams(subscription.Resource);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Update Subscription Failed: {subscription.Id}");
            }
        }

        /// <summary>
        /// Delete existing subscription 
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
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
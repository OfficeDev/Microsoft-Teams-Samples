using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ChangeNotification.Helper
{
    public class SubscriptionManager : BackgroundService
    {
        private const int SubscriptionExpirationTimeInMinutes = 60;
        private const int SubscriptionRenewTimeInMinutes = 15;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly string _token;
        private readonly ITurnContext _turnContext;

        public static readonly Dictionary<string, Subscription> Subscriptions = new Dictionary<string, Subscription>();

        public SubscriptionManager(IConfiguration config, ILogger<SubscriptionManager> logger, string token, ITurnContext turnContext)
        {
            _config = config;
            _logger = logger;
            _token = token;
            _turnContext = turnContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitializeAllSubscription();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(SubscriptionRenewTimeInMinutes), stoppingToken).ConfigureAwait(false);
                _logger.LogWarning("Renewal started.");
                await this.CheckSubscriptions().ConfigureAwait(false); ;
            }
        }

        public override async Task StartAsync(CancellationToken stoppingToken)
        {
            await InitializeAllSubscription();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(SubscriptionRenewTimeInMinutes), stoppingToken).ConfigureAwait(false);
                _logger.LogWarning("Renewal started.");
                await this.CheckSubscriptions().ConfigureAwait(false); ;
            }
        }

        public async Task InitializeAllSubscription()
        {
            _logger.LogWarning("InitializeAllSubscription-started");
            var UserId = _turnContext.Activity.From.AadObjectId;

            await CreateNewSubscription(UserId);

            await this.CheckSubscriptions().ConfigureAwait(false);
            _logger.LogWarning("InitializeAllSubscription-completed");
        }

        private async Task<Subscription> CreateNewSubscription(string userId)
        {
            _logger.LogWarning($"CreateNewSubscription-start: {userId}");

            if (string.IsNullOrEmpty(userId))
                return null;
            var resource = $"communications/presences/{userId}";
            return await CreateSubscriptionWithResource(resource);
        }

        private async Task<Subscription> CreateSubscriptionWithResource(string resource)
        {
            if (string.IsNullOrEmpty(resource))
                return null;

            var graphServiceClient = GetGraphClient();

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

            var notificationUrl = _config["BaseUrl"] + "/api/notifications";

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
                    ChangeType = "updated",
                    NotificationUrl = notificationUrl,
                    ClientState = "ClientState",
                    ExpirationDateTime = DateTime.UtcNow + new TimeSpan(days: 0, hours: 0, minutes: SubscriptionExpirationTimeInMinutes, seconds: 0),
                    LatestSupportedTlsVersion = "v1_2",
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

        public async Task CheckSubscriptions()
        {
            _logger.LogWarning($"Checking subscriptions {DateTime.UtcNow.ToString("h:mm:ss.fff")}");

            //if (Subscriptions.Count != 1)
            if (Subscriptions.Count <= 0)
            {
                _logger.LogWarning($"CheckSubscriptions-Number of subscription={Subscriptions.Count}");
                // Possible failure.
                InitializeAllSubscription().RunSynchronously();
            }

            foreach (var subscription in Subscriptions)
            {
                await RenewSubscription(subscription.Value);
            }
        }

        private async Task RenewSubscription(Subscription subscription)
        {
            _logger.LogWarning($"Current subscription: {subscription.Id}, Expiration: {subscription.ExpirationDateTime}");

            var graphServiceClient = GetGraphClient();

            var newSubscription = new Subscription
            {
                ExpirationDateTime = DateTime.UtcNow.AddMinutes(60)
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

        private async Task DeleteSubscription(Subscription subscription)
        {
            _logger.LogWarning($"Current subscription: {subscription.Id}, Expiration: {subscription.ExpirationDateTime}");

            var graphServiceClient = GetGraphClient();

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

        private GraphServiceClient GetGraphClient()
        {
            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                // get an access token for Graph
                var accessToken = this._token;

                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                return Task.FromResult(0);
            }));

            return graphClient;
        }
    }
}
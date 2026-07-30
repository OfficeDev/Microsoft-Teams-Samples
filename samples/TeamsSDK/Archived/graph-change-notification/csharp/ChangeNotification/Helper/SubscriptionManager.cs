using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;

namespace ChangeNotification.Helper
{
    public class SubscriptionManager
    {
        private const int SubscriptionExpirationTimeInMinutes = 60;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly string _token;
        private readonly string _userId;

        public static readonly Dictionary<string, Subscription> Subscriptions = new Dictionary<string, Subscription>();

        public SubscriptionManager(IConfiguration config, ILogger<SubscriptionManager> logger, string token, string userId)
        {
            _config = config;
            _logger = logger;
            _token = token;
            _userId = userId;
        }

        public async Task InitializeAllSubscription()
        {
            _logger.LogWarning("InitializeAllSubscription-started");

            await CreateNewSubscription(_userId);

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

            SubscriptionCollectionResponse existingSubscriptions = null;
            try
            {
                existingSubscriptions = await graphServiceClient
                          .Subscriptions.GetAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CreateNewSubscription-ExistingSubscriptions-Failed: {resource}");
                return null;
            }

            var notificationUrl = _config["BaseUrl"] + "/api/notifications";

            var existingSubscription = existingSubscriptions.Value.FirstOrDefault(s => s.Resource == resource);
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
                              .Subscriptions.PostAsync(sub);
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

        private async Task DeleteSubscription(Subscription subscription)
        {
            _logger.LogWarning($"Current subscription: {subscription.Id}, Expiration: {subscription.ExpirationDateTime}");

            var graphServiceClient = GetGraphClient();

            try
            {
                await graphServiceClient
                     .Subscriptions[subscription.Id]
                     .DeleteAsync();

                _logger.LogWarning($"Deleted subscription: {subscription.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Delete Subscription Failed: {subscription.Id}");
            }
        }

        public GraphServiceClient GetGraphClient()
        {
            var tokenProvider = new SimpleAccessTokenProvider(_token);
            var authProvider = new BaseBearerTokenAuthenticationProvider(tokenProvider);

            return new GraphServiceClient(authProvider);
        }
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
}
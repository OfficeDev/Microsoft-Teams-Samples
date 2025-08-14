namespace Microsoft.Teams.Apps.Selfhelp.Shared.Services.MicrosoftGraph.GraphHelper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Configuration;

    /// <summary>
    /// The class that represent the helper methods to access Microsoft Graph API.
    /// </summary>
    public class GraphApiHelper : IGraphApiHelper
    {
        /// <summary>
        /// Instance to send logs to the application insights service.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The cache key for user profile picture.
        /// </summary>
        private const string ProfilePictureCacheKey = "_user_profile_picture";

        /// <summary>
        /// The cache duration in minutes.
        /// </summary>
        private readonly double cacheDurationInMinutes;

        /// <summary>
        /// Holds the instance of memory cache which will be used to store and retrieve adaptive card payload.
        /// </summary>
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphApiHelper"/> class.
        /// </summary>
        /// <param name="logger">Entity represents logger object.</param>
        /// <param name="botOptions">Entity represents bot options.</param>
        /// <param name="memoryCache">Entity represents memory chache.</param>
        public GraphApiHelper(
            ILogger<GraphApiHelper> logger,
            IOptions<BotSettings> botOptions,
            IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
            this.logger = logger;
            this.cacheDurationInMinutes = botOptions.Value.ProfileImageCacheDurationInMinutes;
        }

        /// <summary>
        /// Get user profile details from microsoft graph.
        /// </summary>
        /// <param name="token">Microsoft Graph user access token.</param>
        /// <param name="query">The query.</param>
        /// <returns>List of user profile details.</returns>
        public async Task<IEnumerable<User>> SearchAsync(string token, string query)
        {
            var users = new List<User>();
            var graphClient = this.GetGraphServiceClient(token);
            string filter = string.Empty;
            filter = $"startsWith(displayName,'{query}') or startsWith(mail,'{query}')";
            var graphResult = await graphClient
                .Users
                .Request()
                .WithMaxRetry(GraphConstants.MaxRetry)
                .Filter(filter)
                .Select(user => new
                {
                    user.Id,
                    user.DisplayName,
                    user.UserPrincipalName,
                    user.Mail,
                }).Top(25).GetAsync();

            if (graphResult == null)
            {
                return null;
            }

            users = graphResult.OfType<User>().ToList();

            while (graphResult.NextPageRequest != null)
            {
                graphResult = await graphResult.NextPageRequest.GetAsync();
                users?.AddRange(graphResult.OfType<User>() ?? new List<User>());
            }

            return users;
        }

        /// <summary>
        /// Get user by teams and channel.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="UserAadId">The user Aad id.</param>
        /// <returns>Team data.</returns>
        public async Task<List<Team>> GetMyJoinedTeamsDataAsync(string token, string UserAadId)
        {
            var graphClient = this.GetGraphServiceClient(token);

            var collectionPage = await graphClient.Me.JoinedTeams
                .Request()
                .GetAsync();

            var teamsList = collectionPage.OfType<Team>().ToList();
            List<Team> finalteamlist = new List<Team>();
            foreach (var item in teamsList)
            {
                var channelPage = await graphClient.Teams[item.Id].Channels
                                 .Request()
                                 .GetAsync();
                item.Channels = channelPage;
                finalteamlist.Add(item);
            }

            while (collectionPage.NextPageRequest != null)
            {
                collectionPage = await collectionPage.NextPageRequest.GetAsync();
                teamsList?.AddRange(collectionPage.OfType<Team>() ?? new List<Team>());
            }

            return finalteamlist;
        }

        /// <summary>
        /// Fetching public url of user profile photo.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="userAadId">The UserAadId.</param>
        /// <returns>Returns public url of user profile photo.</returns>
        public async Task<string> GetPublicURLForProfilePhoto(string token, string userAadId)
        {
            try
            {
                bool isCacheEntryExists = this.memoryCache.TryGetValue(ProfilePictureCacheKey + userAadId, out string fileName);
                if (!isCacheEntryExists)
                {
                    fileName = userAadId + "_ProfilePhoto.png";
                    string imagePath = Path.Combine(".", "wwwroot", "photos");
                    imagePath = Path.Combine(imagePath, fileName);
                    var graphClient = this.GetGraphServiceClient(token);
                    var stream = await graphClient.Users[userAadId].Photo.Content
                                .Request()
                                .GetAsync();

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                    using (var fileStream = System.IO.File.Create(imagePath))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(fileStream);
                    }

                    this.memoryCache.Set(ProfilePictureCacheKey + userAadId, fileName, TimeSpan.FromMinutes(this.cacheDurationInMinutes == 0 ? 60 : this.cacheDurationInMinutes));
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    return string.Empty;
                }
                else
                {
                    return $"/photos/{fileName}";
                }
            }
            catch
            {
                this.memoryCache.Set(ProfilePictureCacheKey + userAadId, string.Empty, TimeSpan.FromMinutes(this.cacheDurationInMinutes == 0 ? 60 : this.cacheDurationInMinutes));
                throw;
            }
        }

        /// <summary>
        /// Get microsoft graph service client.
        /// </summary>
        /// <param name="accessToken">Token to access MS graph.</param>
        /// <returns>Returns a graph service client object.</returns>
        private GraphServiceClient GetGraphServiceClient(string accessToken)
        {
            return new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        await Task.Run(() =>
                        {
                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                                "Bearer",
                                accessToken);
                        });
                    }));
        }

        /// <summary>
        /// Install the app for user.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="appId">Application id.</param>
        /// <param name="userId">Unique id of user.</param>
        /// <returns>Return the installed app for user.</returns>
        public async Task InstallAppForUserAsync(string token, string appId, string userId)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var userScopeTeamsAppInstallation = new UserScopeTeamsAppInstallation
            {
                AdditionalData = new Dictionary<string, object>()
                {
                    { "teamsApp@odata.bind", $"{GraphConstants.BetaBaseUrl}/appCatalogs/teamsApps/{appId}" },
                },
            };
            try
            {
                var graphClient = this.GetGraphServiceClient(token);
                await graphClient.Users[userId]
                    .Teamwork
                    .InstalledApps
                    .Request()
                    .WithMaxRetry(GraphConstants.MaxRetry)
                    .AddAsync(userScopeTeamsAppInstallation);
            }
            catch (Exception ex)
            {
                var errorMessage = $"FailedToInstallApplicationForUserFormat: {ex.Message}";
                this.logger.LogError(ex, errorMessage);
            }
        }

        /// <summary>
        /// Get added new chat thread id for user.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="userId">Unique id of user.</param>
        /// <param name="appId">Application id.</param>
        /// <returns>Return chat thread id for user.</returns>
        public async Task<string> GetChatThreadIdAsync(string token, string userId, string appId)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            try
            {
                var graphClient = this.GetGraphServiceClient(token);
                var installationId = await this.GetAppInstallationIdForUserAsync(token, appId, userId);

                var chat = await graphClient.Users[userId]
                    .Teamwork
                    .InstalledApps[installationId]
                    .Chat
                    .Request()
                    .WithMaxRetry(GraphConstants.MaxRetry)
                    .GetAsync();

                return chat?.Id;
            }
            catch (Exception ex)
            {
                var errorMessage = $"GetChatThreadIdAsync: {ex.Message}";
                this.logger.LogError(ex, errorMessage);
                return string.Empty;
            }
        }

        /// <summary>
        /// Get the team application installed id.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="teamId">Id of the team.</param>
        /// <param name="appId">Application id.</param>
        /// <returns>Return the team application installed id.</returns>
        public async Task<string> GetteamAppInstalledIdAsync(string token, string teamId, string appId)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (string.IsNullOrWhiteSpace(teamId))
            {
                throw new ArgumentNullException(nameof(teamId));
            }

            try
            {
                var graphClient = this.GetGraphServiceClient(token);

                var teamsAppInstallation = await graphClient.Teams[teamId].InstalledApps[appId]
                                               .Request()
                                               .GetAsync();

                return teamsAppInstallation?.Id;
            }
            catch (Exception ex)
            {
                var errorMessage = $"GetteamAppInstalledIdAsync: {ex.Message}";
                this.logger.LogError(ex, errorMessage);
                return string.Empty;
            }
        }

        /// <summary>
        /// Getting application installation id for user.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="appId">Application id.</param>
        /// <param name="userId">Unique id of user.</param>
        /// <returns>Return application installation id for user.</returns>
        public async Task<string> GetAppInstallationIdForUserAsync(string token, string appId, string userId)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var graphClient = this.GetGraphServiceClient(token);
            var collection = await graphClient.Users[userId]
                .Teamwork
                .InstalledApps
                .Request()
                .Expand("teamsApp")
                .Filter($"teamsApp/id eq '{appId}'")
                .WithMaxRetry(GraphConstants.MaxRetry)
                .GetAsync();

            return collection?.FirstOrDefault().Id;
        }

        /// <summary>
        /// Get the application installation for team.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="appId">Application id.</param>
        /// <param name="teamId">Id of the team.</param>
        /// <returns>Returns the installed app for team.</returns>
        public async Task InstallAppForTeamAsync(string token, string appId, string teamId)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (string.IsNullOrWhiteSpace(teamId))
            {
                throw new ArgumentNullException(nameof(teamId));
            }

            var teamsAppInstallation = new TeamsAppInstallation
            {
                AdditionalData = new Dictionary<string, object>()
                {
                    { "teamsApp@odata.bind", $"{GraphConstants.V1BaseUrl}/appCatalogs/teamsApps/{appId}" },
                },
            };
            try
            {
                var graphClient = this.GetGraphServiceClient(token);
                await graphClient.Teams[teamId]
                    .InstalledApps
                    .Request()
                    .WithMaxRetry(GraphConstants.MaxRetry)
                    .AddAsync(teamsAppInstallation);
            }
            catch (Exception ex)
            {
                var errorMessage = $"InstallAppForTeamAsync: {ex.Message}";
                this.logger.LogError(ex, errorMessage);
            }
        }

        /// <summary>
        /// Get an external application id.
        /// </summary>
        /// <param name="token">Microsoft Graph application access token.</param>
        /// <param name="appId">Application id.</param>
        /// <returns>Return the external application id.</returns>
        public async Task<string?> GetExternalAppIdAsync(string token, string appId)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            try
            {
                var graphClient = this.GetGraphServiceClient(token);
                var teamsApps = await graphClient.AppCatalogs.TeamsApps.Request().Filter($"externalId eq '{appId}'").GetAsync();
                return teamsApps?.FirstOrDefault().Id;
            }
            catch (Exception ex)
            {
                var errorMessage = $"FailedToInstallApplicationForUserFormat: {ex.Message}";
                this.logger.LogError(ex, errorMessage);
                return string.Empty;
            }
        }
    }
}
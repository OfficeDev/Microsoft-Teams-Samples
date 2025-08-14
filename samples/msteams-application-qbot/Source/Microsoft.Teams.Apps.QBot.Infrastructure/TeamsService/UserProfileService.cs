namespace Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using Microsoft.Identity.Web;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.IServices;
    using ErrorCode = Microsoft.Teams.Apps.QBot.Domain.Errors.ErrorCode;
    using User = Microsoft.Teams.Apps.QBot.Domain.Models.User;

    /// <summary>
    /// Service to fetch user profile.
    /// </summary>
    internal sealed class UserProfileService : IUserProfileService
    {
        private const int MaxRetry = 3;

        private readonly IGraphServiceClient graphServiceClient;
        private readonly IUserProfilePictureCache cache;
        private readonly ILogger<UserProfileService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileService"/> class.
        /// </summary>
        /// <param name="graphServiceClient">Graph client.</param>
        /// <param name="memoryCache">Memory cache.</param>
        /// <param name="logger">Logger.</param>
        public UserProfileService(
            GraphServiceClient graphServiceClient,
            IUserProfilePictureCache memoryCache,
            ILogger<UserProfileService> logger)
        {
            this.graphServiceClient = graphServiceClient ?? throw new ArgumentNullException(nameof(graphServiceClient));
            this.cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<User> GetUserProfileAsync(string userId, bool fetchPhoto)
        {
            try
            {
                var profile = await this.graphServiceClient
                    .Users[userId]
                    .Request()
                    .WithAppOnly()
                    .WithMaxRetry(MaxRetry)
                    .GetAsync();

                var user = new User()
                {
                    AadId = profile.Id,
                    Name = profile.DisplayName,
                    Upn = profile.UserPrincipalName,
                };

                if (fetchPhoto && string.IsNullOrEmpty(user.ProfilePicUrl))
                {
                    user.ProfilePicUrl = await this.GetUserProfilePicAsync(userId);
                }

                return user;
            }
            catch (ServiceException exception)
            {
                // No permission granted.
                if (exception.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new QBotException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, "Make sure the app has permissions to read user profile.", exception);
                }

                // User not found
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new QBotException(HttpStatusCode.NotFound, ErrorCode.UserNotFound, $"User {userId} not found.", exception);
                }

                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, "Something went wrong.", exception);
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetUserProfilePicAsync(string userId)
        {
            // Read from cache.
            var profilePic = this.cache.ReadFromCache(userId);
            if (profilePic != null)
            {
                return profilePic;
            }

            try
            {
                var stream = await this.graphServiceClient
                    .Users[userId]
                    .Photos["48x48"]
                    .Content
                    .Request()
                    .WithAppOnly()
                    .WithMaxRetry(MaxRetry)
                    .GetAsync();

                if (stream == null)
                {
                    this.logger.LogWarning($"Invalid data stream for user: {userId}");
                    return null;
                }

                // convert to base64
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
                profilePic = Convert.ToBase64String(bytes);

                // Write to cache.
                this.cache.WriteToCache(userId, profilePic);

                return profilePic;
            }
            catch (ServiceException exception)
            {
                // No profile pic set.
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    this.logger.LogWarning(exception, $"No profile pic found for user: {userId}");
                    return null;
                }

                // No permission granted.
                if (exception.StatusCode == HttpStatusCode.Forbidden)
                {
                    this.logger.LogWarning(exception, "Make sure the app has permissions to read user profile");
                    throw new QBotException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, "Make sure the app has permissions to read user profile", exception);
                }

                this.logger.LogWarning(exception, $"Someting went wrong. Failed to fetch user profile pic: {userId}");
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, "Something went wrong.", exception);
            }
        }
    }
}

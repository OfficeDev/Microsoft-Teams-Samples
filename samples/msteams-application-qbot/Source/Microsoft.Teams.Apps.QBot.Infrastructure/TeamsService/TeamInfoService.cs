namespace Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using Microsoft.Identity.Web;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.IServices;
    using ErrorCode = Microsoft.Teams.Apps.QBot.Domain.Errors.ErrorCode;

    /// <summary>
    /// Teams info service implementation.
    /// </summary>
    internal class TeamInfoService : ITeamInfoService
    {
        private const int MaxRetry = 3;

        private readonly IGraphServiceClient graphServiceClient;
        private readonly ITeamPhotoCache cache;
        private readonly ILogger<TeamInfoService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamInfoService"/> class.
        /// </summary>
        /// <param name="graphServiceClient">Graph service client.</param>
        /// <param name="memoryCache">Memory cache.</param>
        /// <param name="logger">Logger.</param>
        public TeamInfoService(
            GraphServiceClient graphServiceClient,
            ITeamPhotoCache memoryCache,
            ILogger<TeamInfoService> logger)
        {
            this.graphServiceClient = graphServiceClient ?? throw new ArgumentNullException(nameof(graphServiceClient));
            this.cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetTeamOwnersIdsAsync(string teamAadId)
        {
            if (string.IsNullOrEmpty(teamAadId))
            {
                throw new ArgumentException($"'{nameof(teamAadId)}' cannot be null or empty.", nameof(teamAadId));
            }

            try
            {
                var result = await this.graphServiceClient
                    .Teams[teamAadId]
                    .Members
                    .Request()
                    .WithAppOnly()
                    .WithMaxRetry(MaxRetry)
                    .GetAsync();

                List<string> members = result.CurrentPage
                    .Where(m => m.Roles != null && m.Roles.Contains("owner"))
                    .Select(m => ((AadUserConversationMember)m).UserId)
                    .ToList();

                while (result.NextPageRequest != null)
                {
                    result = await result.NextPageRequest
                        .WithAppOnly()
                        .GetAsync();
                    members.AddRange(result.CurrentPage
                        .Where(m => m.Roles != null && m.Roles.Contains("owner"))
                        .Select(m => ((AadUserConversationMember)m).UserId));
                }

                return members;
            }
            catch (ServiceException exception)
            {
                // No permission granted.
                if (exception.StatusCode == HttpStatusCode.Forbidden)
                {
                    this.logger.LogWarning(exception, "Make sure the app has permissions to read team info. Add RSC Permission 'TeamMember.Read.Group' to app manifest.");
                    throw new QBotException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, $"{nameof(this.GetTeamOwnersIdsAsync)}: Make sure the app has permissions to read team info. Add RSC Permission 'TeamMember.Read.Group' to app manifest.", exception);
                }

                // Resource not found
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    // Team not found.
                    this.logger.LogWarning(exception, $"Team: {teamAadId} not found.");
                    throw new QBotException(HttpStatusCode.NotFound, ErrorCode.CourseNotFound, $"{nameof(this.GetTeamOwnersIdsAsync)}: Team not found.", exception);
                }

                this.logger.LogError(exception, $"Failed to fetch team member information. StatusCode: {exception.StatusCode}, Exception: {exception.Message}");
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, $"{nameof(this.GetTeamOwnersIdsAsync)}:Unknown error.", exception);
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetTeamPhotoAsync(string teamAadId)
        {
            var profilePic = this.cache.ReadFromCache(teamAadId);
            if (!string.IsNullOrEmpty(profilePic))
            {
                return profilePic;
            }

            try
            {
                var stream = await this.graphServiceClient
                    .Teams[teamAadId]
                    .Photo // Note: Graph Beta doesn't have option to specificy photo size for a course.
                    .Content
                    .Request()
                    .WithAppOnly()
                    .WithMaxRetry(MaxRetry)
                    .GetAsync();

                if (stream == null)
                {
                    this.logger.LogWarning($"Invalid data stream for team: {teamAadId}");
                    return null;
                }

                // Update cache.
                var dataUri = this.GetBase64EncodedPhoto(stream);
                this.cache.WriteToCache(teamAadId, dataUri);
                return dataUri;
            }
            catch (ServiceException exception)
            {
                // No profile pic set.
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    this.logger.LogWarning(exception, $"No team photo found for team: {teamAadId}");
                    return null;
                }

                // No permission granted.
                if (exception.StatusCode == HttpStatusCode.Forbidden)
                {
                    this.logger.LogWarning(exception, "Make sure the app has permission to read team photo. Add RSC Permission 'TeamSettings.Read.Group' to the app manifest.");
                    throw new QBotException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, "Make sure the app has permissions to read team photo", exception);
                }

                this.logger.LogWarning(exception, $"Someting went wrong. Failed to fetch team photo : {teamAadId}");
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, "Something went wrong.", exception);
            }
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, string>> GetTeamsPhotoAsync(IEnumerable<string> teamAadIds)
        {
            if (teamAadIds is null)
            {
                throw new ArgumentNullException(nameof(teamAadIds));
            }

            // Read from cache.
            var response = this.ReadFromCache(teamAadIds);

            // Items not found in cache.
            var teamIds = teamAadIds.Except(response.Keys).ToArray();
            var index = 0;
            while (index < teamIds.Length)
            {
                // Prepare batch request content. (max 20 requests).
                var batchRequestContent = new BatchRequestContent();
                var requestIdMap = new Dictionary<string, string>();
                for (int j = 0; j < 20 && index < teamIds.Length; j++, index++)
                {
                    var teamId = teamIds[index];
                    var request = this.graphServiceClient
                        .Teams[teamId]
                        .Photo
                        .Content
                        .Request()
                        .WithAppOnly()
                        .WithMaxRetry(MaxRetry)
                        .GetHttpRequestMessage();

                    request.Method = HttpMethod.Get;
                    var requestId = batchRequestContent.AddBatchRequestStep(request);
                    requestIdMap.Add(teamId, requestId);
                }

                BatchResponseContent result;
                try
                {
                    result = await this.graphServiceClient
                        .Batch
                        .Request()
                        .WithAppOnly()
                        .WithMaxRetry(MaxRetry)
                        .PostAsync(batchRequestContent);
                }
                catch (ServiceException exception)
                {
                    this.logger.LogWarning(exception, $"Something went wrong. Failed to send batch request. StatusCode: {exception.StatusCode}");
                    throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, "Failed to send batch request to fetch team photo.", exception);
                }

                foreach (var teamId in requestIdMap.Keys)
                {
                    try
                    {
                        using (var httpResponse = await result.GetResponseByIdAsync(requestIdMap.GetValueOrDefault(teamId)))
                        {
                            // Move to next if the request failed.
                            if (!httpResponse.IsSuccessStatusCode)
                            {
                                this.logger.LogWarning($"Failed to fetch team photo for team: {teamId}. Status Code: {httpResponse.StatusCode} Message: {httpResponse.ReasonPhrase} ");
                                continue;
                            }

                            // Read image data.
                            var dataUri = await httpResponse.Content.ReadAsStringAsync();
                            response.Add(teamId, dataUri);

                            // Update cache
                            this.cache.WriteToCache(teamId, dataUri);
                        }
                    }
                    catch (ServiceException exception)
                    {
                        // No permission granted.
                        if (exception.StatusCode == HttpStatusCode.Forbidden)
                        {
                            this.logger.LogWarning(exception, "Make sure the app has permissions to read channel messages. Add RSC Permission 'ChannelMessage.Read.Group' to app manifest.");
                            throw new QBotException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, $"{nameof(this.GetTeamsPhotoAsync)}: Make sure the app has permissions to read team photo. Add RSC Permission 'ChannelMessage.Read.Group' to app manifest.", exception);
                        }

                        // Resource not found
                        if (exception.StatusCode == HttpStatusCode.NotFound)
                        {
                            // Message not found.
                            this.logger.LogWarning(exception, $"Team photo not found. Team Id: {teamId}");
                            response.Add(teamId, null);
                        }

                        this.logger.LogWarning(exception, $"Someting went wrong. Failed to fetch team photo: {teamId}");
                        throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, $"Something went wrong.", exception);
                    }
                }
            }

            return response;
        }

        private Dictionary<string, string> ReadFromCache(IEnumerable<string> teamAadIds)
        {
            var response = new Dictionary<string, string>();
            foreach (var teamId in teamAadIds)
            {
                var profilePic = this.cache.ReadFromCache(teamId);
                if (!string.IsNullOrEmpty(profilePic))
                {
                    response.Add(teamId, profilePic);
                }
            }

            return response;
        }

        private string GetBase64EncodedPhoto(Stream stream)
        {
            // convert to base64
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            return Convert.ToBase64String(bytes);
        }
    }
}

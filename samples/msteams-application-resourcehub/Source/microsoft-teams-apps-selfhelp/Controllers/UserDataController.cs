namespace Microsoft.Teams.Selfhelp.Authentication.Controllers
{
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Selfhelp.Helper;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Configuration;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.AppConfig;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.TeamRepository;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.UserRepository;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Services.MicrosoftGraph.GraphHelper;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Services.MicrosoftGraph.Token;
    using Microsoft.Teams.Selfhelp.Authentication.Model;

    /// <summary>
    /// Controller for the user data.
    /// </summary>
    [Authorize]
    [Route("api/UserData")]
    [ApiController]
    public class UserDataController : BaseController
    {
        /// <summary>
        /// Instance to send logs to the application insights service.
        /// </summary>
        private readonly ILogger<UserDataController> logger;

        /// <summary>
        /// The client id details.
        /// </summary>
        private readonly string clientId;

        /// <summary>
        /// User secrete details.
        /// </summary>
        private readonly string userSecret;

        /// <summary>
        /// The unique id of tenant.
        /// </summary>
        private readonly string tenantId;

        /// <summary>
        /// User principal name details.
        /// </summary>
        private readonly string UPN;

        /// <summary>
        /// Instance of the token helper details.
        /// </summary>
        private readonly ITokenHelper tokenHelper;

        /// <summary>
        /// Instance of microsoft graph api helper.
        /// </summary>
        private readonly IGraphApiHelper graphApiHelper;

        /// <summary>
        /// Instance of user data repository details.
        /// </summary>
        private readonly IUserRepository userDataRepository;

        /// <summary>
        /// Instance of application repository details.
        /// </summary>
        private readonly IAppConfigRepository appConfigRepository;

        /// <summary>
        /// Instance of message services.
        /// </summary>
        private readonly IMessageService messageService;

        /// <summary>
        /// Instance of team repository details.
        /// </summary>
        private readonly ITeamRepository teamDataRepository;

        /// <summary>
        /// Holds the information about the web hosting environment an application is running in.
        /// </summary>
        private readonly IWebHostEnvironment webHostEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDataController"/> class.
        /// </summary>
        /// <param name="graphApiHelper">Entity represent graph api helper.</param>
        /// <param name="loggerFactory">Entity represent logger factory details.</param>
        /// <param name="tokenHelper">Entity represent token helper.</param>
        /// <param name="userDataRepository">Entity represent user data repository details.</param>
        /// <param name="botOptions">Entity represent bot options.</param>
        /// <param name="webHostEnvironment">Entity represent web hosting environment details.</param>
        /// <param name="telemetryClient">Entity represent application insights telemetry client.</param>
        /// <param name="appConfigRepository">Entity represent application configuration repository details.</param>
        /// <param name="messageService">Entity represent message services.</param>
        /// <param name="teamDataRepository">Entity represent team data repository details.</param>
        public UserDataController(
            IGraphApiHelper graphApiHelper,
            ILoggerFactory loggerFactory,
            ITokenHelper tokenHelper,
            IUserRepository userDataRepository,
            IOptions<BotSettings> botOptions,
            IWebHostEnvironment webHostEnvironment,
            TelemetryClient telemetryClient, IAppConfigRepository appConfigRepository, IMessageService messageService, ITeamRepository teamDataRepository)
            : base(telemetryClient)
        {
            this.graphApiHelper = graphApiHelper ?? throw new ArgumentNullException(nameof(graphApiHelper));
            this.logger = loggerFactory?.CreateLogger<UserDataController>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
            this.clientId = botOptions?.Value?.MicrosoftAppId ?? throw new ArgumentNullException(nameof(botOptions));
            this.userSecret = botOptions?.Value?.MicrosoftAppPassword ?? throw new ArgumentNullException(nameof(botOptions));
            this.tenantId = botOptions?.Value?.TenantId ?? throw new ArgumentNullException(nameof(botOptions));
            this.UPN = botOptions?.Value?.UPN ?? throw new ArgumentNullException(nameof(botOptions));
            this.userDataRepository = userDataRepository;
            this.appConfigRepository = appConfigRepository;
            this.messageService = messageService;
            this.webHostEnvironment = webHostEnvironment;
            this.teamDataRepository = teamDataRepository;
        }

        /// <summary>
        /// Get data for all teams.
        /// </summary>
        /// <param name="query">The query data.</param>
        /// <returns>A list of team member data.</returns>
        [HttpGet("allusers")]
        [Authorize]
        public async Task<IEnumerable<UserData>> SearchAsync(string query)
        {
            try
            {
                this.Request.Headers.TryGetValue("Authorization", out var encodedToken);
                var users = new List<UserData>();
                var delegatedToken = await this.tokenHelper.ObtainDelegatedGraphTokenAsync(encodedToken);
                var response = await this.graphApiHelper.SearchAsync(delegatedToken, query);
                foreach (var item in response)
                {
                    var user = new UserData()
                    {
                        UserId = item.Id,
                        DisplayName = item.DisplayName,
                        Email = item.Mail,
                        Image = await this.GetUserPictureUrl(delegatedToken, item.Id),
                    };
                    users.Add(user);
                }

                return users;
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error occurred in method GetTeamMembersDataAsync", ex);
                throw;
            }
        }

        /// <summary>
        /// Get data for all teams.
        /// </summary>
        /// <returns>A list of team member data.</returns>
        [HttpGet("allteams")]
        [Authorize]
        public async Task<IEnumerable<TeamData>> GetAllTeamDataAsync()
        {
            try
            {
                var teams = new List<TeamData>();
                this.Request.Headers.TryGetValue("Authorization", out var encodedToken);
                var delegatedToken = await this.tokenHelper.ObtainDelegatedGraphTokenAsync(encodedToken);
                var response = await this.graphApiHelper.GetMyJoinedTeamsDataAsync(delegatedToken, this.UserAadId);
                foreach (var item in response)
                {
                    if (item.Channels.Count > 1)
                    {
                        foreach (var channelitem in item.Channels)
                        {
                            var team = new TeamData()
                            {
                                teamId = item.Id,
                                teamName = item.DisplayName,
                                channelId = channelitem.Id,
                                channelName = channelitem.DisplayName,
                            };
                            teams.Add(team);
                        }
                    }
                    else
                    {
                        var team = new TeamData()
                        {
                            teamId = item.Id,
                            teamName = item.DisplayName,
                            channelId = item.Channels.CurrentPage[0].Id,
                            channelName = item.Channels.CurrentPage[0].DisplayName,
                        };
                        teams.Add(team);
                    }
                }

                return teams;
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error occurred in method GetAllTeamDataAsync", ex);
                throw;
            }
        }

        /// <summary>
        /// Send notification to the user.
        /// </summary>
        /// <param name="learningEntity">Learning entity details.</param>
        /// <param name="title">Title of content.</param>
        /// <returns>Returns true if details inserted successfully,Else returns false.</returns> 
        [HttpPost("SendNotification")]
        [Authorize]
        public async Task<bool> SendNotificationToUsers([FromBody] List<ArticleCheckBoxEntity> learningEntity, [FromQuery] string title)
        {
            try
            {
                this.Request.Headers.TryGetValue("Authorization", out var encodedToken);
                var delegatedToken = await this.tokenHelper.ObtainDelegatedGraphTokenAsync(encodedToken);
                var userList = await this.userDataRepository.GetAllUserConversationAsync();
                var userDataList = userList.Where(x => x.Status == false);
                var serviceUrl = await this.appConfigRepository.GetServiceUrlAsync();
                var userConversationDetails = string.Empty;
                foreach (var item in userDataList)
                {
                    var cardAttachment = await this.messageService.SendNotificationtoUsersCard(title, learningEntity, this.webHostEnvironment.ContentRootPath, this.clientId);
                    var messageActivity = cardAttachment;
                    await this.messageService.SendMessageAsync(MessageFactory.Attachment(messageActivity), item.ConversationId, serviceUrl, 2, this.logger);
                }

                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error occurred in method SendNotificationToUser", ex);
                throw;
            }
        }

        /// <summary>
        /// Verify admin role.
        /// </summary>
        /// <returns>True if user is admin role.</returns>
        [HttpGet]
        [Route("userrole")]
        public async Task<bool?> GetUserAppId([FromQuery] string upn)
        {
            try
            {
                var groupMembers = this.UPN.Split(',');
                if (groupMembers == null || groupMembers.Count() == 0)
                {
                    return false;
                }

                var res = groupMembers?.Contains(upn, StringComparer.OrdinalIgnoreCase);
                return res;
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error occurred in method GetUserAppId", ex);
                throw;
            }
        }

        /// <summary>
        /// Get the user picture url.
        /// </summary>
        /// <param name="token">Token details.</param>
        /// <param name="userId">Unique user id.</param>
        /// <returns>Return the user picture url.</returns>
        private async Task<string> GetUserPictureUrl(string token, string userId)
        {
            try
            {
                string pictureUrl = await this.graphApiHelper.GetPublicURLForProfilePhoto(token, userId);

                return pictureUrl;
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error occurred in method GetUserPictureUrl", ex);
                return null;
            }
        }
    }
}
namespace Microsoft.Teams.Selfhelp.Authentication.Controllers
{
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.SelfHelp.AdaptiveCard.Services;
    using Microsoft.Teams.Apps.Selfhelp.Helper;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Configuration;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.AppConfig;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.TeamRepository;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.UserRepository;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Services.MicrosoftGraph.GraphHelper;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Services.MicrosoftGraph.Token;
    using Microsoft.Teams.Selfhelp.Models.Configuration;
    using Newtonsoft.Json;

    /// <summary>
    /// Controller for share the article.
    /// </summary>
    [Authorize]
    [Route("api/sharearticle")]
    public class ShareArticleContoller : BaseController
    {
        /// <summary>
        /// Instance of the logger details.
        /// </summary>
        private readonly ILogger<ShareArticleContoller> logger;

        /// <summary>
        /// The unique client id.
        /// </summary>
        private readonly string clientId;

        /// <summary>
        /// User secrete details.
        /// </summary>
        private readonly string userSecret;

        /// <summary>
        /// The unique tenant id.
        /// </summary>
        private readonly string tenantId;

        /// <summary>
        /// Instance of the token helper details.
        /// </summary>
        private readonly ITokenHelper tokenHelper;

        /// <summary>
        /// Instance of the adaptive card helper details.
        /// </summary>
        private readonly IAdaptiveCardService AdaptiveCardHelper;

        /// <summary>
        /// Instance of message services.
        /// </summary>
        private readonly IMessageService messageService;

        /// <summary>
        /// Instance of application repository details.
        /// </summary>
        private readonly IAppConfigRepository appConfigRepository;

        /// <summary>
        /// Instance of microsoft graph api helper.
        /// </summary>
        private readonly IGraphApiHelper graphApiHelper;

        /// <summary>
        /// Instance of user data repository details.
        /// </summary>
        private readonly IUserRepository userDataRepository;

        /// <summary>
        /// Instance of team repository details.
        /// </summary>
        private readonly ITeamRepository teamRepository;

        /// <summary>
        /// Holds the information about the web hosting environment an application is running in.
        /// </summary>
        private readonly IWebHostEnvironment webHostEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShareArticleContoller"/> class.
        /// </summary>
        /// <param name="graphApiHelper">Entity represent graph api helper.</param>
        /// <param name="loggerFactory">Entity represent logger factory details.</param>
        /// <param name="tokenHelper">Entity represent the token helper.</param>
        /// <param name="botOptions">Entity represent the bot options.</param>
        /// <param name="appConfigRepository">Entity application configuration repository details.</param>
        /// <param name="telemetryClient">Entity represent application insights telemetry client.</param>
        /// <param name="webHostEnvironment">Entity represent web hosting environment.</param>
        /// <param name="messageService">Entity represent message services.</param>
        /// <param name="AdaptiveCardHelper">Entity represent adaptive card helper.</param>
        /// <param name="userDataRepository">Entity represent user data repository deatils.</param>
        /// <param name="teamRepository">Entity represent team repository details..</param>
        public ShareArticleContoller(
           IGraphApiHelper graphApiHelper,
           ILoggerFactory loggerFactory,
           ITokenHelper tokenHelper,
           IOptions<BotSettings> botOptions,
           IAppConfigRepository appConfigRepository,
           TelemetryClient telemetryClient,
           IWebHostEnvironment webHostEnvironment, IMessageService messageService, IAdaptiveCardService AdaptiveCardHelper, IUserRepository userDataRepository, ITeamRepository teamRepository)
            : base(telemetryClient)
        {
            this.logger = loggerFactory?.CreateLogger<ShareArticleContoller>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.AdaptiveCardHelper = AdaptiveCardHelper ?? throw new ArgumentNullException(nameof(AdaptiveCardHelper));
            this.tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
            this.clientId = botOptions?.Value?.MicrosoftAppId ?? throw new ArgumentNullException(nameof(botOptions));
            this.userSecret = botOptions?.Value?.MicrosoftAppPassword ?? throw new ArgumentNullException(nameof(botOptions));
            this.tenantId = botOptions?.Value?.TenantId ?? throw new ArgumentNullException(nameof(botOptions));
            this.webHostEnvironment = webHostEnvironment;
            this.messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            this.appConfigRepository = appConfigRepository;
            this.graphApiHelper = graphApiHelper;
            this.userDataRepository = userDataRepository;
            this.teamRepository = teamRepository;
        }

        /// <summary>
        /// Share article learning content.
        /// </summary>
        /// <param name="entity">Details of the feedback to be created.</param>
        /// <returns>Returns true if task is created else return false.</returns>
        [HttpPost]
        public async Task<IActionResult> ShareArticleAsync([FromBody] ShareArticleRequest entity)
        {
            this.RecordEvent("ShareArticleAsync- The HTTP POST call to create task has been initiated.", RequestType.Initiated);
            if (entity == null)
            {
                this.logger.LogError("ShareArticleAsync Entitydetail is null.");
                this.RecordEvent("ShareArticleAsync - The HTTP POST call to create task has has failed.", RequestType.Failed);
                return this.BadRequest();
            }

            this.Request.Headers.TryGetValue("Authorization", out var encodedToken);
            var token = await this.tokenHelper.ObtainApplicationTokenAsync(this.UserTenantId, this.clientId, this.userSecret);

            try
            {
                string cardMessage = entity.Message;
                if (entity.IsShareToUser)
                {
                    var userAadIds = JsonConvert.DeserializeObject<List<string>>(entity.Users);
                    var serviceUrl = await this.appConfigRepository.GetServiceUrlAsync();
                    var externalAppId = await graphApiHelper.GetExternalAppIdAsync(token.AccessToken, this.clientId);
                    foreach (var item in userAadIds)
                    {
                        var userDetails = await this.userDataRepository.GetUserByUserIdAsync(item);
                        var conversationId = "";
                        if (userDetails.Count() == 0)
                        {
                            conversationId = await this.AdaptiveCardHelper.InstallAppAndGetConversationId(entity.LearningId, item, token.AccessToken, this.webHostEnvironment.ContentRootPath, externalAppId);
                        }
                        else
                        {
                            conversationId = userDetails.FirstOrDefault().ConversationId;
                        }

                        var cardAttachment = await this.messageService.GetUserAdaptiveCard(cardMessage, entity.LearningId, this.webHostEnvironment.ContentRootPath, this.UserName);

                        // Send message.
                        var messageActivity = cardAttachment;
                        var response = await this.messageService.SendMessageAsync(
                            message: MessageFactory.Attachment(messageActivity),
                            serviceUrl: serviceUrl,
                            conversationId: conversationId,
                            maxAttempts: 2,
                            logger: this.logger);
                    }
                }
                else
                {
                    var teamIds = JsonConvert.DeserializeObject<List<string>>(entity.TeamId);
                    var channelIds = JsonConvert.DeserializeObject<List<string>>(entity.ChannelId);
                    var externalAppId = await graphApiHelper.GetExternalAppIdAsync(token.AccessToken, this.clientId);
                    var serviceUrl = await this.appConfigRepository.GetServiceUrlAsync();
                    foreach (var item in channelIds)
                    {
                        var teamDetails = await this.teamRepository.GetTeamByChannelIdAsync(item);
                        var channelId = item;
                        if (teamDetails.Count() == 0)
                        {
                            await this.AdaptiveCardHelper.SendTeamsAssignemntTaskNotification(entity.LearningId, teamIds.FirstOrDefault(), token.AccessToken, this.webHostEnvironment.ContentRootPath, externalAppId);
                        }
                        else
                        {
                            channelId = teamDetails.FirstOrDefault().ChannelId;
                        }

                        var cardAttachment = await this.messageService.GetUserAdaptiveCard(cardMessage, entity.LearningId, this.webHostEnvironment.ContentRootPath, this.UserName);

                        // Send message.
                        var messageActivity = cardAttachment;
                        var response = await this.messageService.SendMessageAsync(
                            message: MessageFactory.Attachment(messageActivity),
                            serviceUrl: serviceUrl,
                            conversationId: channelId,
                            maxAttempts: 2,
                            logger: this.logger);
                    }
                }

                return this.Ok();
            }
            catch (Exception ex)
            {
                this.RecordEvent("ShareArticleAsync- The HTTP POST call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while ShareArticleAsync.");
                throw;
            }
        }
    }
}
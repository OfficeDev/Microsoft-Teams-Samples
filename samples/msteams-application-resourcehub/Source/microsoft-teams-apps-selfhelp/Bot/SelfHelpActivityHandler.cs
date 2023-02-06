namespace Microsoft.Teams.Selfhelp.Bot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Selfhelp.Helper;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.AppConfig;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.TeamRepository;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.UserRepository;
    using Microsoft.Teams.Selfhelp.Authentication.Model;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class is responsible for reacting to incoming events from Microsoft Teams sent from BotFramework.
    /// </summary>
    public class SelfHelpActivityHandler : TeamsActivityHandler
    {
        /// <summary>
        /// A set of key/value application configuration properties for bot settings.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Instance to send logs to the logger service.
        /// </summary>
        private readonly ILogger<SelfHelpActivityHandler> logger;

        /// <summary>
        /// Instance of Application Insights Telemetry client.
        /// </summary>
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Instance of user repository details.
        /// </summary>
        private readonly IUserRepository userDataRepository;

        /// <summary>
        /// Instance of Application configuration repository details.
        /// </summary>
        private readonly IAppConfigRepository appConfigRepository;

        /// <summary>
        /// Instance of team repository details.
        /// </summary>
        private readonly ITeamRepository teamRepository;

        /// <summary>
        /// Instance of message services extension.
        /// </summary>
        private readonly IMessageService messageService;

        /// <summary>
        /// Instance of web hosting environment details.
        /// </summary>
        private readonly IWebHostEnvironment webHostEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfHelpActivityHandler"/> class.
        /// </summary>
        /// <param name="logger">Instance to send logs to the logger service.</param>
        /// <param name="telemetryClient">The Application Insights telemetry client.</param>
        /// <param name="configuration">A set of key/value application configuration properties for activity handler.</param>
        public SelfHelpActivityHandler(
            ILogger<SelfHelpActivityHandler> logger,
            TelemetryClient telemetryClient,
            IConfiguration configuration,
            IAppConfigRepository appConfigRepository,
            IUserRepository userDataRepository, ITeamRepository teamRepository, IMessageService messageService, IWebHostEnvironment webHostEnvironment)
        {
            this.logger = logger;
            this.telemetryClient = telemetryClient;
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.userDataRepository = userDataRepository;
            this.appConfigRepository = appConfigRepository;
            this.teamRepository = teamRepository;
            this.messageService = messageService;
            this.webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Handles an incoming activity.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// Reference link: https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.activityhandler.onturnasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        public override Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            this.RecordEvent(nameof(this.OnTurnAsync), turnContext);
            return base.OnTurnAsync(turnContext, cancellationToken);
        }

        /// <summary>
        /// Invoked method when member is submitting adaptive cards and opening task module and feching a task module.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents a response.</returns>
        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var activityValue = JObject.FromObject(taskModuleRequest.Data);
            var adaptiveCardData = activityValue?.ToObject<ShareArticleData>();
            var valuesFromCard = JsonConvert.DeserializeObject<ShareArticleData>(taskModuleRequest.Data.ToString());

            if (valuesFromCard.ItemType == ItemType.Video)
            {
                return Task.FromResult(
                        new TaskModuleResponse
                        {
                            Task = new TaskModuleContinueResponse
                            {
                                Value = new TaskModuleTaskInfo()
                                {
                                    Url = this.configuration.GetValue<string>("App:AppBaseUri") + "/view-video-content?id=" + valuesFromCard.LearningId + "&status" + true,
                                    Height = 600,
                                    Width = 600,
                                    Title = "Video article",
                                },
                            },
                        }
                     );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Invoked when members other than this bot (like a user) are removed from the conversation.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            this.RecordEvent(nameof(this.OnConversationUpdateActivityAsync), turnContext);

            var activity = turnContext.Activity;
            this.logger.LogInformation($"conversationType: {activity.Conversation.ConversationType}, membersAdded: {activity.MembersAdded?.Count}, membersRemoved: {activity.MembersRemoved?.Count}");

            if (activity.Conversation.ConversationType == "personal")
            {
                if (activity.MembersAdded != null && activity.MembersAdded.Any(member => member.Id != activity.Recipient.Id))
                {
                    await this.HandleMemberAddedInPersonalScopeAsync(turnContext, cancellationToken);
                }
            }
            if (activity.Conversation.ConversationType == "channel")
            {
                if (activity.MembersAdded != null && activity.MembersAdded.Any(member => member.Id == activity.Recipient.Id))
                {
                    await this.HandleMemberAddedInTeamAsync(turnContext, cancellationToken);
                }
            }
            else if (activity.MembersRemoved != null && activity.MembersRemoved.Any(member => member.Id == activity.Recipient.Id))
            {
                this.HandleMemberRemovedInTeamScopeAsync(turnContext);
            }
        }

        /// <summary>
        /// When OnTurn method receives a submit invoke activity on bot turn, it calls this method.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn of a bot.</param>
        /// <param name="taskModuleRequest">Task module invoke request value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents a task module response.</returns>
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var activityValue = JObject.FromObject(taskModuleRequest.Data);
            var adaptiveCardData = activityValue?.ToObject<ShareandFeedbackData>();
            var valuesFromCard = JsonConvert.DeserializeObject<ShareandFeedbackData>(taskModuleRequest.Data.ToString());

            if (valuesFromCard.Message == "isFeedbackOpen")
            {
                return new TaskModuleResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo()
                        {
                            Url = this.configuration.GetValue<string>("App:AppBaseUri") + "/user-feedback?id=" + valuesFromCard.LearningId + "&status" + FeedbackType.LearningContentFeedback,
                            Height = 315,
                            Width = 700,
                            Title = "Feedback",
                        },
                    },
                };
            }
            else
            {
                return new TaskModuleResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo()
                        {
                            Url = this.configuration.GetValue<string>("App:AppBaseUri") + "/view-content-share?id=" + valuesFromCard.LearningId,
                            Height = 600,
                            Width = 600,
                            Title = "Share content",
                        },
                    },
                };
            }
        }

        /// <summary>
        /// Deleting team information from storage when bot is uninstalled from a team.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        private void HandleMemberRemovedInTeamScopeAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            this.logger.LogInformation($"Bot removed from team {turnContext.Activity.Conversation.Id}");
            var teamsChannelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
            var teamId = teamsChannelData.Team.Id;

            // Deleting team information from storage when bot is uninstalled from a team.
            this.logger.LogInformation($"Bot removed {turnContext.Activity.Conversation.Id}");
        }

        /// <summary>
        /// Send typing indicator to the user.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task SendTypingIndicatorAsync(ITurnContext turnContext)
        {
            var typingActivity = turnContext.Activity.CreateReply();
            typingActivity.Type = ActivityTypes.Typing;
            await turnContext.SendActivityAsync(typingActivity);
        }

        /// <summary>
        /// Records event data to Application Insights telemetry client.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        private void RecordEvent(string eventName, ITurnContext turnContext)
        {
            var teamsChannelData = turnContext.Activity.GetChannelData<TeamsChannelData>();

            this.telemetryClient.TrackEvent(eventName, new Dictionary<string, string>
            {
                { "userId", turnContext.Activity.From.AadObjectId },
                { "tenantId", turnContext.Activity.Conversation.TenantId },
            });
        }

        /// <summary>
        /// Send welcome card to personal chat.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents a response.</returns>
        private async Task HandleMemberAddedInPersonalScopeAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Bot added in personal {turnContext.Activity.Conversation.Id}");
            var activity = turnContext.Activity;

            var userDetails = await TeamsInfo.GetMemberAsync(turnContext, activity.From.Id, cancellationToken);
            var result = await this.userDataRepository.GetUserByUserIdAsync(userDetails.AadObjectId);
            if (result.Count() == 0)
            {
                await this.userDataRepository.CreateUserConfigurationsAsync(new Apps.Selfhelp.Shared.Models.Entity.UserEntity()
                {
                    UserId = userDetails.AadObjectId,
                    ConversationId = activity.Conversation.Id,
                    BotInstalledOn = DateTime.UtcNow,
                    ServiceUrl = activity.ServiceUrl,
                    TenantId = userDetails.TenantId,
                    CreatedOn = DateTime.UtcNow,
                    Status = false,
                });
            }

            await this.appConfigRepository.SetServiceUrlAsync(activity.ServiceUrl);
            var cardAttachment = await this.messageService.SendWelcomeCardToUser(this.webHostEnvironment.ContentRootPath, this.configuration.GetValue<string>("AzureAd:ClientId"));
            var welcomeCardAttachment = MessageFactory.Attachment(cardAttachment);
            this.logger.LogInformation($"Sending welcome card to user in personal chat.");
            await turnContext.SendActivityAsync(welcomeCardAttachment, cancellationToken);
        }

        /// <summary>
        /// Send team welcome card to team channel.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents a response.</returns>
        private async Task HandleMemberAddedInTeamAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"bot added in team {turnContext.Activity.Conversation.Id}");
            var channeldata = turnContext.Activity.GetChannelData<TeamsChannelData>();
            var teamsDetails = turnContext.Activity.TeamsGetTeamInfo();
            this.logger.LogInformation($"bot added in personal {turnContext.Activity.Conversation.Id}");
            var result = await this.teamRepository.GetTeamByTeamsIdAsync(channeldata.Team.Id, channeldata.Team.Id);
            if (result.Count() == 0)
            {
                await this.teamRepository.CreateTeamsAsync(new Apps.Selfhelp.Shared.Models.Entity.TeamEntity()
                {
                    TeamId = teamsDetails.AadGroupId,
                    ChannelId = channeldata.Team.Id,
                    BotInstalledOn = DateTime.UtcNow,
                    TenantId = channeldata.Tenant.Id,
                    TeamName = teamsDetails.Name,
                });
            }

            var cardAttachment = await this.messageService.SendWelcomeCardToUser(this.webHostEnvironment.ContentRootPath, this.configuration.GetValue<string>("AzureAd:ClientId"));
            var welcomeCardAttachment = MessageFactory.Attachment(cardAttachment);
            this.logger.LogInformation($"Bot added to team {teamsDetails.Name}. Sending welcome card in team.");
            await turnContext.SendActivityAsync(welcomeCardAttachment, cancellationToken);
        }
    }
}
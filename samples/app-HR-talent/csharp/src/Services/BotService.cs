using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TeamsTalentMgmtApp.Models;
using TeamsTalentMgmtApp.Models.TemplateModels;
using TeamsTalentMgmtApp.Services.Interfaces;
using TeamsTalentMgmtApp.Services.Templates;
using TeamsTalentMgmtApp.Constants;
using TeamsTalentMgmtApp.Models.Commands;
using TeamsTalentMgmtApp.Models.DatabaseContext;

namespace TeamsTalentMgmtApp.Services
{
    public class BotService : IBotService
    {
        private readonly AppSettings _appSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IRecruiterService _recruiterService;
        private readonly IPositionService _positionService;
        private readonly ICandidateService _candidateService;
        private readonly IInterviewService _interviewService;
        private readonly ILocationService _locationService;
        private readonly ITokenProvider _tokenProvider;
        private readonly PositionsTemplate _positionsTemplate;
        private readonly NewJobPostingToAdaptiveCardTemplate _newJobPostingTemplate;
        private readonly CandidatesTemplate _candidatesTemplate;
        private readonly IStatePropertyAccessor<bool> _welcomeMessageState;

        public BotService(
            IOptions<AppSettings> appSettings,
            IHttpClientFactory httpClientFactory,
            IRecruiterService recruiterService,
            IPositionService positionService,
            ICandidateService candidateService,
            IInterviewService interviewService,
            ILocationService locationService,
            ITokenProvider tokenProvider,
            CandidatesTemplate candidatesTemplate,
            PositionsTemplate positionsTemplate,
            NewJobPostingToAdaptiveCardTemplate newJobPostingTemplate,
            UserState userState)
        {
            _appSettings = appSettings.Value;
            _httpClientFactory = httpClientFactory;
            _recruiterService = recruiterService;
            _positionService = positionService;
            _candidateService = candidateService;
            _interviewService = interviewService;
            _locationService = locationService;
            _tokenProvider = tokenProvider;
            _candidatesTemplate = candidatesTemplate;
            _positionsTemplate = positionsTemplate;
            _newJobPostingTemplate = newJobPostingTemplate;

            // When sending a proactive message without first storing the conversation reference
            // we need to call CreateConversationAsync which results in a ConversationUpdate activity being
            // sent to the bot with new members - because we can now receive MembersAddedAsync more than once
            // we need to cache whether the user has already been welcomed.
            _welcomeMessageState = userState.CreateProperty<bool>("SeenWelcomeMessage");
        }

        public async Task HandleMembersAddedAsync(
            ITurnContext<IConversationUpdateActivity> turnContext,
            IList<ChannelAccount> membersAdded,
            CancellationToken cancellationToken)
        {
            if (turnContext.Activity.MembersAdded.All(m => m.Id != turnContext.Activity.Recipient?.Id))
            {
                // Bot was added before so grab information about new members.
                membersAdded = membersAdded.Where(ca => turnContext.Activity.MembersAdded.Any(member => member.Id == ca.Id)).ToList();
            }
            else
            {
                if (turnContext.Activity.Conversation.IsGroup != true)
                {
                    // Check to see if we've already sent this user the welcome message since the last installation
                    // update
                    if (!await _welcomeMessageState.GetAsync(turnContext, cancellationToken: cancellationToken))
                    {
                        var card = new HeroCard
                        {
                            Title = "Hi, I'm Talent bot!",
                            Text = "I can assist you with creating new job postings, get details about your candidates, open positions and notify about your candidates stage updates. If you are admin, you can install bot for hiring managers.",
                            Buttons = new List<CardAction>
                            {
                                new CardAction(ActionTypes.ImBack, value: BotCommands.HelpDialogCommand, title: "Help")
                            }
                        };

                        await turnContext.SendActivityAsync(MessageFactory.Attachment(card.ToAttachment()), cancellationToken);

                        // Cache the fact that the user has now been sent the welcome message.
                        // They will now not see this again until either the state is removed or
                        // the installation state changes
                        await _welcomeMessageState.SetAsync(turnContext, true, cancellationToken);
                    }
                }
            }
        }

        public async Task HandleInstallationUpdateAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            // Whenever something changes with the installation, new, update or uninstalled
            // we want to reset the cached state that checks whether the user has seen the welcome message
            await _welcomeMessageState.SetAsync(turnContext, false, cancellationToken);
        }

        public async Task<IMessageActivity> LeaveInternalCommentAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            var commandData = JsonConvert.DeserializeObject<LeaveCommentCommand>(turnContext.Activity.Value?.ToString());
            if (commandData is null)
            {
                return null;
            }

            await _candidateService.AddComment(commandData, turnContext.Activity.From.Name, cancellationToken);
            var candidate = await _candidateService.GetById(commandData.CandidateId, cancellationToken);
            var interviewers = await _recruiterService.GetAllInterviewers(cancellationToken);

            var templateModel = new CandidateTemplateModel
            {
                Items = new List<Candidate> { candidate },
                Interviewers = interviewers,
                AppSettings = _appSettings,
                NoItemsLabel = "You don't have such candidate."
            };

            var messageActivity = await _candidatesTemplate.RenderTemplate(turnContext, null, TemplateConstants.CandidateAsAdaptiveCardWithMultipleItems, templateModel);

            return messageActivity;
        }

        public async Task<IMessageActivity> OpenPositionAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            var commandData = JsonConvert.DeserializeObject<PositionCreateCommand>(turnContext.Activity.Value?.ToString());
            if (commandData is null)
            {
                return null;
            }

            var position = await _positionService.AddNewPosition(turnContext.Activity.Conversation.TenantId, commandData, cancellationToken);
            var positionsTemplate = new PositionTemplateModel
            {
                Items = new List<Position> { position }
            };

            var messageActivity = await _positionsTemplate.RenderTemplate(turnContext, null, TemplateConstants.PositionAsAdaptiveCardWithMultipleItems, positionsTemplate);

            return messageActivity;
        }

        public async Task<IMessageActivity> ScheduleInterviewAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            var commandData = JsonConvert.DeserializeObject<ScheduleInterviewCommand>(turnContext.Activity.Value?.ToString());
            if (commandData is null)
            {
                return null;
            }

            await _interviewService.ScheduleInterview(commandData, cancellationToken);
            var candidate = await _candidateService.GetById(commandData.CandidateId, cancellationToken);
            var interviewers = await _recruiterService.GetAllInterviewers(cancellationToken);

            var templateModel = new CandidateTemplateModel
            {
                Items = new List<Candidate> { candidate },
                Interviewers = interviewers,
                AppSettings = _appSettings,
                NoItemsLabel = "You don't have such candidate."
            };

            var messageActivity = await _candidatesTemplate.RenderTemplate(turnContext, null, TemplateConstants.CandidateAsAdaptiveCardWithMultipleItems, templateModel);

            return messageActivity;
        }

        public async Task HandleFileAttachments(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var description = await TryToExtractDescriptionFromFile(turnContext.Activity.Attachments);
            if (!string.IsNullOrEmpty(description))
            {
                var locations = await _locationService.GetAllLocations(cancellationToken);
                var hiringManagers = await _recruiterService.GetAllHiringManagers(cancellationToken);

                await _newJobPostingTemplate.ReplyWith(turnContext, nameof(NewJobPostingToAdaptiveCardTemplate), new
                {
                    Locations = locations,
                    HiringManagers = hiringManagers,
                    Description = description
                });
            }
        }

        private async Task<string> TryToExtractDescriptionFromFile(IEnumerable<Attachment> attachments)
        {
            var attachment = attachments.First(); // assuming that user sends only one file
            if (attachment.ContentType == FileDownloadInfo.ContentType)
            {
                var downloadInfo = JsonConvert.DeserializeObject<FileDownloadInfo>(attachment.Content.ToString());
                if (downloadInfo != null)
                {
                    var client = _httpClientFactory.CreateClient();

                    // downloadUrl is an unauthenticated URL to the file contents, valid for only a few minutes
                    return await client.GetStringAsync(downloadInfo.DownloadUrl);
                }
            }

            return string.Empty;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using TeamsTalentMgmtApp.Constants;
using TeamsTalentMgmtApp.Models;
using TeamsTalentMgmtApp.Models.DatabaseContext;
using TeamsTalentMgmtApp.Models.TemplateModels;
using TeamsTalentMgmtApp.Services.Interfaces;
using TeamsTalentMgmtApp.Services.Templates;

namespace TeamsTalentMgmtApp.Controllers
{
    public class NotificationService : INotificationService
    {
        private readonly IGraphApiService _graphApiService;
        private readonly AppSettings _appSettings;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IRecruiterService _recruiterService;
        private readonly PositionsTemplate _positionsTemplate;
        private readonly CandidatesTemplate _candidatesTemplate;
        private readonly ITokenProvider _tokenProvider;

        public NotificationService(
            IGraphApiService graphApiService, 
            IOptions<AppSettings> appSettings, 
            IBotFrameworkHttpAdapter adapter, 
            IRecruiterService recruiterService,
            PositionsTemplate positionsTemplate,
            CandidatesTemplate candidatesTemplate,
            ITokenProvider tokenProvider)
        {
            _graphApiService = graphApiService;
            _appSettings = appSettings.Value;
            _adapter = adapter;
            _recruiterService = recruiterService;
            _positionsTemplate = positionsTemplate;
            _candidatesTemplate = candidatesTemplate;
            _tokenProvider = tokenProvider;
        }

        public async Task NotifyRecruiterAboutCandidateStageChange(string tenantId, Candidate candidate, CancellationToken cancellationToken)
        {
            if (candidate?.Position != null)
            {
                var recruiter = await _recruiterService.GetById(candidate.Position.HiringManagerId, cancellationToken);

                var interviewers = await _recruiterService.GetAllInterviewers(cancellationToken);
                var templateModel = new CandidateTemplateModel
                {
                    Items = new List<Candidate> { candidate },
                    Interviewers = interviewers,
                    AppSettings = _appSettings
                };

                var attachments = (await _candidatesTemplate.RenderTemplate(null, null, TemplateConstants.CandidateAsAdaptiveCardWithMultipleItems, templateModel)).Attachments;

                var activity = MessageFactory.Text($"Candidate stage has been changed for {candidate.Name} from {candidate.PreviousStage} to {candidate.Stage}");

                activity.Attachments = attachments;

                await SendProactiveNotification(recruiter.Alias, tenantId, activity, cancellationToken);
            }
        }

        public async Task NotifyRecruiterAboutNewOpenPosition(string tenantId, Position position, CancellationToken cancellationToken)
        {
            var recruiter = await _recruiterService.GetById(position.HiringManagerId, cancellationToken);

            var staticTabName = "Potential candidates";

            var positionsTemplate = new PositionTemplateModel
            {
                Items = new List<Position> { position },
                ButtonActions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Title = "Show all assigned positions",
                        Url = new Uri(string.Format(CommonConstants.DeepLinkUrlFormat, _appSettings.TeamsAppId, _appSettings.OpenPositionsTabEntityId, staticTabName))
                    }
                }
            };

            var attachments = (await _positionsTemplate.RenderTemplate(null, null, TemplateConstants.PositionAsAdaptiveCardWithMultipleItems, positionsTemplate)).Attachments;

            var activity = MessageFactory.Attachment(attachments);

            await SendProactiveNotification(recruiter.Alias, tenantId, activity, cancellationToken);
        }

        public async Task<NotificationResult> SendProactiveNotification(string aliasUpnOrOid, string tenantId, IActivity activity, CancellationToken cancellationToken)
        {
            var (upn, chatId) = await _graphApiService.GetProactiveChatIdForUser(aliasUpnOrOid, tenantId, cancellationToken);

            if (upn == null)
            {
                return NotificationResult.AliasNotFound;
            }

            if (chatId == null)
            {
                return NotificationResult.BotNotInstalled;
            }

            var credentials = new MicrosoftAppCredentials(_appSettings.MicrosoftAppId, _appSettings.MicrosoftAppPassword);

            var connectorClient = new ConnectorClient(new Uri(_appSettings.ServiceUrl), credentials);

            var members = await connectorClient.Conversations.GetConversationMembersAsync(chatId);

            var conversationParameters = new ConversationParameters
            {
                IsGroup = false,
                Bot = new ChannelAccount
                {
                    Id = "28:" + credentials.MicrosoftAppId
                },
                Members = new ChannelAccount[] { members[0] },
                TenantId = tenantId,
            };

            await ((CloudAdapter)_adapter).CreateConversationAsync(credentials.MicrosoftAppId, null, _appSettings.ServiceUrl, credentials.OAuthScope, conversationParameters, async (t1, c1) =>
            {
                var conversationReference = t1.Activity.GetConversationReference();
                await ((CloudAdapter)_adapter).ContinueConversationAsync(credentials.MicrosoftAppId, conversationReference, async (t2, c2) =>
                {
                    await t2.SendActivityAsync(activity, c2);
                }, cancellationToken);
            }, cancellationToken);

            return NotificationResult.Success;
        }
    }
}

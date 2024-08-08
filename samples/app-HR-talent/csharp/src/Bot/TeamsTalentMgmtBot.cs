using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using TeamsTalentMgmtApp.Bot.Dialogs;
using TeamsTalentMgmtApp.Extensions;
using TeamsTalentMgmtApp.Services.Interfaces;

namespace TeamsTalentMgmtApp.Bot
{
    public class TeamsTalentMgmtBot : TeamsActivityHandler
    {
        private readonly MainDialog _mainDialog;
        private readonly IInvokeActivityHandler _invokeActivityHandler;
        private readonly IBotService _botService;
        private readonly ConversationState _conversationState;
        private readonly UserState _userState;

        public TeamsTalentMgmtBot(
            MainDialog mainDialog,
            IInvokeActivityHandler invokeActivityHandler,
            IBotService botService,
            ConversationState conversationState,
            UserState userState)
        {
            _mainDialog = mainDialog;
            _conversationState = conversationState;
            _userState = userState;
            _botService = botService;
            _invokeActivityHandler = invokeActivityHandler;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
            => _invokeActivityHandler.HandleSigninVerifyStateAsync(turnContext, cancellationToken);

        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
            => _invokeActivityHandler.HandleMessagingExtensionQueryAsync(turnContext, query, cancellationToken);

        protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
            => _invokeActivityHandler.HandleMessagingExtensionFetchTaskAsync(turnContext, action, cancellationToken);

        protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
            => _invokeActivityHandler.HandleMessagingExtensionSubmitActionAsync(turnContext, action, cancellationToken);

        protected override Task OnTeamsMessagingExtensionCardButtonClickedAsync(ITurnContext<IInvokeActivity> turnContext, JObject cardData, CancellationToken cancellationToken)
            => _invokeActivityHandler.HandleMessagingExtensionOnCardButtonClickedAsync(turnContext, cardData, cancellationToken);

        protected override Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
            => _botService.HandleMembersAddedAsync(turnContext, membersAdded, cancellationToken);

        protected override Task OnInstallationUpdateActivityAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
            => _botService.HandleInstallationUpdateAsync(turnContext, cancellationToken);

        protected override Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
            => _invokeActivityHandler.HandleAppBasedLinkQueryAsync(turnContext, query, cancellationToken);

        protected override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.HasFileAttachments())
            {
                return _botService.HandleFileAttachments(turnContext, cancellationToken);
            }

            // continue process for text messages
            return _mainDialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(TeamsTalentMgmtBot)), cancellationToken);
        }

        protected override Task<InvokeResponse> OnTeamsFileConsentDeclineAsync(
            ITurnContext<IInvokeActivity> turnContext,
            FileConsentCardResponse fileConsentCardResponse,
            CancellationToken cancellationToken)
            => _invokeActivityHandler.HandleFileConsentDeclineResponse(turnContext, fileConsentCardResponse, cancellationToken);

        protected override Task<InvokeResponse> OnTeamsFileConsentAcceptAsync(
            ITurnContext<IInvokeActivity> turnContext,
            FileConsentCardResponse fileConsentCardResponse,
            CancellationToken cancellationToken)
            => _invokeActivityHandler.HandleFileConsentAcceptResponse(turnContext, fileConsentCardResponse, cancellationToken);
    }
}

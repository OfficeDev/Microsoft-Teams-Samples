using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Options;
using AppCompleteSample.utility;
using AppCompleteSample.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCompleteSample.src.dialogs;
using Microsoft.Extensions.Configuration;

namespace AppCompleteSample.Bots
{
    public class DialogBot<T> : TeamsActivityHandler where T : Dialog
    {
        protected readonly Dialog _dialog;
        protected readonly BotState _conversationState;
        protected readonly BotState _userState;
        private readonly IOptions<AzureSettings> azureSettings;

        public DialogBot(IConfiguration configuration, ConversationState conversationState, T dialog, UserState userState, IOptions<AzureSettings> azureSettings)
        {
            _conversationState = conversationState;
            _dialog = dialog;
            _userState = userState;
            this.azureSettings = azureSettings;
        }

        /// <summary>
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// For more information on bot messaging in Teams, see the documentation
        /// https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/conversation-basics?tabs=dotnet#receive-a-message .
        /// </remarks>
        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            // Set the Locale for Bot
            turnContext.Activity.Locale = TemplateUtility.GetLocale(turnContext.Activity);

            // Run the Dialog with the new message Activity.
            await _dialog.Run(
                    turnContext,
                    _conversationState.CreateProperty<DialogState>(nameof(DialogState)),
                    cancellationToken);

            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Handle when a message reaction is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageReactionActivityAsync(ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == ActivityTypes.MessageReaction)
            {
                var reactions = turnContext.Activity.AsMessageReactionActivity();
                if (reactions.ReactionsAdded != null && reactions.ReactionsAdded.Count > 0)
                {
                    await turnContext.SendActivityAsync(Strings.LikeMessage);
                }
                else if (reactions.ReactionsRemoved != null && reactions.ReactionsRemoved.Count > 0)
                {
                    await turnContext.SendActivityAsync(Strings.RemoveLike);
                }
            }
        }

        /// <summary>
        /// Invoked when the user submit any action from O365 Connector Card.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="query">Contains O365 Connector Card query keywords.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnTeamsO365ConnectorCardActionAsync(ITurnContext<IInvokeActivity> turnContext, O365ConnectorCardActionQuery query, CancellationToken cancellationToken)
        {
            await HandleO365ConnectorCardActionQuery(turnContext, query);
        }

        /// <summary>
        /// Invoked when the user askfor sign in.
        /// </summary>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnSignInInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            await PopUpSignInHandler(turnContext);
        }

        /// <summary>
        /// Invoked when the user opens the Messaging Extension or searching any content in it.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="query">Contains Messaging Extension query keywords.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Messaging extension response object to fill compose extension section.</returns>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.teams.teamsactivityhandler.onteamsmessagingextensionqueryasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionQuery query,
            CancellationToken cancellationToken)
        {
                turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));

                var activity = turnContext.Activity;
                WikipediaComposeExtension wikipediaComposeExtension = new WikipediaComposeExtension(this._userState.CreateProperty<UserData>(nameof(UserData)), this.azureSettings);
                if (activity.Name == "composeExtension/selectItem")
                {
                    var selectedItemResponse = wikipediaComposeExtension.HandleComposeExtensionSelectedItem(turnContext, query);
                    this._userState.SaveChangesAsync(turnContext, false, cancellationToken);
                    return selectedItemResponse;
                }
                else
                {
                    var result = wikipediaComposeExtension.GetComposeExtensionResponseAsync(turnContext, query);
                    this._userState.SaveChangesAsync(turnContext, false, cancellationToken);
                    return result;
                }
        }

        /// <summary>
        /// Handle request from bot.
        /// </summary>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Invoked when bot (like a user) are added to the conversation.
        /// </summary>
        /// <param name="membersAdded">Object containing information of the member added.</param>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            var activity = turnContext.Activity;

            if (activity.Conversation.ConversationType == "personal")
            {
                if (activity.MembersAdded != null && activity.MembersAdded.Any(member => member.Id == activity.Recipient.Id))
                {
                    await this.SendWelcomeMessagePersonalScopeAsync(turnContext);
                }
            }
        }

        /// <summary>
        /// Invoked when bot (like a user) are removed from the conversation.
        /// </summary>
        /// <param name="membersAdded">Object containing information of the member added.</param>
        /// <param name = "turnContext" > The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMembersRemovedAsync(IList<ChannelAccount> membersRemoved, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            await this._conversationState.ClearStateAsync(turnContext, cancellationToken);
            await this._userState.ClearStateAsync(turnContext, cancellationToken);
        }

        /// <summary>
        /// Sent welcome message to personal chat.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        private async Task SendWelcomeMessagePersonalScopeAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            var userStateAccessors = this._conversationState.CreateProperty<UserConversationState>(nameof(UserConversationState));
            var userConversationState = await userStateAccessors.GetAsync(turnContext, () => new UserConversationState());

            if (userConversationState?.IsWelcomeCardSent == null || userConversationState?.IsWelcomeCardSent == false)
            {
                userConversationState.IsWelcomeCardSent = true;
                await userStateAccessors.SetAsync(turnContext, userConversationState);
                await turnContext.SendActivityAsync(MessageFactory.Text(Strings.BotWelcomeMessage));
            }
        }

        /// <summary>
        /// Handles O365 connector card action queries.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="query">Incoming request from Bot Framework.</param>
        /// <returns>Task tracking operation.</returns>
        private static async Task HandleO365ConnectorCardActionQuery(ITurnContext<IInvokeActivity> turnContext, O365ConnectorCardActionQuery query)
        {
            // Get O365 connector card query data.
            O365ConnectorCardActionQuery o365CardQuery = query;
            var Text = $"Thanks, {turnContext.Activity.From.Name}\nYour input action ID:{o365CardQuery.ActionId}\nYour input body:{o365CardQuery.Body}";
            await turnContext.SendActivityAsync(Text);
        }

        /// <summary>
        /// Handle the PopUp SignIn requests
        /// </summary>
        /// <param name="turnContext"></param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> PopUpSignInHandler(ITurnContext<IInvokeActivity> turnContext)
        {
            await turnContext.SendActivityAsync("Authentication Successful");
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
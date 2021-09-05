using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using Microsoft.Teams.TemplateBotCSharp.utility;
using Microsoft.Teams.TemplateBotCSharp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Bots
{
    public class DialogBot<T> : TeamsActivityHandler where T : Dialog
    {
        protected readonly Dialog _dialog;
        protected readonly BotState _conversationState;
        protected readonly BotState _userState;


        public DialogBot(ConversationState conversationState, T dialog, UserState userState)
        {
            _conversationState = conversationState;
            _dialog = dialog;
            _userState = userState;
        }

        protected override async Task OnMessageActivityAsync(
    ITurnContext<IMessageActivity> turnContext,
    CancellationToken cancellationToken)
        {
            //Set the Locale for Bot
            turnContext.Activity.Locale = TemplateUtility.GetLocale(turnContext.Activity);

            // Run the Dialog with the new message Activity.
            await _dialog.Run(
                turnContext,
                _conversationState.CreateProperty<DialogState>(nameof(DialogState)),
                cancellationToken);

            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
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
            try
            {
                turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));

                var activity = turnContext.Activity;
                WikipediaComposeExtension wikipediaComposeExtension = new WikipediaComposeExtension(this._userState.CreateProperty<UserData>(nameof(UserData)));
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
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Invoked when members other than this bot (like a user) are removed from the conversation.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
                   var activity = turnContext.Activity;
 
                if (activity.Conversation.ConversationType == "personal")
                {
                    if (activity.MembersAdded != null && activity.MembersAdded.Any(member => member.Id == activity.Recipient.Id))
                    {
                        await this.SendWelcomeCardInPersonalScopeAsync(turnContext);
                    }
                }
         }

        /// <summary>
        /// Sent welcome card to personal chat.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        private async Task SendWelcomeCardInPersonalScopeAsync(ITurnContext<IConversationUpdateActivity> turnContext)
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
    }

    public class UserConversationState
    {
        /// <summary>
        /// Gets or sets a value indicating whether the welcome card is sent to user or not.
        /// </summary>
        /// <remark>Value is null when bot is installed for first time.</remark>
        public bool IsWelcomeCardSent { get; set; }
    }
}

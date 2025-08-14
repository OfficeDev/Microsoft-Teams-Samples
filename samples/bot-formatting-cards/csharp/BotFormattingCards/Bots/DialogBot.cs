/// <summary>
/// Copyright(c) Microsoft. All Rights Reserved.
/// Licensed under the MIT License.
/// </summary>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BotAllCards.Cards;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// This IBot implementation can run any type of Dialog. The use of type parameterization allows multiple different bots
    /// to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    /// each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    /// The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    /// and the requirement is that all BotState objects are saved at the end of a turn.
    /// </summary>
    /// <typeparam name="T">The type of dialog to run.</typeparam>
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        protected readonly BotState conversationState;
        protected readonly Dialog dialog;
        protected readonly ILogger logger;
        protected readonly BotState userState;

        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            this.conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            this.userState = userState ?? throw new ArgumentNullException(nameof(userState));
            this.dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles an incoming activity.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A Task resolving to either a login card or the adaptive card of the Reddit post.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            logger.LogInformation("Running dialog with Message Activity.");

            if (turnContext.Activity.Text == "MentionSupport")
            {
                var member = new TeamsChannelAccount();

                try
                {
                    member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
                }
                catch (ErrorResponseException e) when (e.Body.Error.Code.Equals("MemberNotFoundInConversation", StringComparison.OrdinalIgnoreCase))
                {
                    await turnContext.SendActivityAsync("Member not found.", cancellationToken: cancellationToken);
                    return;
                }

                var attachments = new List<Attachment>
                    {
                        AllCards.SendMentionSupportCardAsync(member.Name)
                    };

                var reply = MessageFactory.Attachment(attachments);
                await turnContext.SendActivityAsync(reply, cancellationToken);

                await turnContext.SendActivityAsync(MessageFactory.Text($"You have selected <b>{turnContext.Activity.Text}</b>"), cancellationToken);
                await turnContext.SendActivityAsync(MessageFactory.Text("Type anything to see all cards."), cancellationToken);
            }
            else if (turnContext.Activity.Value != null && turnContext.Activity.Text == null)
            {
                var activityValue = (JObject)turnContext.Activity.Value;

                // Star ratings in Adaptive Cards
                if (activityValue.ContainsKey("rating1") && activityValue.ContainsKey("rating2"))
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Ratings Feedback: {turnContext.Activity.Value}"), cancellationToken);
                }
            }
            else
            {
                // Run the Dialog with the new message Activity.
                await dialog.RunAsync(turnContext, conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            }
        }
    }
}

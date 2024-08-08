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

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    /// to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    /// each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    /// The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    /// and the requirement is that all BotState objects are saved at the end of a turn.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        protected readonly BotState ConversationState;
        protected readonly Dialog Dialog;
        protected readonly ILogger Logger;
        protected readonly BotState UserState;

        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
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
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A Task resolving to either a login card or the adaptive card of the Reddit post.</returns>
        /// <remarks>
        /// For more information on bot messaging in Teams, see the documentation
        /// https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/conversation-basics?tabs=dotnet#receive-a-message .
        /// </remarks>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with Message Activity.");
            if (turnContext.Activity.Text == "MentionSupport")
            {
                var member = new TeamsChannelAccount();

                try
                {
                    member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
                }
                catch (ErrorResponseException e)
                {
                    if (e.Body.Error.Code.Equals("MemberNotFoundInConversation", StringComparison.OrdinalIgnoreCase))
                    {
                        await turnContext.SendActivityAsync("Member not found.");
                        return;
                    }
                    else
                    {
                        throw e;
                    }
                }

                var attachments = new List<Attachment>();

                // Reply to the activity we received with an activity.
                var reply = MessageFactory.Attachment(attachments);
                reply.Attachments.Add(AllCards.sendMentionSupportCardAsync(member.Name));
                await turnContext.SendActivityAsync(reply, cancellationToken);

                await turnContext.SendActivityAsync(MessageFactory.Text("You have Selected <b>" + turnContext.Activity.Text + "</b>"), cancellationToken);

                // Give the user instructions about what to do next
                await turnContext.SendActivityAsync(MessageFactory.Text("Type anything to see all card."), cancellationToken);
                
            }
            else
            {
                // Run the Dialog with the new message Activity.
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            }
        }
    }
}

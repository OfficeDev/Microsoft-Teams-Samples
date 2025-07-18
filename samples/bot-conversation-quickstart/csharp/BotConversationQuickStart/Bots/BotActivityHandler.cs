// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Teams;
namespace BotConversationQuickStart.Bots
{
    public class BotActivityHandler : TeamsActivityHandler
    {
        public BotActivityHandler() { }


        /// <summary>
        /// Handles incoming message activities from the user.
        /// If the message text is "Hello", it triggers a response via MentionActivityAsync.
        /// Otherwise, it sends a Hero Card with a "Say Hello" button using MessageBack action.
        /// </summary>
        /// <param name="turnContext">Context object containing information about the incoming message activity.</param>
        /// <param name="cancellationToken">Token for cancelling the async operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var userMessage = turnContext.Activity.Text?.Trim();

            if (userMessage == "Hello")
            {
                await MentionActivityAsync(turnContext, cancellationToken);
            }
            else
            {
                var card = new HeroCard
                {
                    Title = "Let's talk...",
                    Buttons = new List<CardAction>
                    {
                        new CardAction
                        {
                            Type = ActionTypes.MessageBack,
                            Title = "Say Hello",
                            Text = "Hello",
                            Value = new { count = 0 }
                        }
                    }
                };

                // Sends the Hero Card with the "Say Hello" button as a response to the user.
                await turnContext.SendActivityAsync(MessageFactory.Attachment(card.ToAttachment()), cancellationToken);
            }
        }

        /// <summary>
        /// Sends a mention activity that tags the user in the reply message.
        /// Constructs a mention entity using the user's name and includes it in the response.
        /// </summary>
        /// <param name="turnContext">Context object containing information about the incoming message activity.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation if needed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task MentionActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var user = turnContext.Activity.From;

            var mention = new Mention
            {
                Mentioned = user,
                Text = $"<at>{user.Name}</at>",
                Type = "mention"
            };

            var reply = MessageFactory.Text($"Hi {mention.Text}");
            reply.Entities = new List<Entity> { mention };

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

    }
}

using MessageReaction.Log;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;

namespace MessageReaction.Bot
{
    public class MessageReactionBot : TeamsActivityHandler
    {
        private readonly ActivityLog _log;

        public MessageReactionBot(ActivityLog log)
        {
            _log = log;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await SendMessageAndLogActivityId(turnContext, $"echo: {turnContext.Activity.Text}", cancellationToken);
        }

        protected override async Task OnReactionsAddedAsync(IList<Microsoft.Bot.Schema.MessageReaction> messageReactions, ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var reaction in messageReactions)
            {
                // The ReplyToId property of the inbound MessageReaction Activity will correspond to a Message Activity which
                // had previously been sent from this bot.
                var activity = await _log.Find(turnContext.Activity.ReplyToId);
                if (activity == null)
                {
                    // If we had sent the message from the error handler we wouldn't have recorded the Activity Id and so we
                    // shouldn't expect to see it in the log.
                    await SendMessageAndLogActivityId(turnContext, $"Activity {turnContext.Activity.ReplyToId} not found in the log.", cancellationToken);
                }

                await SendMessageAndLogActivityId(turnContext, $"You added '{reaction.Type}' regarding '{activity.Text}'", cancellationToken);
            }
        }

        protected override async Task OnReactionsRemovedAsync(IList<Microsoft.Bot.Schema.MessageReaction> messageReactions, ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var reaction in messageReactions)
            {
                // The ReplyToId property of the inbound MessageReaction Activity will correspond to a Message Activity which
                // was previously sent from this bot.
                var activity = await _log.Find(turnContext.Activity.ReplyToId);
                if (activity == null)
                {
                    // If we had sent the message from the error handler we wouldn't have recorded the Activity Id and so we
                    // shouldn't expect to see it in the log.
                    await SendMessageAndLogActivityId(turnContext, $"Activity {turnContext.Activity.ReplyToId} not found in the log.", cancellationToken);
                }

                await SendMessageAndLogActivityId(turnContext, $"You removed '{reaction.Type}' regarding '{activity.Text}'", cancellationToken);
            }
        }

        private async Task SendMessageAndLogActivityId(ITurnContext turnContext, string text, CancellationToken cancellationToken)
        {
            // We need to record the Activity Id from the Activity just sent in order to understand what the reaction is a reaction too. 
            var replyActivity = MessageFactory.Text(text);
            var resourceResponse = await turnContext.SendActivityAsync(replyActivity, cancellationToken);
            await _log.Append(resourceResponse.Id, replyActivity);
        }
    }
}


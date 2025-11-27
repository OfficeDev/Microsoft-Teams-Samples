using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Annotations;
using System.Collections.Concurrent;

namespace MessageReaction.Controllers
{
    [TeamsController]
    public class Controller()
    {
        // Store sent messages in memory (activityId -> messageText)
        private static readonly ConcurrentDictionary<string, string> _messageLog = new();

        [Message]
        public async Task OnMessage([Context] MessageActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            log.Info("hit!");
            await client.Typing();
            var response = await client.Send($"you said '{activity.Text}'");
            
            // Store the sent message for later reference
            if (response?.Id != null)
            {
                _messageLog[response.Id] = $"you said '{activity.Text}'";
            }
        }

        [Conversation.MembersAdded]
        public async Task OnMembersAdded(IContext<ConversationUpdateActivity> context)
        {
            var welcomeText = "How can I help you today?";
            foreach (var member in context.Activity.MembersAdded)
            {
                if (member.Id != context.Activity.Recipient.Id)
                {
                    var response = await context.Send(welcomeText);
                    
                    // Store the sent message
                    if (response?.Id != null)
                    {
                        _messageLog[response.Id] = welcomeText;
                    }
                }
            }
        }

        [Message.ReactionAdded]
        public async Task OnReactionAdded(IContext<MessageReactionActivity> context)
        {
            var reactionsAdded = context.Activity.ReactionsAdded;
            if (reactionsAdded != null && reactionsAdded.Count > 0)
            {
                foreach (var reaction in reactionsAdded)
                {
                    var replyToId = context.Activity.ReplyToId;
                    var originalMessage = _messageLog.TryGetValue(replyToId ?? "", out var msg) 
                        ? msg 
                        : replyToId;
                    
                    var message = $"You reacted with '{reaction.Type}' to the following message: '{originalMessage}'";
                    await context.Send(message);
                }
            }
        }

        [Message.ReactionRemoved]
        public async Task OnReactionRemoved(IContext<MessageReactionActivity> context)
        {
            var reactionsRemoved = context.Activity.ReactionsRemoved;
            if (reactionsRemoved != null && reactionsRemoved.Count > 0)
            {
                foreach (var reaction in reactionsRemoved)
                {
                    var replyToId = context.Activity.ReplyToId;
                    var originalMessage = _messageLog.TryGetValue(replyToId ?? "", out var msg) 
                        ? msg 
                        : replyToId;
                    
                    var message = $"You removed the reaction '{reaction.Type}' from the following message: '{originalMessage}'";
                    await context.Send(message);
                }
            }
        }
    }
}
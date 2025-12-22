using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Annotations;

namespace SidePanel.Controllers
{
    [TeamsController]
    public class Controller()
    {
        [Message]
        public async Task OnMessage([Context] MessageActivity activity, [Context] IContext.Client client, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            // Store conversation details for proactive messaging
            HomeController.serviceUrl = activity.ServiceUrl;
            HomeController.conversationId = activity.Conversation.Id;

            var replyText = $"Hello and welcome **{activity.From.Name}** to the Meeting Extensibility SidePanel app.";
            
            log.Info($"Message received from {activity.From.Name}");
            await client.Send(replyText);
        }

        [Conversation.MembersAdded]
        public async Task OnMembersAdded(IContext<ConversationUpdateActivity> context)
        {
            // Store conversation details immediately when members are added
            HomeController.serviceUrl = context.Activity.ServiceUrl;
            HomeController.conversationId = context.Activity.Conversation.Id;

            var welcomeText = $"Hello and welcome to the Meeting Extensibility SidePanel app.";
            foreach (var member in context.Activity.MembersAdded)
            {
                if (member.Id != context.Activity.Recipient.Id)
                {
                    await context.Send(welcomeText);
                }
            }
        }

        [Conversation.Update]
        public Task OnConversationUpdate([Context] ConversationUpdateActivity activity, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            // Store conversation details for proactive messaging
            HomeController.serviceUrl = activity.ServiceUrl;
            HomeController.conversationId = activity.Conversation.Id;
            
            log.Info($"Conversation updated: {activity.Conversation.Id}, ServiceUrl: {activity.ServiceUrl}");
            return Task.CompletedTask;
        }

        [Install]
        public Task OnInstall([Context] InstallUpdateActivity activity, [Context] Microsoft.Teams.Common.Logging.ILogger log)
        {
            // Store conversation details when app is installed
            HomeController.serviceUrl = activity.ServiceUrl;
            HomeController.conversationId = activity.Conversation.Id;
            
            log.Info($"App installed in conversation: {activity.Conversation.Id}, ServiceUrl: {activity.ServiceUrl}");
            return Task.CompletedTask;
        }
    }
}
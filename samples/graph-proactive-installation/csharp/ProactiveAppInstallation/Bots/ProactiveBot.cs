using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using ProactiveBot.Bots;
using ProactiveBot.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples
{
    public class ProactiveBot : TeamsActivityHandler
    {
        public readonly IConfiguration _configuration;
        private readonly ProactiveAppIntallationHelper _helper = new ProactiveAppIntallationHelper();
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.Activity.RemoveRecipientMention();
            var text = turnContext.Activity.Text.Trim().ToLower();

            if (text.Contains("install"))
            {
                var result = await InstalledAppsinPersonalScopeAsync(turnContext, cancellationToken);
                await turnContext.SendActivityAsync(MessageFactory.Text($"Existing: {result.Existing} \n\n Newly Installed: {result.New}"), cancellationToken);
            }
            else if (text.Contains("send"))
            {
                var count = await SendNotificationToAllUsersAsync(turnContext, cancellationToken);
                await turnContext.SendActivityAsync(MessageFactory.Text($"Message sent: {count}"), cancellationToken);
            }
        }

        public ProactiveBot(ConcurrentDictionary<string, ConversationReference> conversationReferences, IConfiguration configuration)
        {
            _conversationReferences = conversationReferences;
            _configuration = configuration;
        }

        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.AadObjectId, conversationReference, (key, newValue) => conversationReference);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // Add current user to conversation reference.
                    AddConversationReference(turnContext.Activity as Activity);
                }
            }
        }

        public async Task<int> SendNotificationToAllUsersAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            int msgSentCount = 0;

            // Send notification to all the members
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await turnContext.Adapter.ContinueConversationAsync(_configuration["MicrosoftAppId"], conversationReference, BotCallback, cancellationToken);
                msgSentCount++;
            }

            return msgSentCount;
        }

        public async Task<InstallationCounts> InstalledAppsinPersonalScopeAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // For demo purpose we are installing this for first 10 users.
            var currentPage_Memebers = await TeamsInfo.GetPagedMembersAsync(turnContext, 10, null, cancellationToken);
            int existingAppInstallCount = _conversationReferences.Count;
            int newInstallationCount = 0;

            foreach (var teamMember in currentPage_Memebers.Members)
            {
                // Check if present in App Conversation reference
                if (!_conversationReferences.ContainsKey(teamMember.AadObjectId))
                {
                    // Perform installation for all the member whose conversation reference is not available.
                    await _helper.AppinstallationforPersonal(teamMember.AadObjectId, turnContext.Activity.Conversation.TenantId, _configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"], _configuration["AppCatalogTeamAppId"]);
                    newInstallationCount++;
                }
            }

            return new InstallationCounts
            {
                Existing = existingAppInstallCount,
                New = newInstallationCount
            };
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("Proactive hello.");
        }
    }
}
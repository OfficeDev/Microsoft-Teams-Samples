using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema.Teams;
using ProactiveBot.Bots;
using Microsoft.Bot.Connector;
using System.Net;
using Attachment = Microsoft.Bot.Schema.Attachment;
using System.IO;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples
{
    public class ProactiveBot : TeamsActivityHandler
    {
        public static string MicrosoftTenantId;
        public static string MicrosoftAppId;
        public static string MicrosoftAppPassword;
        public static string MicrosoftTeamAppid;
        public static string  continuationToken = null;

        public readonly IConfiguration _configuration;
        ProactiveHelper Helper = new ProactiveHelper();
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        public ProactiveBot(ConcurrentDictionary<string, ConversationReference> conversationReferences, IConfiguration configuration)
        {
            _conversationReferences = conversationReferences;
            _configuration = configuration;
            MicrosoftAppId = _configuration["MicrosoftAppId"];
            MicrosoftAppPassword = _configuration["MicrosoftAppPassword"];
            MicrosoftTeamAppid = _configuration["MicrosoftTeamAppid"];
        }
        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }

        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            bool status = false;
            string Userid = string.Empty;
            string teamId = string.Empty;
            string ChatId = string.Empty;
            MicrosoftTenantId = turnContext.Activity.Conversation.TenantId;
            var adaptiveAttachment = New_AdaptiveCardAttachment();
            switch (turnContext.Activity.Conversation.ConversationType)
            {
                case "personal":
                    Userid = turnContext.Activity.MembersAdded[0].AadObjectId;
                    Tuple<bool, int> App_status = await Helper.AppinstallationforPersonal(Userid, MicrosoftTenantId, MicrosoftAppId, MicrosoftAppPassword, MicrosoftTeamAppid);
                    if (App_status.Item1)
                    {
                        if (App_status.Item2 > 1)
                        {
                            var Exist_adaptiveAttachment = Exist_AdaptiveCardAttachment();
                            await turnContext.SendActivityAsync(MessageFactory.Attachment(Exist_adaptiveAttachment), cancellationToken);
                        }
                        else
                        {
                            await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveAttachment), cancellationToken);
                        }
                    }
                    break;
                case "channel":
                    if (turnContext.Activity.MembersAdded != null)
                    {
                        teamId = turnContext.Activity.TeamsGetTeamInfo().AadGroupId;
                        status = await Helper.AppInstallationforChannel(teamId, MicrosoftTenantId, MicrosoftAppId, MicrosoftAppPassword, MicrosoftTeamAppid);
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveAttachment), cancellationToken);
                    }
                    break;
                case "groupChat":
                    if (turnContext.Activity.MembersAdded != null)
                    {
                        ChatId = turnContext.Activity.Conversation.Id;
                        var serviceUrl = turnContext.Activity.ServiceUrl;
                        status = await Helper.AppInstallationforChat(ChatId, MicrosoftTenantId, MicrosoftAppId, MicrosoftAppPassword, MicrosoftTeamAppid);
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveAttachment), cancellationToken);
                        var currentPage_Memebers = await TeamsInfo.GetPagedMembersAsync(turnContext, 100, continuationToken, cancellationToken);
                        foreach (var teamMember in currentPage_Memebers.Members)
                        {
                            Tuple<bool, int> AppInstalled_status = await Helper.AppinstallationforPersonal(teamMember.AadObjectId, MicrosoftTenantId, MicrosoftAppId, MicrosoftAppPassword, MicrosoftTeamAppid);
                            if (AppInstalled_status.Item1)
                            {
                                var conversationParameters = new ConversationParameters
                                {
                                    IsGroup = false,
                                    Bot = turnContext.Activity.Recipient,
                                    Members = new ChannelAccount[] { teamMember },
                                    TenantId = turnContext.Activity.Conversation.TenantId,
                                };
                                try
                                {
                                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                                    using var connector = new ConnectorClient(new Uri(turnContext.Activity.ServiceUrl), MicrosoftAppId, MicrosoftAppPassword);
                                    var conversationResource = await connector.Conversations.CreateConversationAsync(conversationParameters);
                                    var replyMessage = Activity.CreateMessageActivity();
                                    replyMessage.Conversation = new ConversationAccount(id: conversationResource.Id.ToString());
                                    replyMessage.ChannelData = new TeamsChannelData() { Notification = new NotificationInfo(true) };
                                    if (AppInstalled_status.Item2 > 1)
                                    {
                                        var Exist_adaptiveAttachment = Exist_AdaptiveCardAttachment();
                                        replyMessage.Attachments = new List<Attachment> { Exist_adaptiveAttachment };
                                    }
                                    else
                                    {
                                        replyMessage.Attachments = new List<Attachment> { adaptiveAttachment };
                                    }
                                    await connector.Conversations.SendToConversationAsync(conversationResource.Id, (Activity)replyMessage);
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }
                        }
                    }
                    break;
                default: break;
            }

        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var adaptiveAttachment = New_AdaptiveCardAttachment();
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveAttachment), cancellationToken);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.Activity.RemoveRecipientMention();
            var text = turnContext.Activity.Text.Trim().ToLower();

            if (text.Contains("install"))
                await InstalledAppsinPersonalScopeAsync(turnContext, cancellationToken);

            AddConversationReference(turnContext.Activity as Activity);
            await turnContext.SendActivityAsync(MessageFactory.Text($"You sent '{turnContext.Activity.Text}'"), cancellationToken);
        }

        public async Task InstalledAppsinPersonalScopeAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            MicrosoftTenantId = turnContext.Activity.Conversation.TenantId;
            var adaptiveAttachment = New_AdaptiveCardAttachment();
            var currentPage_Memebers = await TeamsInfo.GetPagedMembersAsync(turnContext, 100, continuationToken, cancellationToken);
            foreach (var teamMember in currentPage_Memebers.Members)
            {
                Tuple<bool, int> AppInstalled_status = await Helper.AppinstallationforPersonal(teamMember.AadObjectId, MicrosoftTenantId, MicrosoftAppId, MicrosoftAppPassword, MicrosoftTeamAppid);
                if (AppInstalled_status.Item1)
                {
                    var conversationParameters = new ConversationParameters
                    {
                        IsGroup = false,
                        Bot = turnContext.Activity.Recipient,
                        Members = new ChannelAccount[] { teamMember },
                        TenantId = turnContext.Activity.Conversation.TenantId,
                    };
                    try
                    {
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        using var connector = new ConnectorClient(new Uri(turnContext.Activity.ServiceUrl), MicrosoftAppId, MicrosoftAppPassword);
                        var conversationResource = await connector.Conversations.CreateConversationAsync(conversationParameters);
                        var replyMessage = Activity.CreateMessageActivity();
                        replyMessage.Conversation = new ConversationAccount(id: conversationResource.Id.ToString());
                        replyMessage.ChannelData = new TeamsChannelData() { Notification = new NotificationInfo(true) };
                        if (AppInstalled_status.Item2 > 1)
                        {
                            var Exist_adaptiveAttachment = Exist_AdaptiveCardAttachment();
                            replyMessage.Attachments = new List<Attachment> { Exist_adaptiveAttachment };
                        }
                        else
                        {
                            replyMessage.Attachments = new List<Attachment> { adaptiveAttachment };
                        }
                        await connector.Conversations.SendToConversationAsync(conversationResource.Id, (Activity)replyMessage);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }
        public static Attachment New_AdaptiveCardAttachment()
        {
            var paths = new[] { ".", "Resources", "New_AdaptiveCard.json" };
            var adaptiveCardJson = System.IO.File.ReadAllText(Path.Combine(paths));
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

        public static Attachment Exist_AdaptiveCardAttachment()
        {
            var paths = new[] { ".", "Resources", "Exist_AdaptiveCard.json" };
            var adaptiveCardJson = System.IO.File.ReadAllText(Path.Combine(paths));
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

    }
}

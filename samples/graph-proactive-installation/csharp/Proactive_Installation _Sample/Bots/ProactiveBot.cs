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
using AdaptiveCards;
using ProactiveBot.Models;

namespace Microsoft.BotBuilderSamples
{
    public class ProactiveBot : TeamsActivityHandler
    {
        public readonly IConfiguration _configuration;
        ProactiveHelper Helper = new ProactiveHelper();
        CheckAppCount CheckAppCount = new CheckAppCount();
        CheckAppStatus objcheckAppStatus = new CheckAppStatus();
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        public ProactiveBot(ConcurrentDictionary<string, ConversationReference> conversationReferences, IConfiguration configuration)
        {
            _conversationReferences = conversationReferences;
            _configuration = configuration;
        }
        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }

        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            switch (turnContext.Activity.Conversation.ConversationType)
            {
                case "channel":
                    if (turnContext.Activity.MembersAdded != null)
                    {
                        await Helper.AppInstallationforChannel(turnContext.Activity.TeamsGetTeamInfo().AadGroupId, turnContext.Activity.Conversation.TenantId, _configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"], _configuration["TeamAppCatalogid"]);
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(New_AdaptiveCardAttachment()), cancellationToken);
                    }
                    break;
                case "groupChat":
                    if (turnContext.Activity.MembersAdded != null)
                    {
                        await Helper.AppInstallationforChat(turnContext.Activity.Conversation.Id, turnContext.Activity.Conversation.TenantId, _configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"], _configuration["TeamAppCatalogid"]);
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(New_AdaptiveCardAttachment()), cancellationToken);
                    }
                    break;
                default: break;
            }
            await InstallAppinGroupchatandTeamScopeAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(New_AdaptiveCardAttachment()), cancellationToken);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.Activity.RemoveRecipientMention();
            var text = turnContext.Activity.Text.Trim().ToLower();

            if (text.Contains("install"))
                CheckAppCount = await InstalledAppsinPersonalScopeAsync(turnContext, cancellationToken);

            AddConversationReference(turnContext.Activity as Activity);
            await turnContext.SendActivityAsync(MessageFactory.Attachment(InstalledAppCount_Attachment(CheckAppCount.New_Count, CheckAppCount.Exist_Count)), cancellationToken);
        }

        public async Task<CheckAppCount> InstalledAppsinPersonalScopeAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var currentPage_Memebers = await TeamsInfo.GetPagedMembersAsync(turnContext, 100, null, cancellationToken);
            int ExistedApp_Count = 0;
            int NewlyAddedApp_Count = 0;
            foreach (var teamMember in currentPage_Memebers.Members)
            {
                objcheckAppStatus = await Helper.AppinstallationforPersonal(teamMember.AadObjectId, turnContext.Activity.Conversation.TenantId, _configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"], _configuration["TeamAppCatalogid"]);
                if (objcheckAppStatus.CheckStatus)
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
                        using var connector = new ConnectorClient(new Uri(turnContext.Activity.ServiceUrl), _configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"]);

                        var conversationResource = await connector.Conversations.CreateConversationAsync(conversationParameters);
                        var replyMessage = Activity.CreateMessageActivity();
                        replyMessage.Conversation = new ConversationAccount(id: conversationResource.Id.ToString());
                        replyMessage.ChannelData = new TeamsChannelData() { Notification = new NotificationInfo(true) };
                        if (objcheckAppStatus.AppCount > 1)
                        {
                            replyMessage.Attachments = new List<Attachment> { Exist_AdaptiveCardAttachment() };
                            ExistedApp_Count++;
                        }
                        else
                        {
                            NewlyAddedApp_Count++;
                            replyMessage.Attachments = new List<Attachment> { New_AdaptiveCardAttachment() };
                        }
                        await connector.Conversations.SendToConversationAsync(conversationResource.Id, (Activity)replyMessage);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            CheckAppCount.Exist_Count = ExistedApp_Count;
            CheckAppCount.New_Count = NewlyAddedApp_Count;
            return CheckAppCount;
        }

        public async Task InstallAppinGroupchatandTeamScopeAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var currentPage_Memebers = await TeamsInfo.GetPagedMembersAsync(turnContext, 100, null, cancellationToken);
            foreach (var teamMember in currentPage_Memebers.Members)
            {
                objcheckAppStatus = await Helper.AppinstallationforPersonal(teamMember.AadObjectId, turnContext.Activity.Conversation.TenantId, _configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"], _configuration["TeamAppCatalogid"]);
                if (objcheckAppStatus.CheckStatus)
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
                        using var connector = new ConnectorClient(new Uri(turnContext.Activity.ServiceUrl), _configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"]);
                        var conversationResource = await connector.Conversations.CreateConversationAsync(conversationParameters);
                        var replyMessage = Activity.CreateMessageActivity();
                        replyMessage.Conversation = new ConversationAccount(id: conversationResource.Id.ToString());
                        replyMessage.ChannelData = new TeamsChannelData() { Notification = new NotificationInfo(true) };
                        if (objcheckAppStatus.AppCount > 1)
                        {
                            replyMessage.Attachments = new List<Attachment> { Exist_AdaptiveCardAttachment() };
                        }
                        else
                        {
                            replyMessage.Attachments = new List<Attachment> { New_AdaptiveCardAttachment() };
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

        public static Attachment InstalledAppCount_Attachment(int New_Count, int Existed_Count)
        {
            var AppCountCard = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer
                    {
                        Items=new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Text=$"Hey ! Please Check the Installed App Count ",
                                Size=AdaptiveTextSize.Large,
                                Wrap=true
                            },

                            new AdaptiveColumnSet()
                            {
                                Columns=new List<AdaptiveColumn>()
                                {
                                   new AdaptiveColumn()
                                    {
                                         Width=AdaptiveColumnWidth.Auto,
                                         Items=new List<AdaptiveElement>()
                                         {
                                             new AdaptiveTextBlock(){Text="Newly Added Count is :  "+New_Count,Color=AdaptiveTextColor.Accent,Size=AdaptiveTextSize.Medium,HorizontalAlignment=AdaptiveHorizontalAlignment.Center,Spacing=AdaptiveSpacing.None}
                                         }
                                    }
                                }
                            },
                             new AdaptiveColumnSet()
                            {
                                Columns=new List<AdaptiveColumn>()
                                {
                                   new AdaptiveColumn()
                                    {
                                         Width=AdaptiveColumnWidth.Auto,
                                         Items=new List<AdaptiveElement>()
                                         {
                                             new AdaptiveTextBlock(){Text="Exised  Count is  :  "+Existed_Count,Color=AdaptiveTextColor.Accent,Size=AdaptiveTextSize.Medium,HorizontalAlignment=AdaptiveHorizontalAlignment.Center,Spacing=AdaptiveSpacing.None}
                                         }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var acard = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = AppCountCard
            };
            return acard;
        }

    }
}

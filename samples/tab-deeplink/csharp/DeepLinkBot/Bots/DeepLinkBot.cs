// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class DeepLinkBot : ActivityHandler
    {
        public readonly IConfiguration _configuration;
        public DeepLinkBot(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string channelID = "<AddYourTeamsChannelId>";
        public string teamsUrl = "https://teams.microsoft.com/l/entity/";
        public string tabUrlTask1;
        public string tabUrlTask2;
        public string tabUrlTask3;
        public string callingDeeplink;
        public string extendedDeepLink;

        DeeplinkHelper deeplinkHelper = new DeeplinkHelper();
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var attachment = AdaptiveDeepLinkCard(turnContext.Activity.From.Name, turnContext);
            await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
        public Attachment AdaptiveDeepLinkCard(string userName, ITurnContext turnContext)
        {
            if (turnContext.Activity.Conversation.ConversationType == "channel")
            {
                channelID = turnContext.Activity.Conversation.Id.Split(';')[0];
                tabUrlTask1 = deeplinkHelper.GetDeepLinkToChannelTask(teamsUrl, _configuration["MicrosoftAppId"], _configuration["BaseURL"], channelID, _configuration["ChannelEntityId"], "bot1");
                tabUrlTask2 = deeplinkHelper.GetDeepLinkToChannelTask(teamsUrl, _configuration["MicrosoftAppId"], _configuration["BaseURL"], channelID, _configuration["ChannelEntityId"], "bot2");
                tabUrlTask3 = deeplinkHelper.GetDeepLinkToChannelTask(teamsUrl, _configuration["MicrosoftAppId"], _configuration["BaseURL"], channelID, _configuration["ChannelEntityId"], "bot3");
                extendedDeepLink = deeplinkHelper.GetDeepLinkToChannelTask(teamsUrl, _configuration["MicrosoftAppId"], _configuration["BaseURL"], channelID, _configuration["ChannelEntityId"], "");
            }
            else
            {
                tabUrlTask1 = deeplinkHelper.GetDeepLinkToTabTask(teamsUrl, _configuration["ManifestAppId"], _configuration["TabEntityId"], "topic1");
                tabUrlTask2 = deeplinkHelper.GetDeepLinkToTabTask(teamsUrl, _configuration["ManifestAppId"], _configuration["TabEntityId"], "topic2");
                tabUrlTask3 = deeplinkHelper.GetDeepLinkToTabTask(teamsUrl, _configuration["ManifestAppId"], _configuration["TabEntityId"], "topic3");
                extendedDeepLink = deeplinkHelper.GetDeepLinkToTabTask(teamsUrl, _configuration["ManifestAppId"], _configuration["TabEntityId"], "");
            }

            var DeepLinkCard = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Text = $"Hey {userName}! Please click on below buttons to navigate to a tab!",
                                Size = AdaptiveTextSize.Large,
                                Wrap = true
                            },
                            new AdaptiveColumnSet()
                            {
                                Columns = new List<AdaptiveColumn>()
                                {
                                    new AdaptiveColumn()
                                    {
                                        Width = AdaptiveColumnWidth.Auto,
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveTextBlock()
                                            {
                                                Text = "Bots in Teams",
                                                Color = AdaptiveTextColor.Accent,
                                                Size = AdaptiveTextSize.Medium,
                                                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                                                Spacing = AdaptiveSpacing.None
                                            }
                                        },
                                        SelectAction = new AdaptiveOpenUrlAction()
                                        {
                                            Url = new Uri(tabUrlTask1),
                                            Title = "Bots in Teams"
                                        }
                                    }
                                }
                            },
                            new AdaptiveColumnSet()
                            {
                                Columns = new List<AdaptiveColumn>()
                                {
                                    new AdaptiveColumn()
                                    {
                                        Width = AdaptiveColumnWidth.Auto,
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveTextBlock()
                                            {
                                                Text ="Bot Framework SDK",
                                                Color = AdaptiveTextColor.Accent,
                                                Size = AdaptiveTextSize.Medium,
                                                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                                                Spacing = AdaptiveSpacing.None
                                            }
                                        },
                                        SelectAction = new AdaptiveOpenUrlAction()
                                        {
                                            Url = new Uri(tabUrlTask2),
                                            Title = "Bot Framework SDK"
                                        }
                                    }
                                }
                            },
                            new AdaptiveColumnSet()
                            {
                                Columns = new List<AdaptiveColumn>()
                                {
                                    new AdaptiveColumn()
                                    {
                                        Width = AdaptiveColumnWidth.Auto,
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveTextBlock()
                                            {
                                                Text = "Teams Apps",
                                                Color = AdaptiveTextColor.Accent,
                                                Size = AdaptiveTextSize.Medium,
                                                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                                                Spacing = AdaptiveSpacing.None
                                            }
                                        },
                                        SelectAction = new AdaptiveOpenUrlAction()
                                        {
                                            Url = new Uri(tabUrlTask3),
                                            Title = "Teams Apps"
                                        }
                                    }
                                }
                            },
                            new AdaptiveColumnSet()
                            {
                                Columns = new List<AdaptiveColumn>()
                                {
                                    new AdaptiveColumn()
                                    {
                                        Width = AdaptiveColumnWidth.Auto,
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveTextBlock()
                                            {
                                                Text = "Extended Deeplink features",
                                                Color = AdaptiveTextColor.Accent,
                                                Size = AdaptiveTextSize.Medium,
                                                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                                                Spacing = AdaptiveSpacing.None
                                            }
                                        },
                                        SelectAction = new AdaptiveOpenUrlAction()
                                        {
                                            Url = new Uri(extendedDeepLink),
                                            Title = "Extended Deeplink features"
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
                Content = DeepLinkCard
            };
            return acard;
        }
    }
}

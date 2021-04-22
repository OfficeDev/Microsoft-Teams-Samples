// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class DeepLinkBot : ActivityHandler
    {
        public static string channelID = "";

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Conversation.ConversationType == "channel")
            {
                channelID = turnContext.Activity.Conversation.Id.Split(';')[0];
                var attachment = AdaptiveDeepLinkCard(turnContext.Activity.From.Name, DeepLinkHelperChannel.Task1Deeplink, DeepLinkHelperChannel.Task2Deeplink, DeepLinkHelperChannel.Task3Deeplink);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
            }
            else
            {
                var attachment = AdaptiveDeepLinkCard(turnContext.Activity.From.Name, DeeplinkHelper.Task1Deeplink, DeeplinkHelper.Task2Deeplink, DeeplinkHelper.Task3Deeplink);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
            }
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

        public static Attachment AdaptiveDeepLinkCard(string userName, string deeplink1, string deeplink2, string deeplink3)
        {
            var DeepLinkCard = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer
                    {
                        Items=new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Text=$"Hey {userName}! Please click on below buttons to navigate to a tab!",
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
                                             new AdaptiveTextBlock(){Text="Bots in Teams",Color=AdaptiveTextColor.Accent,Size=AdaptiveTextSize.Medium,HorizontalAlignment=AdaptiveHorizontalAlignment.Center,Spacing=AdaptiveSpacing.None}
                                         },
                                           SelectAction = new AdaptiveOpenUrlAction()
                                         {
                                             Url=new Uri(deeplink1),
                                             Title = "Bots in Teams"
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
                                             new AdaptiveTextBlock(){Text="Bot Framework SDK",Color=AdaptiveTextColor.Accent,Size=AdaptiveTextSize.Medium,HorizontalAlignment=AdaptiveHorizontalAlignment.Center,Spacing=AdaptiveSpacing.None}
                                         },
                                           SelectAction = new AdaptiveOpenUrlAction()
                                         {
                                             Url=new Uri(deeplink2),
                                             Title = "Bot Framework SDK"
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
                                             new AdaptiveTextBlock(){Text="Teams Apps",Color=AdaptiveTextColor.Accent,Size=AdaptiveTextSize.Medium,HorizontalAlignment=AdaptiveHorizontalAlignment.Center,Spacing=AdaptiveSpacing.None}
                                         },
                                           SelectAction = new AdaptiveOpenUrlAction()
                                         {
                                             Url=new Uri(deeplink3),
                                             Title = "Teams Apps"
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
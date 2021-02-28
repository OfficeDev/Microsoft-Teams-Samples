// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Content_Bubble_Bot
{
    public class ContentBubbleBot : TeamsActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            if (turnContext.Activity.Value == null)
            {
                var path = "./Cards/adaptive.json";
                var adaptiveCardJson = File.ReadAllText(path);
                var adaptiveCardAttachment = new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = JsonConvert.DeserializeObject(adaptiveCardJson),
                };

                await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment));

            }

            else
            {

                string selectedoption = string.Empty;
                JToken commandToken = JToken.Parse(turnContext.Activity.Value.ToString());
                string command = commandToken["action"].Value<string>();

                if (command.ToLowerInvariant() == "inputselector" || command == Constants.FirstTitle || command == Constants.SecondTitle || command == Constants.ThirdTitle)
                {
                    selectedoption = commandToken["myReview"].Value<string>();
                }

                if (selectedoption == Constants.FirstTitle)
                {
                    await FetchTaskFirst(turnContext);

                    var path = "./Cards/AdaptiveFirstTitle.json";
                    var adaptiveCardJson = File.ReadAllText(path);
                    var adaptiveCardAttachment = new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = JsonConvert.DeserializeObject(adaptiveCardJson),
                    };

                    await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment));


                }
                else if (selectedoption == Constants.SecondTitle)
                {
                    await FetchTaskSecond(turnContext);

                    var path = "./Cards/AdaptiveSecondTitle.json";
                    var adaptiveCardJson = File.ReadAllText(path);
                    var adaptiveCardAttachment = new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = JsonConvert.DeserializeObject(adaptiveCardJson),
                    };

                    await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment));


                }
                else if (selectedoption == Constants.ThirdTitle)
                {
                    await FetchTaskThird(turnContext);

                    var path = "./Cards/AdaptiveThirdTitle.json";
                    var adaptiveCardJson = File.ReadAllText(path);
                    var adaptiveCardAttachment = new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = JsonConvert.DeserializeObject(adaptiveCardJson),
                    };

                    await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment));
                }

                else if (selectedoption == "yes")
                {
                    await turnContext.SendActivityAsync(turnContext.Activity.From.Name + " : " + "**Yes** for " + "'" + command + "'");
                }

                else if (selectedoption == "no")
                {
                    await turnContext.SendActivityAsync(turnContext.Activity.From.Name + " : " + "**No** for " + "'" + command + "'");
                }
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

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {

            var replyJson = JObject.Parse(taskModuleRequest.Data.ToString());
            var reply = Convert.ToString(replyJson["userValue"]);
            var title = Convert.ToString(replyJson["title"]);
            if (reply == "yes")
            {

                await turnContext.SendActivityAsync(turnContext.Activity.From.Name + " : " + "**Yes** for " + "'" + title + "'");
            }
            else
            {
                await turnContext.SendActivityAsync(turnContext.Activity.From.Name + " : " + "**No** for " + "'" + title + "'");

            }
            return null;
        }

        public async static Task FetchTaskFirst(ITurnContext turnContext)
        {
            Activity activity = MessageFactory.Text("**Please provide your valuable feedback**");

            activity.ChannelData = new TeamsChannelData
            {
                Notification = new NotificationInfo()
                {
                    AlertInMeeting = true,

                    ExternalResourceUrl = "https://teams.microsoft.com/l/bubble/<<APP-ID>>?url=<<BASE-URL>>/FirstPage&height=<HEIGHT>&width=<WIDTH>&title=ContentBubble&completionBotId=<<BOT-ID>>"
                }
            };
            await turnContext.SendActivityAsync(activity).ConfigureAwait(false);
        }


        public async static Task FetchTaskSecond(ITurnContext turnContext)
        {


            Activity activity = MessageFactory.Text("**Please provide your valuable feedback**");

            activity.ChannelData = new TeamsChannelData
            {
                Notification = new NotificationInfo()
                {
                    AlertInMeeting = true,

                    ExternalResourceUrl = "https://teams.microsoft.com/l/bubble/<<APP-ID>>?url=<<BASE-URL>>/SecondPage&height=<Height>&width=<WIDTH>&title=ContentBubble&completionBotId=<<BOT-ID>>"
                }
            };
            await turnContext.SendActivityAsync(activity).ConfigureAwait(false);
        }


        public async static Task FetchTaskThird(ITurnContext turnContext)
        {

            Activity activity = MessageFactory.Text("**Please provide your valuable feedback**");

            activity.ChannelData = new TeamsChannelData
            {
                Notification = new NotificationInfo()
                {
                    AlertInMeeting = true,

                    ExternalResourceUrl = "https://teams.microsoft.com/l/bubble/<<APP-ID>>?url=<<BASE-URL>>/ThirdPage&height=<HEIGHT>&width=<WIDTH>&title=ContentBubble&completionBotId=<<BOT-ID>>"
                }
            };
            await turnContext.SendActivityAsync(activity).ConfigureAwait(false);
        }
    }
}

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
                await FetchTask(turnContext);

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

                if (command.ToLowerInvariant() == "inputselector")
                {
                    selectedoption = commandToken["myReview"].Value<string>();
                }

                if (selectedoption == "Yes")
                {
                    await turnContext.SendActivityAsync("Sure.. Please check out this link: [Apps in Teams meetings](https://docs.microsoft.com/en-us/microsoftteams/platform/apps-in-teams-meetings/teams-apps-in-meetings#:~:text=Post%2Dmeeting%20app%20experience&text=%E2%9C%94%20Permissioned%20users%20can%20add%20apps%20from%20the%20tab%20gallery,than%20ten%20polls%20or%20surveys.)");
                }
                else
                {
                    await turnContext.SendActivityAsync("We'll provide more interesting topics.. Thank you!");

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
            if (reply == "yes")
            {
                await turnContext.SendActivityAsync("Sure.. Please check out this link: [Apps in Teams meetings](https://docs.microsoft.com/en-us/microsoftteams/platform/apps-in-teams-meetings/teams-apps-in-meetings#:~:text=Post%2Dmeeting%20app%20experience&text=%E2%9C%94%20Permissioned%20users%20can%20add%20apps%20from%20the%20tab%20gallery,than%20ten%20polls%20or%20surveys.)");
            }
            else
            {
                await turnContext.SendActivityAsync("We'll provide more interesting topics.. Thank you!");
            }
            return null;
        }

        public async static Task FetchTask(ITurnContext turnContext)
        {
            Activity activity = MessageFactory.Text("Hello");

            activity.ChannelData = new TeamsChannelData
            {
                Notification = new NotificationInfo()
                {
                    AlertInMeeting = true,

                    ExternalResourceUrl = "https://teams.microsoft.com/l/bubble/<APP-ID>?url=<URL>&height=<Height>&width=<Width>&title=<Title>&completionBotId=<BOT-ID>"
                }
            };
            await turnContext.SendActivityAsync(activity).ConfigureAwait(false);
        }
    }
}

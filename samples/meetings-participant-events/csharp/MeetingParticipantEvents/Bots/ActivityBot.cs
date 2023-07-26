// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MeetingEvents.Bots
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using AdaptiveCards;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ActivityBot : TeamsActivityHandler
    {

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type == "event" && turnContext.Activity.Name == "application/vnd.microsoft.meetingParticipantJoin" || turnContext.Activity.Name == "application/vnd.microsoft.meetingParticipantLeave")
            {
                JObject value = JsonConvert.DeserializeObject<JObject>(turnContext.Activity.Value.ToString());
                if (value["members"] == null)
                {
                    return;
                }
                JObject user = JsonConvert.DeserializeObject<JObject>(value["members"]["user"].ToString());
                string userName = user["name"].ToString();
                if (turnContext.Activity.Name == "application/vnd.microsoft.meetingParticipantJoin")
                {
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(createAdaptiveCardInvokeResponseAsync(userName, "has joined the meeting")));
                }
                if (turnContext.Activity.Name == "application/vnd.microsoft.meetingParticipantLeave")
                {
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(createAdaptiveCardInvokeResponseAsync(userName, "left the meeting")));
                }
            }
           
        }

        private Attachment createAdaptiveCardInvokeResponseAsync(string userName, string action)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = userName + action,
                        Weight = AdaptiveTextWeight.Default,
                        Spacing = AdaptiveSpacing.Medium,
                    }
                }
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}

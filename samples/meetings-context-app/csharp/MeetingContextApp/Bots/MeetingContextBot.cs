// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.15.2

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MeetingContext.Bots
{
    public class MeetingContextBot : TeamsActivityHandler
    {
        public const string commandString = "Please use one of these two commands: **Meeting Context** or **Participant Context**";
        
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text != null)
            {
                var text = turnContext.Activity.RemoveRecipientMention();
                if (text.ToLower().Contains("participant context"))
                {
                    var channelDataObject = JObject.Parse(JsonConvert.SerializeObject(turnContext.Activity.ChannelData));

                    var tenantId = channelDataObject.tenant["id"].ToString();
                    var metingId = channelDataObject.meeting["id"].ToString();
                    var participantId = turnContext.Activity.From.AadObjectId;

                    // GetMeetingParticipant
                    TeamsMeetingParticipant participantDetails = await TeamsInfo.GetMeetingParticipantAsync(turnContext, metingId, participantId, tenantId).ConfigureAwait(false);

                    var formattedString = this.GetFormattedSerializeObject(participantDetails);

                    await turnContext.SendActivityAsync(MessageFactory.Text(formattedString), cancellationToken);
                }
                else if (text.ToLower().Contains("meeting context"))
                {
                    MeetingInfo meetingInfo = await TeamsInfo.GetMeetingInfoAsync(turnContext);

                    var formattedString = this.GetFormattedSerializeObject(meetingInfo);

                    await turnContext.SendActivityAsync(MessageFactory.Text(formattedString), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(commandString), cancellationToken);
                }
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and Welcome!";

            await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText), cancellationToken);
            await turnContext.SendActivityAsync(MessageFactory.Text(commandString), cancellationToken);
        }

        /// <summary>
        /// Gets the serialize formatted object string.
        /// </summary>
        /// <param name="obj">Incoming object needs to be formatted.</param>
        /// <returns>Formatted string.</returns>
        private string GetFormattedSerializeObject (object obj)
        {
            var formattedString = "";
            foreach (var meetingDetails in obj.GetType().GetProperties())
            {
                var detail = meetingDetails.GetValue(obj, null);
                var block = $"<b>{meetingDetails.Name}:</b> <br>";
                var storeTemporaryFormattedString = "";

                if (detail != null)
                {
                    if (detail.GetType().Name != "String")
                    {
                        foreach (var value in detail.GetType().GetProperties())
                        {
                            storeTemporaryFormattedString += $" <b> &nbsp;&nbsp;{value.Name}:</b> {value.GetValue(detail, null)}<br/>";
                        }
                        
                        formattedString += block + storeTemporaryFormattedString;
                        storeTemporaryFormattedString = String.Empty;
                    }
                }
            }

            return formattedString;
        }
    }
}

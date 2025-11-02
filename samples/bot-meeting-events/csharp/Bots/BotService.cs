// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.13.2

using MeetingEventsCallingBot.Authentication;
using MeetingEventsCallingBot.Model;
using MeetingEventsCallingBot.Utility;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Calls;
using Microsoft.Graph.Communications.Client;
using Microsoft.Graph.Communications.Common.Telemetry;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MeetingEventsCallingBot.Bots
{
    public class BotService : ActivityHandler
    {
        public ICommunicationsClient CommunicationsClient { get; private set; }

        public BotService(BotOptions options, IGraphLogger graphLogger)
        {
            var name = this.GetType().Assembly.GetName().Name;
            var builder = new CommunicationsClientBuilder(
                name,
                options.AppId,
                graphLogger);

            var authProvider = new AuthenticationProvider(
                name,
                options.AppId,
                options.AppSecret,
                graphLogger);

            builder.SetAuthenticationProvider(authProvider);
            builder.SetNotificationUrl(options.CallControlBaseUrl);
            builder.SetServiceBaseUrl(options.PlaceCallEndpointUrl);

            this.CommunicationsClient = builder.Build();

        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyText = $"Echo: {turnContext.Activity.Text}";
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == "event")
            {
                var meetingEventInfo = turnContext.Activity.Value as JObject;
                var meetingEventInfoObject = meetingEventInfo.ToObject<MeetingStartEndEventValue>();

                if (meetingEventInfoObject.StartTime != null)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Meeting Started ..."));
                    await this.JoinCallAsync(meetingEventInfoObject.JoinUrl);
                } 
                else if (meetingEventInfoObject.EndTime != null)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Meeting Ended ..."));
                }
            }
        }

        /// <summary>
        /// Joins bot to the call 
        /// </summary>
        /// <param name="JoinURL">Meeting join url</param>
        /// <returns>ICall object</returns>
        public async Task<ICall> JoinCallAsync(string JoinURL)
        {
            Microsoft.Graph.MeetingInfo meetingInfo;
            ChatInfo chatInfo;
            (chatInfo, meetingInfo) = JoinInfo.ParseJoinURL(JoinURL);

            var tenantId = (meetingInfo as OrganizerMeetingInfo)?.Organizer.GetPrimaryIdentity()?.GetTenantId();

            var joinParams = new JoinMeetingParameters(chatInfo, meetingInfo, new[] { Modality.Audio })
            {
                TenantId = tenantId,
            };

            var scenarioId = Guid.NewGuid();

            var statefulCall = await this.CommunicationsClient.Calls().AddAsync(joinParams, scenarioId).ConfigureAwait(false);

            return statefulCall;
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CallingBotSample.Authentication;
using CallingBotSample.Options;
using CallingBotSample.Services.MicrosoftGraph;
using CallingBotSample.Utility;
using CallingMeetingBot.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Client.Authentication;
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.Graph.Communications.Core.Notifications;
using Microsoft.Graph.Communications.Core.Serialization;

namespace CallingBotSample.Bots
{
    public class CallingBot : ActivityHandler
    {
        // TODO: What does GraphLogger provide?
        private readonly IGraphLogger graphLogger;
        private readonly IRequestAuthenticationProvider authenticationProvider;
        private readonly INotificationProcessor notificationProcessor;
        private readonly CommsSerializer serializer;
        private readonly BotOptions botOptions;
        private readonly ICallService callService;
        private readonly AudioRecordingConstants audioRecordingConstants;
        private readonly IMemoryCache callBotCache;
        private readonly ILogger<CallingBot> logger;

        public CallingBot(
        ICallService callService,
        AudioRecordingConstants audioRecordingConstants,
        IGraphLogger graphLogger,
        IMemoryCache callBotCache,
        IOptions<BotOptions> botOptions,
        ILogger<CallingBot> logger)
        {
            this.botOptions = botOptions.Value;
            this.callService = callService;
            this.audioRecordingConstants = audioRecordingConstants;
            this.graphLogger = graphLogger;
            this.callBotCache = callBotCache;
            this.logger = logger;

            var name = this.GetType().Assembly.GetName().Name;
            authenticationProvider = new AuthenticationProvider(name, this.botOptions.AppId, this.botOptions.AppSecret, graphLogger);

            serializer = new CommsSerializer();
            notificationProcessor = new NotificationProcessor(serializer);
            notificationProcessor.OnNotificationReceived += this.NotificationProcessor_OnNotificationReceived;
        }

        /// <summary>
        /// Process "/callback" notifications asynchronously.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task ProcessNotificationAsync(
            HttpRequest request,
            HttpResponse response)
        {
            try
            {
                var httpRequest = request.CreateRequestMessage();
                var results = await authenticationProvider.ValidateInboundRequestAsync(httpRequest).ConfigureAwait(false);
                if (results.IsValid)
                {
                    var httpResponse = await notificationProcessor.ProcessNotificationAsync(httpRequest).ConfigureAwait(false);
                    await httpResponse.CreateHttpResponseAsync(response).ConfigureAwait(false);
                }
                else
                {
                    response.StatusCode = StatusCodes.Status403Forbidden;
                }
            }
            catch (Exception e)
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await response.WriteAsync(e.ToString()).ConfigureAwait(false);
            }
        }

        private void NotificationProcessor_OnNotificationReceived(NotificationEventArgs args)
        {
            _ = NotificationProcessor_OnNotificationReceivedAsync(args).ForgetAndLogExceptionAsync(
              graphLogger,
              $"Error processing notification {args.Notification.ResourceUrl} with scenario {args.ScenarioId}");
        }

        private async Task NotificationProcessor_OnNotificationReceivedAsync(NotificationEventArgs args)
        {
            // Should look to run async as not to block subsequent notifications.
            // https://microsoftgraph.github.io/microsoft-graph-comms-samples/docs/articles/index.html#answer-incoming-call-with-service-hosted-media

            graphLogger.CorrelationId = args.ScenarioId;
            var callId = GetCallIdFromNotification(args);

            if (args.ResourceData is Call call)
            {

                if (args.ChangeType == ChangeType.Created && call.State == CallState.Incoming)
                {
                    await callService.Answer(callId, audioRecordingConstants.Speech, audioRecordingConstants.PleaseRecordYourMessage);
                }
                else if (
                    args.ChangeType == ChangeType.Updated
                    && call.State == CallState.Established)
                {
                    // Some scenarios fire two CallState.Established events. The use of a cache ensures we only play the prompt once on meeting join
                    string key = $"{callId}:established";
                    if (!callBotCache.Get<bool>(key))
                    {
                        callBotCache.Set(key, true);
                        await callService.PlayPrompt(callId, audioRecordingConstants.Speech);
                    }
                }
            }
            else if (args.ChangeType == ChangeType.Updated &&
                args.Notification.ResourceUrl.Contains("/participants") &&
                args.ResourceData is object[] objs)
            {
                string key = $"{callId}:atLeastOneUserJoined";
                Participant[] participants = Array.ConvertAll(objs, (object obj) => (Participant)obj);

                if (participants.Length > 0)
                {
                    bool atLeastOneUserJoined = callBotCache.Get<bool>(key);

                    if (!atLeastOneUserJoined && participants.Any(p => p.Info.Identity.User != null))
                    {
                        callBotCache.Set(key, true);
                    }

                    // If there is only one participant remaining, and it's this application, and at least one user has joined at some point, hang up
                    if (participants.Length == 1 &&
                        participants[0]?.Info?.Identity?.Application?.Id == botOptions.AppId &&
                        atLeastOneUserJoined)
                    {
                        await callService.HangUp(callId);
                        return;
                    }
                }
            }
        }

        private string GetCallIdFromNotification(NotificationEventArgs notificationArgs)
        {
            if (notificationArgs.ResourceData is CommsOperation operation && !string.IsNullOrEmpty(operation.ClientContext))
            {
                return operation.ClientContext;
            }

            // Resource URLs are in the format below, with the call id in the 3rd postion (position 0 will be empty)
            // #microsoft.graph.call: /communications/calls/<<call-id-as-guid>>
            // #microsoft.graph.recordOperation: /communications/calls/<<call-id-as-guid>>/operations/<<operation-id-as-guid>>
            return notificationArgs.Notification.ResourceUrl.Split('/')[3];
        }
    }
}

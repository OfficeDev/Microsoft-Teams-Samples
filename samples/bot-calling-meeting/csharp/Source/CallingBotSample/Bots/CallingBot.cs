// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CallingBotSample.Authentication;
using CallingBotSample.Cache;
using CallingBotSample.Models;
using CallingBotSample.Options;
using CallingBotSample.Services.BotFramework;
using CallingBotSample.Services.CognitiveServices;
using CallingBotSample.Services.MicrosoftGraph;
using CallingBotSample.Services.TeamsRecordingService;
using CallingBotSample.Utility;
using CallingMeetingBot.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
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
        private readonly ITeamsRecordingService teamsRecordingService;
        private readonly ICallCache callCache;
        private readonly IIncidentCache incidentCache;
        private readonly ISpeechService speechService;
        private readonly IBotService botService;
        private readonly ILogger<CallingBot> logger;

        public CallingBot(
            ICallService callService,
            AudioRecordingConstants audioRecordingConstants,
            ITeamsRecordingService teamsRecordingService,
            IGraphLogger graphLogger,
            ICallCache callCache,
            IIncidentCache incidentCache,
            ISpeechService speechService,
            IBotService botService,
            IOptions<BotOptions> botOptions,
            ILogger<CallingBot> logger)
        {
            this.botOptions = botOptions.Value;
            this.callService = callService;
            this.audioRecordingConstants = audioRecordingConstants;
            this.teamsRecordingService = teamsRecordingService;
            this.graphLogger = graphLogger;
            this.callCache = callCache;
            this.incidentCache = incidentCache;
            this.speechService = speechService;
            this.botService = botService;
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
            graphLogger.CorrelationId = args.ScenarioId;
            var callId = GetCallIdFromNotification(args);

            if (args.ResourceData is Call call)
            {
                // If the notification is a newly created, incoming call, answer it
                if (args.ChangeType == ChangeType.Created && call.State == CallState.Incoming)
                {
                    await callService.Answer(
                        callId,
                        new List<MediaInfo>
                        {
                            audioRecordingConstants.Speech,
                            audioRecordingConstants.PleaseRecordYourMessage
                        });
                }
                // If the notification is established (answered), fire a recording prompt
                else if (
                    args.ChangeType == ChangeType.Updated
                    && call.State == CallState.Established)
                {
                    // Some scenarios fire two CallState.Established events. The use of a cache ensures we only play the prompt once on meeting join
                    // This works by keeping track of when the record prompt on meeting established is sent, and if another notifications comes in
                    // we do not send the prompt any more.
                    if (!callCache.GetIsEstablished(callId))
                    {
                        callCache.SetIsEstablished(callId);
                        await callService.Record(callId, audioRecordingConstants.PleaseRecordYourMessage);
                    }

                    if (incidentCache.TryGetValue(callId, out IncidentDetails incidentDetails))
                    {
                        await callService.InviteParticipant(callId, incidentDetails.Participants.Select(p => new IdentitySet { User = p }));
                    }
                }
            }
            // If the notification is a recording, download from the Teams Recording Service, and then echo the audio back to the call
            // We also convert the recording to text and send it to the meeting chat.
            else if (args.ResourceData is RecordOperation recording)
            {
                if (recording.ResultInfo.Code >= 400)
                {
                    return;
                }

                var recordingLocation = await teamsRecordingService.DownloadRecording(recording.RecordingLocation, recording.RecordingAccessToken);

                // Here we are transcribing the recording and sending it as a message in the call chat.
                try
                {
                    var callDetails = callService.Get(callId);
                    var threadId = (await callDetails)?.ChatInfo?.ThreadId;

                    // Calls initiated in a 1:1 chat with the bot do not have a threadId
                    if (threadId != null)
                    {
                        var result = await speechService.ConvertWavToText(recordingLocation);

                        if (result != null)
                        {
                            await botService.SendToConversation($"You said: {result}", threadId);
                        }
                        else
                        {
                            await botService.SendToConversation($"Sorry, something went wrong while trying to transcribe your recording.", threadId);
                        }
                    }
                }
                catch (ErrorResponseException ex)
                {
                    logger.LogError(ex, "Failure while sending a message to the coversation. The app might not be installed in the conversation scope.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failure converting speech to text.");
                }

                await callService.PlayPrompt(
                    callId,
                    new List<MediaInfo>
                    {
                        new MediaInfo {
                            // This URL needs to be publicly accessible, so Microsoft Teams can play the audio.
                            // In a production environment, you might want to consider a better location than
                            // this server's content directory.
                            Uri = new Uri(botOptions.BotBaseUrl, recordingLocation).ToString(),
                            ResourceId = Guid.NewGuid().ToString(),
                        }
                    });
            }
            // If the notification is a play prompt operation, we should check if the prompt is temporary and delete the file
            else if (args.ResourceData is PlayPromptOperation playPromptOperation)
            {
                if (playPromptOperation.ResultInfo.Code >= 400 ||
                    playPromptOperation.Status != OperationStatus.Completed)
                {
                    return;
                }

                if (playPromptOperation.AdditionalData.TryGetValue("prompts", out object? obj))
                {
                    object[] objs = obj as object[] ?? new object[0];
                    MediaPrompt[] prompts = Array.ConvertAll(objs, (object o) => (MediaPrompt)o);

                    foreach (MediaPrompt prompt in prompts)
                    {
                        if (prompt.MediaInfo.Uri.Contains(botOptions.RecordingDownloadDirectory) &&
                            botOptions.BotBaseUrl != null &&
                            prompt.MediaInfo.Uri.StartsWith(botOptions.BotBaseUrl.ToString()))
                        {
                            var relativeRecordingPath = prompt.MediaInfo.Uri.Substring(botOptions.BotBaseUrl.ToString().Length);

                            // If this deletion attempt fails we do not reattempt. If you implement this pattern you should improve the resiliency of this call.
                            teamsRecordingService.DeleteRecording(relativeRecordingPath);
                        }
                    }
                }
            }
            // If the notification is participants change, keep track of if a User has joined the call at some point
            // if at least one user has joined, and only the bot is remaining in the call, get the bot to hang up.
            // This ensures that the Bot doesn't keep a call active for a long period of time.
            else if (args.IsParticipantsNotification() && args.ResourceData is object[] objs)
            {
                Participant[] participants = Array.ConvertAll(objs, (object obj) => (Participant)obj);

                if (participants.Length > 0)
                {
                    bool atLeastOneUserJoined = callCache.GetAtLeastOneUserJoined(callId);

                    if (!atLeastOneUserJoined && participants.Any(p => p.Info.Identity.User != null))
                    {
                        callCache.SetAtLeastOneUserJoined(callId);

                        if (incidentCache.TryGetValue(callId, out IncidentDetails incidentDetails))
                        {
                            string? textToSpeechRecordingLocation = await speechService.ConvertTextToSpeech(
                                $"There is an ongoing incident for {incidentDetails.IncidentSubject}. Your assistance is required.");

                            if (textToSpeechRecordingLocation != null)
                            {
                                await callService.PlayPrompt(
                                    callId,
                                    new List<MediaInfo>
                                    {
                                        new MediaInfo
                                        {
                                            Uri = new Uri(botOptions.BotBaseUrl, textToSpeechRecordingLocation).ToString(),
                                            ResourceId = Guid.NewGuid().ToString(),
                                    } });
                            }
                        }
                        else
                        {
                            // Play the record prompt only when the first user joins the call
                            await callService.Record(callId, audioRecordingConstants.PleaseRecordYourMessage);
                        }
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

            // Resource URLs are in the format below, with the call id in the 3rd position (position 0 will be empty)
            // #microsoft.graph.call: /communications/calls/<<call-id-as-guid>>
            // #microsoft.graph.recordOperation: /communications/calls/<<call-id-as-guid>>/operations/<<operation-id-as-guid>>
            return notificationArgs.Notification.ResourceUrl.Split('/')[3];
        }
    }
}

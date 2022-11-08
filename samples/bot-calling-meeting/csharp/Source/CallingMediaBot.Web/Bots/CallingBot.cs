// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingMediaBot.Web.Bots;
using System.Net;
using System.Text.Json;
using Antlr4.Runtime.Misc;
using CallingMediaBot.Web.Extensions;
using CallingMediaBot.Web.Options;
using CallingMediaBot.Web.Services.MicrosoftGraph;
using CallingMediaBot.Web.Services.TeamsRecordingService;
using CallingMediaBot.Web.Utility;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Client.Authentication;
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.Graph.Communications.Core.Notifications;
using Microsoft.Graph.Communications.Core.Serialization;

public class CallingBot : ActivityHandler
{
    // TODO: What does GraphLogger provide?
    private readonly IGraphLogger graphLogger;
    private readonly IRequestAuthenticationProvider authenticationProvider;
    private readonly INotificationProcessor notificationProcessor;
    private readonly CommsSerializer serializer;
    private readonly BotOptions botOptions;
    private readonly ICallService callService;
    private readonly ITeamsRecordingService teamsRecordingService;
    private readonly ILogger<CallingBot> logger;

    private readonly IHttpClientFactory httpClientFactory;

    public CallingBot(ICallService callService, ITeamsRecordingService teamsRecordingService, IGraphLogger graphLogger, IOptions<BotOptions> botOptions, ILogger<CallingBot> logger, IHttpClientFactory httpClientFactory)
    {
        this.botOptions = botOptions.Value;
        this.callService = callService;
        this.teamsRecordingService = teamsRecordingService;
        this.graphLogger = graphLogger;
        this.logger = logger;

        var name = this.GetType().Assembly.GetName().Name;
        authenticationProvider = new AuthenticationProvider(name, this.botOptions.AppId, this.botOptions.AppSecret, graphLogger);

        serializer = new CommsSerializer();
        notificationProcessor = new NotificationProcessor(serializer);
        notificationProcessor.OnNotificationReceived += this.NotificationProcessor_OnNotificationReceived;

        this.httpClientFactory = httpClientFactory;
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
          graphLogger, logger,
          $"Error processing notification {args.Notification.ResourceUrl} with scenario {args.ScenarioId}");
    }

    private async Task NotificationProcessor_OnNotificationReceivedAsync(NotificationEventArgs args)
    {
        // Should look to run async as not to block subsequent notifications.
        // https://microsoftgraph.github.io/microsoft-graph-comms-samples/docs/articles/index.html#answer-incoming-call-with-service-hosted-media

        logger.LogInformation($"Notification: ${JsonSerializer.Serialize(args)}");

        graphLogger.CorrelationId = args.ScenarioId;
        if (args.ResourceData is Call call)
        {
            if (args.ChangeType == ChangeType.Created && call.State == CallState.Incoming)
            {
                await AnswerIncomingCallAsync(call.Id, args.TenantId, args.ScenarioId).ConfigureAwait(false); ;
            }
            else if (args.ChangeType == ChangeType.Updated && call.State == CallState.Established)
            {
                //await callService.PlayPrompt(GetCallIdFromNotification(args), new MediaInfo
                //{
                //    Uri = new Uri(botOptions.BotBaseUrl, "audio/speech.wav").ToString(),
                //    ResourceId = Guid.NewGuid().ToString(),
                //});
                var recordYourMessageAudio = new MediaInfo
                {
                    Uri = new Uri(botOptions.BotBaseUrl, "audio/please-record-your-message.wav").ToString(),
                    ResourceId = Guid.NewGuid().ToString(),
                };
                var result = await callService.Record(GetCallIdFromNotification(args), recordYourMessageAudio);
            }
            else
            {
                logger.LogInformation($"ChangeType: {args.ChangeType}; State: {call.State}; ");
            }
        }
        else if (args.ResourceData is RecordOperation recording)
        {
            if (recording.ResultInfo.Code >= 400)
            {
                return;
            }

            var recordingLocation = await teamsRecordingService.DownloadRecording(recording.RecordingLocation, recording.RecordingAccessToken);

            await callService.PlayPrompt(
                GetCallIdFromNotification(args),
                new MediaInfo
                {
                    Uri = new Uri(botOptions.BotBaseUrl, recordingLocation).ToString(),
                    ResourceId = Guid.NewGuid().ToString(),
                });
        }
        else
        {
            logger.LogInformation($"ChangeType: {args.ChangeType}; ResourceData: {args.ResourceData}; ");
        }
    }

    private async Task AnswerIncomingCallAsync(string callId, string tenantId, Guid scenarioId)
    {
        var speechAudio = new MediaInfo
        {
            Uri = new Uri(botOptions.BotBaseUrl, "audio/speech.wav").ToString(),
            ResourceId = Guid.NewGuid().ToString(),
        };
        var recordYourMessageAudio = new MediaInfo
        {
            Uri = new Uri(botOptions.BotBaseUrl, "audio/please-record-your-message.wav").ToString(),
            ResourceId = Guid.NewGuid().ToString(),
        };

        await callService.Answer(callId, speechAudio, recordYourMessageAudio);
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

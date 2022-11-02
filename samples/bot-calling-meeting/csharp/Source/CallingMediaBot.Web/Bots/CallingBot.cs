// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CallingMediaBot.Domain.Interfaces;
using CallingMediaBot.Web.Options;
using CallingMediaBot.Web.Utility;
using CallingMeetingBot.Extensions;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Client.Authentication;
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.Graph.Communications.Core.Notifications;
using Microsoft.Graph.Communications.Core.Serialization;
using System.Net;

namespace CallingMediaBot.Web.Bots;

public class CallingBot : ActivityHandler
{
    // TODO: What does GraphLogger provide?
    private readonly IGraphLogger graphLogger;
    private readonly IRequestAuthenticationProvider authenticationProvider;
    private readonly INotificationProcessor notificationProcessor;
    private readonly CommsSerializer serializer;
    private readonly BotOptions botOptions;
    private readonly ICallService callService;
    private readonly ILogger<CallingBot> logger;

    public CallingBot(ICallService callService, IGraphLogger graphLogger, IOptions<BotOptions> botOptions, ILogger<CallingBot> logger)
    {
        this.botOptions = botOptions.Value;
        this.callService = callService;
        this.graphLogger = graphLogger;
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
        if (args.ResourceData is Call call)
        {
            if (args.ChangeType == ChangeType.Created && call.State == CallState.Incoming)
            {
                AnswerIncomingCallAsync(call.Id, args.TenantId, args.ScenarioId);
            }
            else if (args.ChangeType == ChangeType.Updated && call.State == CallState.Established)
            {
                await callService.PlayPrompt(args.Notification.ResourceUrl.Split('/').Last(), new MediaInfo
                {
                    Uri = new Uri(botOptions.BotBaseUrl, "audio/speech.wav").ToString(),
                    ResourceId = Guid.NewGuid().ToString(),
                });
            }
        }
    }

    private void AnswerIncomingCallAsync(string callId, string tenantId, Guid scenarioId)
    {
        // Run async as not to block subsequent notifications.
        // https://microsoftgraph.github.io/microsoft-graph-comms-samples/docs/articles/index.html#answer-incoming-call-with-service-hosted-media
        Task.Run(async () =>
        {
            var resourceId = Guid.NewGuid().ToString();

            await callService.Answer(callId);

            await callService.PlayPrompt(callId, new MediaInfo
            {
                Uri = new Uri(botOptions.BotBaseUrl, "audio/speech.wav").ToString(),
                ResourceId = resourceId,
            });
        });
    }
}

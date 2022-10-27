// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CallingMediaBot.Domain.Factories;
using CallingMediaBot.Web.Interfaces;
using CallingMediaBot.Web.Options;
using CallingMediaBot.Web.Utility;
using CallingMeetingBot.Extensions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Calls;
using Microsoft.Graph.Communications.Client.Authentication;
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.Graph.Communications.Core.Notifications;
using Microsoft.Graph.Communications.Core.Serialization;
using System.Net;
using System.Text.Json;

namespace CallingMediaBot.Web.Bots;

public class CallingBot : ActivityHandler
{
    // TODO: What does GraphLogger provide?
    public IGraphLogger GraphLogger { get; }
    private IRequestAuthenticationProvider AuthenticationProvider { get; }
    private INotificationProcessor NotificationProcessor { get; }
    private CommsSerializer Serializer { get; }
    private readonly BotOptions options;
    private readonly IGraph graph;
    private readonly GraphServiceClient graphServiceClient;
    private readonly ILogger<CallingBot> logger;

    public CallingBot(BotOptions options, IGraph graph, GraphServiceClient graphServiceClient, IGraphLogger graphLogger, ILogger<CallingBot> logger)
    {
        this.options = options;
        this.graph = graph;
        this.graphServiceClient = graphServiceClient;
        this.GraphLogger = graphLogger;
        this.logger = logger;

        var name = this.GetType().Assembly.GetName().Name;
        this.AuthenticationProvider = new AuthenticationProvider(name, options.AppId, options.AppSecret, graphLogger);

        this.Serializer = new CommsSerializer();
        this.NotificationProcessor = new NotificationProcessor(Serializer);
        this.NotificationProcessor.OnNotificationReceived += this.NotificationProcessor_OnNotificationReceived;
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
            var results = await this.AuthenticationProvider.ValidateInboundRequestAsync(httpRequest).ConfigureAwait(false);
            if (results.IsValid)
            {
                var httpResponse = await this.NotificationProcessor.ProcessNotificationAsync(httpRequest).ConfigureAwait(false);
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
          this.GraphLogger,
          $"Error processing notification {args.Notification.ResourceUrl} with scenario {args.ScenarioId}");
    }

    private async Task NotificationProcessor_OnNotificationReceivedAsync(NotificationEventArgs args)
    {
        this.GraphLogger.CorrelationId = args.ScenarioId;
        if (args.ResourceData is Call call)
        {
            if (args.ChangeType == ChangeType.Created && call.State == CallState.Incoming)
            {
                await this.BotAnswerIncomingCallAsync(call.Id, args.TenantId, args.ScenarioId).ConfigureAwait(false);
            }
            else if (args.ChangeType == ChangeType.Updated && call.State == CallState.Established)
            {
                await graph.PlayPrompt(args.Notification.ResourceUrl.Split('/').Last());
            }
        }
    }

    private async Task BotAnswerIncomingCallAsync(string callId, string tenantId, Guid scenarioId)
    {
        var resourceId = Guid.NewGuid().ToString();

        await this.graphServiceClient.Communications.Calls[callId]
            .Answer(
                callbackUri: new Uri(options.BotBaseUrl, "callback").ToString(),
                mediaConfig: new ServiceHostedMediaConfig
                {
                    PreFetchMedia = new List<MediaInfo>()
                    {
                        new MediaInfo()
                        {
                            Uri = new Uri(options.BotBaseUrl, "audio/speech.wav").ToString(),
                            ResourceId = resourceId,
                        }
                    }
                },
                acceptedModalities: new List<Modality> { Modality.Audio })
        .Request()
        .PostAsync();

        await graphServiceClient.Communications.Calls[callId]
            .PlayPrompt(
                prompts: new List<Microsoft.Graph.Prompt>()
                {
                    new MediaPrompt
                    {
                        MediaInfo = new MediaInfo
                        {
                            Uri = new Uri(options.BotBaseUrl, "audio/speech.wav").ToString(),
                            ResourceId = resourceId,
                        }
                    }
                })
            .Request()
            .PostAsync();
    }
}

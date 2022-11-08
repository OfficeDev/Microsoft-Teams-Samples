// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingMediaBot.Web.Services.MicrosoftGraph;
using CallingMediaBot.Web.Options;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Web;

public class CallService : ICallService
{
    private readonly GraphServiceClient graphServiceClient;
    private readonly AzureAdOptions azureAdOptions;

    private readonly string callbackUri;

    public CallService(GraphServiceClient graphServiceClient, IOptions<AzureAdOptions> azureAdOptions, IOptions<BotOptions> botOptions)
    {
        this.graphServiceClient = graphServiceClient;
        this.azureAdOptions = azureAdOptions.Value;

        callbackUri = new Uri(botOptions.Value.BotBaseUrl, "callback").ToString();
    }

    /// <inheritdoc/>
    public Task Answer(string id, params MediaInfo[]? preFetchMedia)
    {
        return graphServiceClient.Communications.Calls[id]
            .Answer(
                callbackUri: callbackUri,
                mediaConfig: new ServiceHostedMediaConfig
                {
                    PreFetchMedia = preFetchMedia
                },
                acceptedModalities: new List<Modality> { Modality.Audio })
            .Request()
            .PostAsync();
    }

    /// <inheritdoc/>
    public Task<Call> Create(params Identity[] users)
    {
        var call = new Call
        {
            CallbackUri = callbackUri,
            TenantId = azureAdOptions.TenantId,
            Targets = users.Select(user => new InvitationParticipantInfo
            {
                Identity = new IdentitySet
                {
                    User = user
                }
            }),
            RequestedModalities = new List<Modality>()
            {
                Modality.Audio
            },
            MediaConfig = new ServiceHostedMediaConfig
            {
            }
        };

        return graphServiceClient.Communications.Calls
            .Request()
            .WithAppOnly()
            .AddAsync(call);
    }

    /// <inheritdoc/>
    public Task<Call> Get(string id)
    {
        return graphServiceClient.Communications.Calls[id]
            .Request()
            .WithAppOnly()
            .GetAsync();
    }

    /// <inheritdoc/>
    public Task HangUp(string id)
    {
        return graphServiceClient.Communications.Calls[id]
            .Request()
            .WithAppOnly()
            .DeleteAsync();
    }

    /// <inheritdoc />
    public Task<PlayPromptOperation> PlayPrompt(string id, params MediaInfo[] mediaPrompts)
    {
        return graphServiceClient.Communications.Calls[id]
            .PlayPrompt(
                CreatePromptsFromMediaInfos(mediaPrompts),
                clientContext: id)
            .Request()
            .PostAsync();
    }

    /// <inheritdoc/>
    public Task<RecordOperation> Record(
        string id,
        MediaInfo mediaPrompt,
        int maxRecordDurationInSeconds = 10,
        IEnumerable<string>? stopTones = null)
    {
        if (stopTones == null)
        {
            stopTones = new List<string>()
            {
                "#",
                "1",
                "*"
            };
        }

        return graphServiceClient.Communications.Calls[id]
            .RecordResponse(
                CreatePromptsFromMediaInfos(new List<MediaInfo>() { mediaPrompt }),
                bargeInAllowed: null,
                initialSilenceTimeoutInSeconds: null,
                maxSilenceTimeoutInSeconds: null,
                maxRecordDurationInSeconds,
                playBeep: null,
                stopTones,
                clientContext: id)
            .Request()
            .WithAppOnly()
            .PostAsync();

    }

    /// <inheritdoc/>
    public Task<Call> Redirect(string id)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task Reject(string id, RejectReason rejectReason)
    {
        return graphServiceClient.Communications.Calls[id]
            .Reject(rejectReason, null)
            .Request()
            .WithAppOnly()
            .PostAsync();
    }

    /// <inheritdoc/>
    public Task<Call> Reject(string id)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task Transfer(string id, Identity transferIdentity, Identity? transfereeIdentity = null)
    {
        var transferTarget = new InvitationParticipantInfo
        {
            Identity = new IdentitySet
            {
                User = transferIdentity
            },
            AdditionalData = new Dictionary<string, object>()
            {
                {"endpointType", "default"}
            }
        };

        ParticipantInfo? transferee = null;
        if (transfereeIdentity != null)
        {
            if (transfereeIdentity.AdditionalData == null)
            {
                transfereeIdentity.AdditionalData = new Dictionary<string, object>();
            }
            transfereeIdentity.AdditionalData["tenantId"] = azureAdOptions.TenantId;

            transferee = new ParticipantInfo
            {
                Identity = new IdentitySet
                {
                    User = transfereeIdentity
                },
                // ParticipantId = "909c6581-5130-43e9-88f3-fcb3582cde37"
            };
        }

        return graphServiceClient.Communications.Calls[id]
            .Transfer(transferTarget, transferee)
            .Request()
            .WithAppOnly()
            .PostAsync();
    }

    private IEnumerable<Prompt> CreatePromptsFromMediaInfos(IEnumerable<MediaInfo> mediaInfos)
    {
        return mediaInfos.Select(mediaPrompt =>
            new MediaPrompt
            {
                MediaInfo = mediaPrompt
            });
    }
}

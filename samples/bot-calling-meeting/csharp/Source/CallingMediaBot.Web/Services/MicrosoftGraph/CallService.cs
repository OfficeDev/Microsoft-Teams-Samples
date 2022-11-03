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
    public async Task Answer(string id, params MediaInfo[]? preFetchMedia)
    {
        await graphServiceClient.Communications.Calls[id]
            .Answer(
                callbackUri: callbackUri,
                mediaConfig: new ServiceHostedMediaConfig
                {
                    PreFetchMedia = preFetchMedia
                },
                acceptedModalities: new List<Modality> { Modality.Audio })
            .Request()
            .WithAppOnly()
            .PostAsync();
    }

    /// <inheritdoc/>
    public async Task<Call> Create(params Identity[] users)
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

        return await graphServiceClient.Communications.Calls
            .Request()
            .WithAppOnly()
            .AddAsync(call);
    }

    /// <inheritdoc/>
    public async Task<Call> Get(string id)
    {
        return await graphServiceClient.Communications.Calls[id]
            .Request()
            .WithAppOnly()
            .GetAsync();
    }

    /// <inheritdoc/>
    public async Task HangUp(string id)
    {
        await graphServiceClient.Communications.Calls[id]
            .Request()
            .WithAppOnly()
            .DeleteAsync();
    }

    /// <inheritdoc />
    public async Task<PlayPromptOperation> PlayPrompt(string id, params MediaInfo[] mediaPrompts)
    {
        var prompts = mediaPrompts.Select(mediaPrompt =>
            new MediaPrompt
            {
                MediaInfo = mediaPrompt
            });

        return await graphServiceClient.Communications.Calls[id]
            .PlayPrompt(prompts)
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
    public async Task Reject(string id, RejectReason rejectReason)
    {
        await graphServiceClient.Communications.Calls[id]
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
    public async Task Transfer(string id, Identity transferIdentity, Identity? transfereeIdentity = null)
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

        await graphServiceClient.Communications.Calls[id]
            .Transfer(transferTarget, transferee)
            .Request()
            .WithAppOnly()
            .PostAsync();
    }
}

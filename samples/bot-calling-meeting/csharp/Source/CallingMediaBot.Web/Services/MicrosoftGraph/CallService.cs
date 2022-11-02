using CallingMediaBot.Domain.Interfaces;
using CallingMediaBot.Web.Options;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Web;

namespace CallingMediaBot.Web.Services.MicrosoftGraph;

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

    public async Task Answer(string id)
    {
        await graphServiceClient.Communications.Calls[id]
            .Answer(
                callbackUri: callbackUri,
                mediaConfig: new ServiceHostedMediaConfig
                {
                    //PreFetchMedia = new List<MediaInfo>()
                    //{
                    //    new MediaInfo()
                    //    {
                    //        Uri = new Uri(options.BotBaseUrl, "audio/speech.wav").ToString(),
                    //        ResourceId = resourceId,
                    //    }
                    //}
                },
                acceptedModalities: new List<Modality> { Modality.Audio })
            .Request()
            .WithAppOnly()
            .PostAsync();
    }

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

    public async Task<Call> Get(string id)
    {
        return await graphServiceClient.Communications.Calls[id]
            .Request()
            .WithAppOnly()
            .GetAsync();
    }

    public async Task HangUp(string id)
    {
        await graphServiceClient.Communications.Calls[id]
            .Request()
            .WithAppOnly()
            .DeleteAsync();
    }

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

    public Task<Call> Redirect(string id)
    {
        throw new NotImplementedException();
    }

    public async Task Reject(string id, RejectReason rejectReason)
    {
        await graphServiceClient.Communications.Calls[id]
            .Reject(rejectReason, null)
            .Request()
            .WithAppOnly()
            .PostAsync();
    }

    public Task<Call> Reject(string id)
    {
        throw new NotImplementedException();
    }

    public Task Transfer(string id)
    {
        throw new NotImplementedException();
    }
}

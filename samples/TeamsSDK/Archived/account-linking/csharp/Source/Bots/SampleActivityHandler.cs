using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Teams.Samples.AccountLinking.OAuth;
using Microsoft.Teams.Samples.AccountLinking.GitHub;
using Microsoft.AspNetCore.DataProtection;
using System.Web;
using System.Text.Json;

namespace Microsoft.Teams.Samples.AccountLinking.Bots;

/// <summary>
///  The is an example implementation of a bot and query-based messaging extension that perform account linking.
/// 
/// Unlike the tab, the bot/ME derives the user's identity from the payload and not the token. We can do this because
/// the request is verified as coming from the botframework so it is more trustworthy than an unauthenticated request.
/// </summary>
/// <typeparam name="TDialog">The bot dialog type to use for conversations</typeparam>
public sealed class SampleActivityHandler<TDialog> : TeamsActivityHandler where TDialog : Dialog
{
    private readonly ILogger<SampleActivityHandler<TDialog>> _logger;

    private readonly TDialog _dialog;

    private readonly ConversationState _botState;

    private readonly UserState _userState;

    private readonly GitHubServiceClient _gitHubServiceClient;

    private readonly OAuthTokenProvider _oAuthTokenProvider;

    private readonly IDataProtector _dataProtector;

    public SampleActivityHandler(
        ILogger<SampleActivityHandler<TDialog>> logger,
        TDialog dialog,
        OAuthTokenProvider oAuthTokenProvider,
        GitHubServiceClient gitHubServiceClient,
        ConversationState botState,
        UserState userState,
        IDataProtectionProvider dataProtectionProvider) : base()
    {
        _logger = logger;
        _gitHubServiceClient = gitHubServiceClient;
        _oAuthTokenProvider = oAuthTokenProvider;
        _dialog = dialog;
        _botState = botState;
        _userState = userState;
        _dataProtector = dataProtectionProvider.CreateProtector(nameof(SampleActivityHandler<TDialog>));
    }

    protected override async Task OnMessageActivityAsync(
        ITurnContext<IMessageActivity> turnContext,
        CancellationToken cancellationToken)
    {
        await _dialog.RunAsync(turnContext, _botState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
    }

    public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
    {
        await base.OnTurnAsync(turnContext, cancellationToken);

        _logger.LogInformation("Saving state");
        // Save any state changes that might have occurred during the turn.
        await _botState.SaveChangesAsync(turnContext, force: false, cancellationToken: cancellationToken);
        await _userState.SaveChangesAsync(turnContext, force: false, cancellationToken: cancellationToken);
    }

    protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(
        ITurnContext<IInvokeActivity> turnContext,
        MessagingExtensionQuery query,
        CancellationToken cancellationToken)
    {
        var userId = turnContext.Activity.From.AadObjectId;
        var tenantId = turnContext.Activity.Conversation.TenantId;

        if (!string.IsNullOrEmpty(query.State))
        {
            var authResponseObject = JsonSerializer.Deserialize<AuthResponse>(query.State);
            if (authResponseObject == default)
            {
                _logger.LogWarning("Invalid state object provided: {state}", query.State);
                throw new Exception("Invalid state format");
            }
            _logger.LogInformation("Params:\nState: {state}\nCode: {code}", authResponseObject.State, authResponseObject.AccountLinkingState);
            var codeVerifier = _dataProtector.Unprotect(authResponseObject.State);
            await _oAuthTokenProvider.ClaimTokenAsync(
                accountLinkingToken: authResponseObject.AccountLinkingState, // these are inverted because
                codeVerifier: codeVerifier,
                tenantId: tenantId,
                userId: userId);
        }
        
        // Attempt to retrieve the github token
        var tokenResult = await _oAuthTokenProvider.GetAccessTokenAsync(tenantId: tenantId, userId: userId);

        if (tokenResult is NeedsConsentResult needsConsentResult)
        {
            _logger.LogInformation("Messaging Extension query with no GitHub token, sending login prompt");
            var (codeChallenge, codeVerifier) = Pkce.GeneratePkceCodes();
            var queryParams = HttpUtility.ParseQueryString(needsConsentResult.AuthorizeUri.Query);
            queryParams.Add("state", _dataProtector.Protect(codeVerifier));
            queryParams.Add("code_challenge", codeChallenge);
            var loginConsentUri = new UriBuilder(needsConsentResult.AuthorizeUri)
            {
                Query = queryParams.ToString()
            };
            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "auth",
                    SuggestedActions = new MessagingExtensionSuggestedAction
                    {
                        Actions = new List<CardAction>
                        {
                            new CardAction
                            {
                                Type = ActionTypes.OpenUrl,
                                Title = "Please login to GitHub",
                                Value = loginConsentUri.ToString()
                            },
                        },
                    },
                },
            };
        }
        else if (tokenResult is AccessTokenResult accessTokenResult)
        {
            var repos = await _gitHubServiceClient.GetRepositoriesAsync(accessTokenResult.AccessToken);

            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = repos.Select(r =>
                        new MessagingExtensionAttachment
                        {
                            ContentType = HeroCard.ContentType,
                            Content = new HeroCard { Title = $"{r.Name} ({r.Stars})" },
                            Preview = new HeroCard { Title = $"{r.Name} ({r.Stars})" }.ToAttachment(),
                        }).ToList(),
                },
            };
        }
        // There was an error
        return new MessagingExtensionResponse
        {
        };
    }

    protected override async Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running dialog with signin/verifystate from an Invoke Activity.");

        // The OAuth Prompt needs to see the Invoke Activity in order to complete the login process.

        // Run the Dialog with the new Invoke Activity.
        await _dialog.RunAsync(turnContext, _botState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
    }
}
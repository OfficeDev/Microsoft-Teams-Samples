using System.Text.Json;
using System.Web;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Microsoft.Teams.Samples.AccountLinking.OAuth;
using Newtonsoft.Json.Linq;

namespace Microsoft.Teams.Samples.AccountLinking.Dialogs;

/// <summary>
/// This is an implementation of a BotBuilder Dialog prompt that ensures that we have a valid access token for the 
/// current user in the foreign OAuth system. 
/// </summary>
/// <remarks>
/// This prompt is a bit non-idiomatic as it gets most of its parameters through DI and doesn't support PromptOptions
/// </remarks>
public sealed class AccountLinkingPrompt : Dialog
{
    private const string ExpirationKey = "expires";
    private const string CardActivityKey = "cardActivity";
    private const string CodeVerifierKey = "codeVerifier";

    private readonly ILogger<AccountLinkingPrompt> _logger;
    private readonly AccountLinkingPromptOptions _options;
    private readonly OAuthTokenProvider _oauthTokenProvider;

    public AccountLinkingPrompt(
        ILogger<AccountLinkingPrompt> logger,
        IOptions<AccountLinkingPromptOptions> options,
        OAuthTokenProvider oauthTokenProvider)
    {
        _logger = logger;
        _options = options.Value;
        _oauthTokenProvider = oauthTokenProvider;
    }

    public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object? options = null, CancellationToken cancellationToken = default)
    {
        dc = dc ?? throw new ArgumentNullException(nameof(dc));

        if (options != null)
        {
            throw new ArgumentException($"{nameof(options)} cannot be defined for {nameof(AccountLinkingPrompt)}");
        }

        // Initialize state
        var state = dc.ActiveDialog.State;

        state[ExpirationKey] = DateTimeOffset.UtcNow + _options.Timeout;
        var userId = dc.Context.Activity.From.AadObjectId;
        var tenantId = dc.Context.Activity.Conversation.TenantId;

        // Attempt to get the users token
        var tokenResult = await _oauthTokenProvider.GetAccessTokenAsync(tenantId: tenantId, userId: userId);
        if (tokenResult is NeedsConsentResult needsConsentResult)
        {
            var (codeChallenge, codeVerifier) = Pkce.GeneratePkceCodes();
            var queryParams = HttpUtility.ParseQueryString(needsConsentResult.AuthorizeUri.Query);
            queryParams.Add("state", codeChallenge); // For bot we'll just use the codeChallenge as the 'state'
            queryParams.Add("code_challenge", codeChallenge);
            var loginConsentUri = new UriBuilder(needsConsentResult.AuthorizeUri)
            {
                Query = queryParams.ToString()
            }; 
            // we can keep the code verifier out of the 
            state[CodeVerifierKey] = codeVerifier;
            var activity = MessageFactory.Attachment(new Attachment
            {
                ContentType = SigninCard.ContentType,
                Content = new SigninCard
                {
                    Text = "Please sign in",
                    Buttons = new[]
                    {
                        new CardAction
                        {
                            Title = "Sign in",
                            Type = ActionTypes.Signin,
                            Value = loginConsentUri.ToString()
                        },
                    },
                },
            });
            var response = await dc.Context.SendActivityAsync(activity, cancellationToken: cancellationToken).ConfigureAwait(false);
            state[CardActivityKey] = response.Id;
            return EndOfTurn;
        }
        else if (tokenResult is AccessTokenResult accessTokenResult)
        {
            return await dc.EndDialogAsync(accessTokenResult, cancellationToken).ConfigureAwait(false);
        }

        // Prompt user to login
        _logger.LogWarning("Unknown token result, ending turn");
        return EndOfTurn;
    }

    public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
    {
        dc = dc ?? throw new ArgumentNullException(nameof(dc));
        var userId = dc.Context.Activity.From.AadObjectId;
        var tenantId = dc.Context.Activity.Conversation.TenantId;

        // Check for timeout
        var state = dc.ActiveDialog.State;
        var expires = (DateTimeOffset)state[ExpirationKey];
        var isMessage = dc.Context.Activity.Type == ActivityTypes.Message;

        // If the incoming Activity is a message, or an Activity Type normally handled by OAuthPrompt,
        // check to see if this OAuthPrompt Expiration has elapsed, and end the dialog if so.
        var isTokenResponse = IsTokenResponseEvent(dc.Context);
        var isTimeoutActivityType = isMessage || IsTokenResponseEvent(dc.Context);
        var hasTimedOut = isTimeoutActivityType && DateTimeOffset.UtcNow >= expires;

        if (hasTimedOut)
        {
            _logger.LogWarning("User completed logout after timeout, bailing on dialog");
            // if the token fetch request times out, complete the prompt with no result.
            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        if (isTokenResponse)
        {
            _logger.LogInformation("Detected token response, attempting to complete auth flow");
            var value = dc.Context.Activity.Value as JObject;
            var stateString = value?.GetValue("state")?.ToString() ?? string.Empty;
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(stateString);
            var codeVerifier = state[CodeVerifierKey] as string ?? string.Empty;
            var expectedState = Pkce.Base64UrlEncodeSha256(codeVerifier);
            if (!string.Equals(authResponse?.State, expectedState))
            {
                // The state returned doesn't match the expected. potentially a forgery attempt.
                _logger.LogWarning("Potential forgery attempt: {expectedState} | {actualState} | {verifier}", expectedState, authResponse?.State, codeVerifier);
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            await _oauthTokenProvider.ClaimTokenAsync(
                accountLinkingToken: authResponse?.AccountLinkingState ?? string.Empty,
                tenantId: tenantId,
                userId: userId,
                codeVerifier: codeVerifier
            );
        }

        var tokenResult = await _oauthTokenProvider.GetAccessTokenAsync(tenantId: tenantId, userId: userId);
        if (tokenResult is NeedsConsentResult)
        {
            _logger.LogWarning("User failed to consent, bailing on dialog");
            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        else if (tokenResult is AccessTokenResult accessTokenResult)
        {
            var activityId = state[CardActivityKey] as string;
            if (activityId != default)
            {
                // Since the signin "state" is only good for one login, we need to ensure the card for 'login' is overwritten
                var activity = MessageFactory.Attachment(new Attachment
                {
                    ContentType = HeroCard.ContentType,
                    Content = new HeroCard(title: "You are now logged in")
                });
                activity.Id = activityId;
                await dc.Context.UpdateActivityAsync(activity, cancellationToken: cancellationToken); 
            }
            return await dc.EndDialogAsync(accessTokenResult, cancellationToken).ConfigureAwait(false);
        }

        return EndOfTurn;
    }

    private static bool IsTokenResponseEvent(ITurnContext turnContext)
    {
        var activity = turnContext.Activity;
        return activity.Type == ActivityTypes.Invoke && activity.Name == SignInConstants.VerifyStateOperationName;
    }

}

public sealed class AccountLinkingPromptOptions
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
}
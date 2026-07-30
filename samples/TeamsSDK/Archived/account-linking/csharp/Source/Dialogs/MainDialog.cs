using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

using Microsoft.Teams.Samples.AccountLinking.OAuth;
using Microsoft.Teams.Samples.AccountLinking.GitHub;

namespace Microsoft.Teams.Samples.AccountLinking.Dialogs;

/// <summary>
/// An example dialog that links to the user's GitHub account and shows them the list of their repositories. 
/// </summary>
/// <remarks>
/// This dialog makes use of the <seealso cref="AccountLinkingPrompt" /> in the same directory for getting the user to 
/// consent / login if we don't have their access token.
/// </remarks>
public sealed class MainDialog: LogoutDialog
{
    private readonly ILogger<MainDialog> _logger;
    private readonly GitHubServiceClient _githubServiceClient;

    public MainDialog(
        ILogger<MainDialog> logger,
        AccountLinkingPrompt accountLinkingPrompt,
        GitHubServiceClient githubServiceClient, 
        OAuthTokenProvider oauthTokenProvider): base(oauthTokenProvider, nameof(MainDialog))
    {
        _logger = logger;
        _githubServiceClient = githubServiceClient;

        AddDialog(accountLinkingPrompt);
        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new []
        {
            GetWaterfallDialogStep(accountLinkingPrompt.Id),
            TokenStepAsync,
        }));

        InitialDialogId = nameof(WaterfallDialog);
    }

    private WaterfallStep GetWaterfallDialogStep(string id, object? options=default)
    {
        Task<DialogTurnResult> BeginDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return stepContext.BeginDialogAsync(id, options, cancellationToken);
        }

        return BeginDialogAsync;
    }

    private async Task<DialogTurnResult> TokenStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token Step Async");
        var tokenResponse = (AccessTokenResult?)stepContext.Result;
        if (tokenResponse == default)
        {
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
        var repos = await _githubServiceClient.GetRepositoriesAsync(tokenResponse.AccessToken);
        await stepContext.Context.SendActivityAsync(
            MessageFactory.Text(JsonConvert.SerializeObject(repos)),
            cancellationToken: cancellationToken);
        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
    }
}
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Teams.Samples.AccountLinking.OAuth;
using Microsoft.Teams.Samples.AccountLinking.GitHub;

namespace Microsoft.Teams.Samples.AccountLinking.Dialogs;

/// <summary>
/// The LayoutDialog is a utility dialog that lets the user issue the 'logout' command at any time and have their
/// token(s) deleted from our system. 
/// </summary>
/// <remarks>
/// The 'logout' command will end the dialog
/// </remarks>
public class LogoutDialog : ComponentDialog
{
    private readonly OAuthTokenProvider _oauthTokenProvider;

    public LogoutDialog(OAuthTokenProvider oauthTokenProvider, string id = nameof(LogoutDialog)): base(id)
    {
        _oauthTokenProvider = oauthTokenProvider;
    }

    // Called when the dialog is started and pushed onto the parent's dialog stack.
    protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default)
    {
        var result = await InterruptAsync(innerDc, cancellationToken);

        return result ?? await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
    }

    // Called when the dialog is _continued_, where it is the active dialog and the user replies with a new activity.
    protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
    {
        var result = await InterruptAsync(innerDc, cancellationToken);
        return result ?? await base.OnContinueDialogAsync(innerDc, cancellationToken);
    }

    private async Task<DialogTurnResult?> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
    {
        if (innerDc.Context.Activity.Type == ActivityTypes.Message)
        {
            var text = innerDc.Context.Activity.Text.ToLowerInvariant();
            var userId = innerDc.Context.Activity.From.AadObjectId;
            var tenantId = innerDc.Context.Activity.Conversation.TenantId;
            // Allow logout anywhere in the command
            if (text.Contains("logout"))
            {
                await _oauthTokenProvider.LogoutAsync(tenantId: tenantId, userId: userId);
                await innerDc.Context.SendActivityAsync(MessageFactory.Text("You are now logged out"), cancellationToken: cancellationToken);
                return await innerDc.CancelAllDialogsAsync(cancellationToken);
            }
        }

        return default;
    }
}
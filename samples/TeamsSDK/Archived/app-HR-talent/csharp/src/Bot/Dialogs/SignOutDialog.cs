using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Options;
using TeamsTalentMgmtApp.Models;
using TeamsTalentMgmtApp.Services.Interfaces;

namespace TeamsTalentMgmtApp.Bot.Dialogs
{
    public class SignOutDialog : Dialog
    {
        private readonly AppSettings _appSettings;
        private readonly ITokenProvider _tokenProvider;

        public SignOutDialog(IOptions<AppSettings> appSettings, ITokenProvider tokenProvider)
            : base(nameof(SignOutDialog))
        {
            _appSettings = appSettings.Value;
            _tokenProvider = tokenProvider;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var notificationMessage = "You are not logged in yet.";
            var token = await _tokenProvider.GetTokenAsync(dc.Context, cancellationToken);
            if (token != null)
            {
                await _tokenProvider.SetTokenAsync(null, dc.Context, cancellationToken);
                notificationMessage = "You've been logged out.";
            }

            await dc.Context.SendActivityAsync(notificationMessage, cancellationToken: cancellationToken);
            return await dc.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}

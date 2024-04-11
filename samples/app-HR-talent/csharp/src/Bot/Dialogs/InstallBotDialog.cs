using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Options;
using TeamsTalentMgmtApp.Models;
using TeamsTalentMgmtApp.Services.Interfaces;

namespace TeamsTalentMgmtApp.Bot.Dialogs
{
    public class InstallBotDialog : Dialog
    {
        private readonly IGraphApiService _graphApiService;
        private readonly IRecruiterService _recruiterService;
        private readonly ITokenProvider _tokenProvider;
        private readonly AppSettings _appSettings;

        public InstallBotDialog(
            IGraphApiService graphApiService,
            IOptions<AppSettings> appSettings,
            IRecruiterService recruiterService,
            ITokenProvider tokenProvider)
            : base(nameof(InstallBotDialog))
        {
            _appSettings = appSettings.Value;
            _graphApiService = graphApiService;
            _recruiterService = recruiterService;
            _tokenProvider = tokenProvider;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(
            DialogContext dc,
            object options = null,
            CancellationToken cancellationToken = default)
        {
            var hiringManagers = await _recruiterService.GetAllHiringManagers(cancellationToken);

            var successfullyInstalled = new List<string>();
            foreach (var manager in hiringManagers)
            {
                if (await _graphApiService.InstallBotForUser(manager.Alias, dc.Context.Activity.Conversation.TenantId, cancellationToken) == InstallResult.InstallSuccess)
                {
                    successfullyInstalled.Add(manager.Name);
                }
            }

            var message = successfullyInstalled.Count == 0
                ? "Bot wasn't installed to any of hiring manager."
                : $"Bot was successfully installed for: {string.Join(", ", successfullyInstalled)}";

            await dc.Context.SendActivityAsync(message, cancellationToken: cancellationToken);
            return await dc.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}

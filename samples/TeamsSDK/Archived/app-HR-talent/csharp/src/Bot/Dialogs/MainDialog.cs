using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TeamsTalentMgmtApp.Extensions;
using TeamsTalentMgmtApp.Models;
using TeamsTalentMgmtApp.Services.Interfaces;
using TeamsTalentMgmtApp.Constants;
using TeamsTalentMgmtApp.Models.Commands;

namespace TeamsTalentMgmtApp.Bot.Dialogs
{
    public class MainDialog : RouterDialog
    {
        private readonly IBotService _botService;
        private readonly ITokenProvider _tokenProvider;
        private readonly AppSettings _appSettings;

        public MainDialog(
            CandidateDetailsDialog candidateDetailsDialog,
            CandidateSummaryDialog candidateSummaryDialog,
            HelpDialog helpDialog,
            NewJobPostingDialog newJobPostingDialog,
            OpenPositionsDialog openPositionsDialog,
            PositionsDetailsDialog positionsDetailsDialog,
            NewTeamDialog newTeamDialog,
            SignOutDialog signOutDialog,
            InstallBotDialog installBotDialog,
            TopCandidatesDialog topCandidatesDialog,
            IBotService botService,
            IOptions<AppSettings> appSettings,
            IBotTelemetryClient botTelemetryClient,
            ITokenProvider tokenProvider)
            : base(nameof(MainDialog), botTelemetryClient)
        {
            _botService = botService;
            _tokenProvider = tokenProvider;
            _appSettings = appSettings.Value;

            AddDialog(candidateDetailsDialog);
            AddDialog(candidateSummaryDialog);
            AddDialog(helpDialog);
            AddDialog(newJobPostingDialog);
            AddDialog(openPositionsDialog);
            AddDialog(positionsDetailsDialog);
            AddDialog(signOutDialog);
            AddDialog(newTeamDialog);
            AddDialog(topCandidatesDialog);
            AddDialog(installBotDialog);
            AddDialog(new OAuthPrompt(nameof(OAuthPrompt), new OAuthPromptSettings
            {
                ConnectionName = _appSettings.OAuthConnectionName,
                Text = "Please sign in to proceed.",
                Title = "Sign In",
                Timeout = 9000
            }));
        }

        protected override async Task RouteAsync(
            DialogContext innerDc,
            CancellationToken cancellationToken = default)
        {
            var activityText = innerDc.Context.Activity.GetActivityTextWithoutMentions()?.Trim() ?? string.Empty;

            List<(string CommandName, string DialogName, bool AuthorizationIsNeeded)> commandDialogs = new List<(string, string, bool)>
            {
                (BotCommands.HelpDialogCommand, nameof(HelpDialog), false),
                (BotCommands.SignOutDialogCommand, nameof(SignOutDialog), false),
                (BotCommands.CandidateDetailsDialogCommand, nameof(CandidateDetailsDialog), true),
                (BotCommands.TopCandidatesDialogCommand, nameof(TopCandidatesDialog), true),
                (BotCommands.OpenPositionsDialogCommand, nameof(OpenPositionsDialog), true),
                (BotCommands.PositionsDetailsDialogCommand, nameof(PositionsDetailsDialog), true),
                (BotCommands.NewJobPostingDialog, nameof(NewJobPostingDialog), true),
                (BotCommands.CandidateSummaryDialog, nameof(CandidateSummaryDialog), true),
                (BotCommands.NewTeamDialog, nameof(NewTeamDialog), true),
                (BotCommands.InstallBotDialogCommand, nameof(InstallBotDialog), true)
            };

            var sentAnswer = false;
            foreach (var commandDialog in commandDialogs)
            {
                var isFit = Regex.IsMatch(
                    activityText,
                    $@"^(.*){commandDialog.CommandName}(.*)$",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (isFit && !sentAnswer)
                {
                    var dialogName = commandDialog.DialogName;
                    if (commandDialog.AuthorizationIsNeeded)
                    {
                        var connectionName = _appSettings.OAuthConnectionName;
                        var token = await _tokenProvider.GetTokenAsync(innerDc.Context, cancellationToken: cancellationToken);

                        if (string.IsNullOrEmpty(token))
                        {
                            dialogName = nameof(OAuthPrompt);
                        }
                    }

                    await innerDc.BeginDialogAsync(dialogName, cancellationToken: cancellationToken);
                    sentAnswer = true;
                }
            }

            if (!sentAnswer)
            {
                var message = $"Sorry, I didn't understand '{activityText}'. Type {BotCommands.HelpDialogCommand} to explore commands.";
                await innerDc.Context.SendActivityAsync(message, cancellationToken: cancellationToken);
            }
        }

        protected override async Task OnEventAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(innerDc.Context.Activity.Name) &&
                   string.IsNullOrEmpty(innerDc.Context.Activity.Text) &&
                   innerDc.Context.Activity.Value != null)
            {
                var command = JsonConvert.DeserializeObject<ActionCommandBase>(innerDc.Context.Activity.Value.ToString());
                if (string.IsNullOrEmpty(command?.CommandId))
                {
                    return;
                }

                IMessageActivity message = null;
                switch (command.CommandId)
                {
                    case AppCommands.OpenNewPosition:
                        message = await _botService.OpenPositionAsync(innerDc.Context, cancellationToken);
                        break;

                    case AppCommands.LeaveInternalComment:
                        message = await _botService.LeaveInternalCommentAsync(innerDc.Context, cancellationToken);
                        break;

                    case AppCommands.ScheduleInterview:
                        message = await _botService.ScheduleInterviewAsync(innerDc.Context, cancellationToken);
                        break;
                }

                if (message != null)
                {
                    await innerDc.Context.TurnState.Get<IConnectorClient>().Conversations.UpdateActivityWithHttpMessagesAsync(
                        innerDc.Context.Activity.Conversation.Id,
                        innerDc.Context.Activity.ReplyToId,
                        (Activity)message,
                        cancellationToken: cancellationToken);
                }
            }
        }
    }
}

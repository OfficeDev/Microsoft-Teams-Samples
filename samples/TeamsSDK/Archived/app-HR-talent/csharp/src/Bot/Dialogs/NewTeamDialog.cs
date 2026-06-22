using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Options;
using TeamsTalentMgmtApp.Extensions;
using TeamsTalentMgmtApp.Models;
using TeamsTalentMgmtApp.Models.TemplateModels;
using TeamsTalentMgmtApp.Services.Interfaces;
using TeamsTalentMgmtApp.Services.Templates;
using TeamsTalentMgmtApp.Constants;

namespace TeamsTalentMgmtApp.Bot.Dialogs
{
    public class NewTeamDialog : Dialog
    {
        private readonly AppSettings _appSettings;
        private readonly IGraphApiService _graphApiService;
        private readonly IPositionService _positionService;
        private readonly PositionsTemplate _positionsTemplate;

        public NewTeamDialog(
            IGraphApiService graphApiService,
            PositionsTemplate positionsTemplate,
            IPositionService positionService,
            IOptions<AppSettings> appSettings)
            : base(nameof(NewTeamDialog))
        {
            _graphApiService = graphApiService;
            _positionsTemplate = positionsTemplate;
            _positionService = positionService;
            _appSettings = appSettings.Value;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var text = dc.Context.Activity.GetTextWithoutCommand(BotCommands.NewTeamDialog);
            var positions = await _positionService.Search(text, 15, cancellationToken);
            if (positions.Count == 1)
            {
                var connectionName = _appSettings.OAuthConnectionName;
                var token = await ((IUserTokenProvider)dc.Context.Adapter)
                    .GetUserTokenAsync(dc.Context, connectionName, null, cancellationToken);
                var (team, displayName) = await _graphApiService.CreateNewTeamForPosition(positions[0], token.Token, cancellationToken);
                await dc.Context.SendActivityAsync($"[Team {displayName}]({team.WebUrl}) has been created.", cancellationToken: cancellationToken);
            }
            else
            {
                var positionsTemplate = new PositionTemplateModel
                {
                    Items = positions,
                    NoItemsLabel = "You don't have such open positions.",
                    BotCommand = BotCommands.NewTeamDialog,
                    ListCardTitle = "I found following positions:",
                };

                await _positionsTemplate.ReplyWith(dc.Context, TemplateConstants.PositionAsAdaptiveCardWithMultipleItems, positionsTemplate);
            }

            return await dc.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using TeamsTalentMgmtApp.Models.TemplateModels;
using TeamsTalentMgmtApp.Services.Templates;
using TeamsTalentMgmtApp.Constants;
using TeamsTalentMgmtApp.Services.Interfaces;

namespace TeamsTalentMgmtApp.Bot.Dialogs
{
    public class OpenPositionsDialog : Dialog
    {
        private readonly PositionsTemplate _positionsTemplate;
        private readonly IPositionService _positionService;

        public OpenPositionsDialog(
            PositionsTemplate positionsTemplate,
            IPositionService positionService)
            : base(nameof(OpenPositionsDialog))
        {
            _positionsTemplate = positionsTemplate;
            _positionService = positionService;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(
            DialogContext dc,
            object options = null,
            CancellationToken cancellationToken = default)
        {
            var openPositions = await _positionService.GetOpenPositions(dc.Context.Activity.From.Name, cancellationToken);

            var positionsTemplate = new PositionTemplateModel
            {
                ListCardTitle = $"You have {openPositions.Count} active postings right now:",
                Items = openPositions,
                BotCommand = BotCommands.PositionsDetailsDialogCommand,
                NoItemsLabel = "You have no open positions",
                ButtonActions = new Dictionary<string, string>
                {
                    { "Add new job posting", BotCommands.NewJobPostingDialog }
                }
            };

            await _positionsTemplate.ReplyWith(dc.Context, TemplateConstants.PositionAsAdaptiveCardWithMultipleItems, positionsTemplate);

            return await dc.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}

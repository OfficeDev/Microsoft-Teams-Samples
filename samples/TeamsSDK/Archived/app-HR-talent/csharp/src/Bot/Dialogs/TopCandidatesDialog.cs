using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Options;
using TeamsTalentMgmtApp.Extensions;
using TeamsTalentMgmtApp.Models;
using TeamsTalentMgmtApp.Models.TemplateModels;
using TeamsTalentMgmtApp.Services.Templates;
using TeamsTalentMgmtApp.Constants;
using TeamsTalentMgmtApp.Services.Interfaces;

namespace TeamsTalentMgmtApp.Bot.Dialogs
{
    public class TopCandidatesDialog : Dialog
    {
        private readonly AppSettings _appSettings;
        private readonly CandidatesTemplate _candidatesTemplate;
        private readonly PositionsTemplate _positionsTemplate;
        private readonly IPositionService _positionService;
        private readonly IRecruiterService _recruiterService;

        public TopCandidatesDialog(
            IOptions<AppSettings> appSettings,
            IRecruiterService recruiterService,
            CandidatesTemplate candidatesTemplate,
            PositionsTemplate positionsTemplate,
            IPositionService positionService)
            : base(nameof(TopCandidatesDialog))
        {
            _recruiterService = recruiterService;
            _appSettings = appSettings.Value;
            _candidatesTemplate = candidatesTemplate;
            _positionsTemplate = positionsTemplate;
            _positionService = positionService;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(
            DialogContext dc,
            object options = null,
            CancellationToken cancellationToken = default)
        {
            var text = dc.Context.Activity.GetTextWithoutCommand(BotCommands.TopCandidatesDialogCommand);
            var positions = await _positionService.Search(text, 15, cancellationToken);

            if (positions.Count == 1)
            {
                var candidates = positions[0].Candidates;
                var interviewers = await _recruiterService.GetAllInterviewers(cancellationToken);
                CandidateTemplateModel templateModel = new CandidateTemplateModel
                {
                    ListCardTitle = "Top candidates who have recently applied to your position:",
                    BotCommand = BotCommands.CandidateDetailsDialogCommand,
                    Items = candidates,
                    Interviewers = interviewers,
                    AppSettings = _appSettings,
                    NoItemsLabel = $"There are no candidates for position ID: {positions[0].PositionExternalId}",
                    Locale = dc.GetLocale()
                };

                await _candidatesTemplate.ReplyWith(dc.Context, TemplateConstants.CandidateAsAdaptiveCardWithMultipleItems, templateModel);
            }
            else
            {
                PositionTemplateModel positionsTemplate = new PositionTemplateModel
                {
                    ListCardTitle = "I found several positions. Please specify:",
                    Items = positions,
                    BotCommand = BotCommands.TopCandidatesDialogCommand,
                    NoItemsLabel = "You don't have open position with such ID."
                };

                await _positionsTemplate.ReplyWith(dc.Context, TemplateConstants.PositionAsAdaptiveCardWithMultipleItems, positionsTemplate);
            }

            return await dc.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}

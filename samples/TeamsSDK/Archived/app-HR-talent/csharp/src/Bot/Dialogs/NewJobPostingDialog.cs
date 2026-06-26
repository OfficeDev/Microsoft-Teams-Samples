using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using TeamsTalentMgmtApp.Services.Templates;
using TeamsTalentMgmtApp.Services.Interfaces;

namespace TeamsTalentMgmtApp.Bot.Dialogs
{
    public class NewJobPostingDialog : Dialog
    {
        private readonly ILocationService _locationService;
        private readonly IRecruiterService _recruiterService;
        private readonly NewJobPostingToAdaptiveCardTemplate _newJobPostingTemplate;

        public NewJobPostingDialog(
            ILocationService locationService,
            IRecruiterService recruiterService,
            NewJobPostingToAdaptiveCardTemplate newJobPostingTemplate)
            : base(nameof(NewJobPostingDialog))
        {
            _locationService = locationService;
            _recruiterService = recruiterService;
            _newJobPostingTemplate = newJobPostingTemplate;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(
            DialogContext dc,
            object options = null,
            CancellationToken cancellationToken = default)
        {
            var locations = await _locationService.GetAllLocations(cancellationToken);
            var hiringManagers = await _recruiterService.GetAllHiringManagers(cancellationToken);

            await _newJobPostingTemplate.ReplyWith(dc.Context, nameof(NewJobPostingToAdaptiveCardTemplate), new
            {
                Locations = locations,
                HiringManagers = hiringManagers,
                Description = string.Empty
            });

            return await dc.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.EntityFrameworkCore;
using TeamsTalentMgmtApp.Services.Data;

namespace TeamsTalentMgmtApp.Bot.Dialogs
{
    public class HelpDialog : Dialog
    {
        private readonly DatabaseContext _databaseContext;

        public HelpDialog(DatabaseContext databaseContext)
            : base(nameof(HelpDialog))
        {
            _databaseContext = databaseContext;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(
            DialogContext dc,
            object options = null,
            CancellationToken cancellationToken = default)
        {
            var candidate = await _databaseContext.Candidates.Include(x => x.Position).FirstOrDefaultAsync(cancellationToken);

            var helpMessage = "Here's what I can help you do: \n\n"
                              + $"* Show details about a candidate, for example: candidate details {candidate?.Name} \n"
                              + $"* Show summary about a candidate, for example: summary {candidate?.Name} \n"
                              + $"* Show top recent candidates for a Position ID, for example: top candidates {candidate?.Position.PositionExternalId} \n"
                              + "* Create a new job posting \n"
                              + "* List all your open positions";

            await dc.Context.SendActivityAsync(helpMessage, cancellationToken: cancellationToken);
            return await dc.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Root Dialog, its a triggring point for every Child dialog based on the RexEx Match with user input command
    /// </summary>

    public class RootDialog : ComponentDialog
    {
        private static List<Choice> CommandList = new List<Choice>()
            {
                new Choice(DialogMatches.FetchRosterPayloadMatch) { Synonyms = new List<string> { DialogMatches.FetchRosterPayloadMatch } },
                new Choice(DialogMatches.FetchRosterApiMatch) { Synonyms = new List<string> { DialogMatches.FetchRosterApiMatch } },
                new Choice(DialogMatches.HelloDialogMatch1)  { Synonyms = new List<string> { "hello" } }
            };
        public RootDialog()
            : base(nameof(RootDialog))
        {
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptForOptionsAsync
            }));
            AddDialog(new FetchRosterDialog());
            AddDialog(new ListNamesDialog());
            AddDialog(new HelloDialog());
            AddDialog(new HelpDialog());
            AddDialog(new MultiDialog1());
            AddDialog(new MultiDialog2());
            AddDialog(new GetLastDialogUsedDialog());
            AddDialog(new ProactiveMsgTo1to1Dialog());
            AddDialog(new UpdateTextMsgSetupDialog());
            AddDialog(new UpdateTextMsgDialog());
            AddDialog(new UpdateCardMsgSetupDialog());
            AddDialog(new UpdateCardMsgDialog());
            AddDialog(new FetchTeamsInfoDialog());
        }

        private async Task<DialogTurnResult> PromptForOptionsAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = stepContext.Context.Activity.Text.Trim();

            if (command == DialogMatches.FetchRosterPayloadMatch)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(FetchRosterDialog));
            }
            else if (command == DialogMatches.FetchRosterApiMatch)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(ListNamesDialog));
            }
            else if (command == DialogMatches.HelloDialogMatch2 || command == DialogMatches.HelloDialogMatch1)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(HelloDialog));
            }
            else if (command == DialogMatches.Help)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(HelpDialog));
            }
            else if (command == DialogMatches.MultiDialog1Match1)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(MultiDialog1));
            }
            else if (command == DialogMatches.MultiDialog2Match)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(MultiDialog2));
            }
            else if (command == DialogMatches.FecthLastExecutedDialogMatch)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(GetLastDialogUsedDialog));
            }
            else if (command == DialogMatches.Send1to1Conversation)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(ProactiveMsgTo1to1Dialog));
            }
            else if (command == DialogMatches.SetUpTextMsg)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(UpdateTextMsgSetupDialog));
            }
            else if (command == DialogMatches.UpdateLastSetupTextMsg)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(UpdateTextMsgDialog));
            }
            else if (command == DialogMatches.SetUpCardMsg)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(UpdateCardMsgSetupDialog));
            }
            else if (command == DialogMatches.UpdateCard)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(UpdateCardMsgDialog));
            }
            else if (command == DialogMatches.TeamInfo)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(FetchTeamsInfoDialog));
            }
            // We shouldn't get here, but fail gracefully if we do.
            await stepContext.Context.SendActivityAsync(
                "I don't recognize that option.",
                cancellationToken: cancellationToken);
            // Continue through to the next step without starting a child dialog.
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }
    }
}
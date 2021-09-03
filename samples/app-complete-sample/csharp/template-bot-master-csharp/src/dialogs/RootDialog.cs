using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using Microsoft.Teams.TemplateBotCSharp.src.dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Root Dialog, its a triggring point for every Child dialog based on the RexEx Match with user input command
    /// </summary>
    public class RootDialog : ComponentDialog
    {
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;

        public RootDialog(ConversationState conversationState)
            : base(nameof(RootDialog))
        {
            this._conversationState = conversationState.CreateProperty<RootDialogState>(nameof(RootDialogState));
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptForOptionsAsync
            }));
            AddDialog(new FetchRosterDialog(this._conversationState));
            AddDialog(new ListNamesDialog());
            AddDialog(new HelloDialog());
            AddDialog(new HelpDialog());
            AddDialog(new MultiDialog1());
            AddDialog(new MultiDialog2());
            AddDialog(new GetLastDialogUsedDialog(this._conversationState));
            AddDialog(new ProactiveMsgTo1to1Dialog());
            AddDialog(new UpdateTextMsgSetupDialog());
            AddDialog(new UpdateTextMsgDialog());
            AddDialog(new UpdateCardMsgSetupDialog());
            AddDialog(new UpdateCardMsgDialog());
            AddDialog(new FetchTeamsInfoDialog());
            AddDialog(new DeepLinkStaticTabDialog());
            AddDialog(new AtMentionDialog());
            AddDialog(new BeginDialogExampleDialog());
            AddDialog(new HeroCardDialog());
            AddDialog(new ThumbnailcardDialog());
            AddDialog(new MessagebackDialog());
            AddDialog(new AdaptiveCardDialog());
            AddDialog(new PopupSigninCardDialog());
            AddDialog(new QuizFullDialog());
            AddDialog(new PromptDialog());
            AddDialog(new DisplayCardsDialog());
            AddDialog(new O365ConnectorCardActionsDialog());
            AddDialog(new O365ConnectorCardDialog());
        }

        private async Task<DialogTurnResult> PromptForOptionsAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = stepContext.Context.Activity.Text.Trim().ToLower();

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
            else if (command == DialogMatches.DeepLinkTabCard)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(DeepLinkStaticTabDialog));
            }
            else if (command == DialogMatches.AtMentionMatch1 || command == DialogMatches.AtMentionMatch2|| command == DialogMatches.AtMentionMatch3)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(AtMentionDialog));
            }
            else if (command == DialogMatches.DialogFlowMatch)
            {
                await stepContext.Context.SendActivityAsync(Strings.DialogFlowStep1);
                await stepContext.Context.SendActivityAsync(Strings.DialogFlowStep2);
                await stepContext.BeginDialogAsync(
                        nameof(BeginDialogExampleDialog));
                await stepContext.Context.SendActivityAsync(Strings.DialogFlowStep3);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else if (command == DialogMatches.HeroCard)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(HeroCardDialog));
            }
            else if (command == DialogMatches.ThumbnailCard)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(ThumbnailcardDialog));
            }
            else if (command == DialogMatches.MessageBack)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(MessagebackDialog));
            }
            else if (command == DialogMatches.AdaptiveCard)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(AdaptiveCardDialog));
            }
            else if (command == DialogMatches.PopUpSignIn)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(PopupSigninCardDialog));
            }
            else if (command == DialogMatches.RunQuizQuestionsMatch)
            {
                await stepContext.Context.SendActivityAsync(Strings.QuizTitleWelcomeMsg);
                return await stepContext.BeginDialogAsync(
                        nameof(QuizFullDialog));
            }
            else if (command == DialogMatches.PromptFlowGameMatch)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(PromptDialog));
            }
            else if (command == DialogMatches.DisplayCards)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(DisplayCardsDialog));
            }
            else if (command == DialogMatches.StopShowingCards)
            {
                await stepContext.Context.SendActivityAsync(Strings.DisplayCardsThanksMsg);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else if (command == DialogMatches.LocalTime)
            {
                await stepContext.Context.SendActivityAsync(Strings.UTCTimeZonePrompt + stepContext.Context.Activity.Timestamp);
                await stepContext.Context.SendActivityAsync(Strings.LocalTimeZonePrompt + stepContext.Context.Activity.LocalTimestamp);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else if (command == DialogMatches.O365ConnectorCardDefault || command == DialogMatches.DisplayCardO365ConnectorCard2 || command == DialogMatches.DisplayCardO365ConnectorCard3)
            {
                return await stepContext.BeginDialogAsync(
                        nameof(O365ConnectorCardDialog));
            }
            else if (command == DialogMatches.O365ConnectorCardActionableCardDefault || command == DialogMatches.DisplayCardO365ConnectorActionableCard2 )
            {
                return await stepContext.BeginDialogAsync(
                        nameof(O365ConnectorCardActionsDialog));
            }
            // We shouldn't get here, but fail gracefully if we do.
            await stepContext.Context.SendActivityAsync(
                "I don't recognize that option.",
                cancellationToken: cancellationToken);
            // Continue through to the next step without starting a child dialog.
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
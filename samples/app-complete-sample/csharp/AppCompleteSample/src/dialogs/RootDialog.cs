using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using AppCompleteSample.src.dialogs;
using AppCompleteSample.Utility;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace AppCompleteSample.Dialogs
{
    /// <summary>
    /// This is the Root Dialog, a triggering point for every child dialog based on the regex match with user input command.
    /// </summary>
    public class RootDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<RootDialogState> _conversationState;
        private readonly IStatePropertyAccessor<PrivateConversationData> _privateState;
        private readonly IOptions<AzureSettings> _azureSettings;

        public RootDialog(ConversationState conversationState, IOptions<AzureSettings> azureSettings)
            : base(nameof(RootDialog))
        {
            _conversationState = conversationState.CreateProperty<RootDialogState>(nameof(RootDialogState));
            _privateState = conversationState.CreateProperty<PrivateConversationData>(nameof(PrivateConversationData));
            _azureSettings = azureSettings;

            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                    BeginRootDialogAsync
            }));
            AddDialog(new FetchRosterDialog(_conversationState, _azureSettings));
            AddDialog(new ListNamesDialog(_azureSettings));
            AddDialog(new HelloDialog());
            AddDialog(new HelpDialog(_conversationState));
            AddDialog(new MultiDialog1());
            AddDialog(new MultiDialog2(_conversationState, _azureSettings));
            AddDialog(new GetLastDialogUsedDialog(_conversationState));
            AddDialog(new ProactiveMsgTo1to1Dialog(_conversationState, _azureSettings));
            AddDialog(new UpdateTextMsgSetupDialog(_conversationState, _azureSettings));
            AddDialog(new UpdateTextMsgDialog(_conversationState, _azureSettings));
            AddDialog(new UpdateCardMsgSetupDialog(_conversationState));
            AddDialog(new UpdateCardMsgDialog(_conversationState, _azureSettings));
            AddDialog(new FetchTeamsInfoDialog(_conversationState, _azureSettings));
            AddDialog(new DeepLinkStaticTabDialog(_conversationState, _azureSettings));
            AddDialog(new AtMentionDialog(_conversationState));
            AddDialog(new BeginDialogExampleDialog(_conversationState));
            AddDialog(new HeroCardDialog(_conversationState));
            AddDialog(new ThumbnailcardDialog(_conversationState));
            AddDialog(new MessagebackDialog(_conversationState));
            AddDialog(new AdaptiveCardDialog(_conversationState, _azureSettings));
            AddDialog(new PopupSigninCardDialog(_conversationState, _azureSettings));
            AddDialog(new QuizFullDialog(_conversationState));
            AddDialog(new PromptDialog(_conversationState));
            AddDialog(new DisplayCardsDialog(_conversationState, _azureSettings));
            AddDialog(new O365ConnectorCardActionsDialog(_conversationState));
            AddDialog(new O365ConnectorCardDialog(_conversationState));
            AddDialog(new SimpleFacebookAuthDialog(_azureSettings));
        }

        private async Task<DialogTurnResult> BeginRootDialogAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default)
        {
            var activity = stepContext.Context.Activity;
            activity = Middleware.StripAtMentionText(activity);

            // Set activity text if request is from an adaptive card submit action
            activity = Middleware.AdaptiveCardSubmitActionHandler(activity);

            var command = activity.Text.Trim().ToLower();

            switch (command)
            {
                case DialogMatches.FetchRosterPayloadMatch:
                    return await stepContext.BeginDialogAsync(nameof(FetchRosterDialog));
                case DialogMatches.FetchRosterApiMatch:
                    await stepContext.BeginDialogAsync(nameof(ListNamesDialog));
                    await stepContext.Context.SendActivityAsync(Strings.ThanksRosterTitleMsg);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                case DialogMatches.HelloDialogMatch2:
                case DialogMatches.HelloDialogMatch1:
                    return await stepContext.BeginDialogAsync(nameof(HelloDialog));
                case DialogMatches.Help:
                    return await stepContext.BeginDialogAsync(nameof(HelpDialog));
                case DialogMatches.MultiDialog1Match1:
                    return await stepContext.BeginDialogAsync(nameof(MultiDialog1));
                case DialogMatches.MultiDialog2Match:
                    return await stepContext.BeginDialogAsync(nameof(ThumbnailcardDialog));
                case DialogMatches.FetchLastExecutedDialogMatch:
                    return await stepContext.BeginDialogAsync(nameof(GetLastDialogUsedDialog));
                case DialogMatches.Send1to1Conversation:
                    return await stepContext.BeginDialogAsync(nameof(ProactiveMsgTo1to1Dialog));
                case DialogMatches.SetUpTextMsg:
                    return await stepContext.BeginDialogAsync(nameof(UpdateTextMsgSetupDialog));
                case DialogMatches.UpdateLastSetupTextMsg:
                    return await stepContext.BeginDialogAsync(nameof(UpdateTextMsgDialog));
                case DialogMatches.SetUpCardMsg:
                    return await stepContext.BeginDialogAsync(nameof(UpdateCardMsgSetupDialog));
                case DialogMatches.UpdateCard:
                    return await stepContext.BeginDialogAsync(nameof(UpdateCardMsgDialog));
                case DialogMatches.TeamInfo:
                    return await stepContext.BeginDialogAsync(nameof(FetchTeamsInfoDialog));
                case DialogMatches.DeepLinkTabCard:
                    return await stepContext.BeginDialogAsync(nameof(DeepLinkStaticTabDialog));
                case DialogMatches.AtMentionMatch1:
                case DialogMatches.AtMentionMatch2:
                case DialogMatches.AtMentionMatch3:
                    return await stepContext.BeginDialogAsync(nameof(AtMentionDialog));
                case DialogMatches.DialogFlowMatch:
                    await stepContext.Context.SendActivityAsync(Strings.DialogFlowStep1);
                    await stepContext.Context.SendActivityAsync(Strings.DialogFlowStep2);
                    await stepContext.BeginDialogAsync(nameof(BeginDialogExampleDialog));
                    await stepContext.Context.SendActivityAsync(Strings.DialogFlowStep3);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                case DialogMatches.HeroCard:
                    return await stepContext.BeginDialogAsync(nameof(HeroCardDialog));
                case DialogMatches.ThumbnailCard:
                    return await stepContext.BeginDialogAsync(nameof(ThumbnailcardDialog));
                case DialogMatches.MessageBack:
                    return await stepContext.BeginDialogAsync(nameof(MessagebackDialog));
                case DialogMatches.AdaptiveCard:
                    return await stepContext.BeginDialogAsync(nameof(AdaptiveCardDialog));
                case DialogMatches.PopUpSignIn:
                    return await stepContext.BeginDialogAsync(nameof(PopupSigninCardDialog));
                case DialogMatches.RunQuizQuestionsMatch:
                    await stepContext.Context.SendActivityAsync(Strings.QuizTitleWelcomeMsg);
                    return await stepContext.BeginDialogAsync(nameof(QuizFullDialog));
                case DialogMatches.PromptFlowGameMatch:
                    return await stepContext.BeginDialogAsync(nameof(PromptDialog));
                case DialogMatches.DisplayCards:
                    return await stepContext.BeginDialogAsync(nameof(DisplayCardsDialog));
                case DialogMatches.StopShowingCards:
                    await stepContext.Context.SendActivityAsync(Strings.DisplayCardsThanksMsg);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                case DialogMatches.LocalTime:
                    await stepContext.Context.SendActivityAsync(Strings.UTCTimeZonePrompt + stepContext.Context.Activity.Timestamp);
                    await stepContext.Context.SendActivityAsync(Strings.LocalTimeZonePrompt + stepContext.Context.Activity.LocalTimestamp);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                case DialogMatches.O365ConnectorCardDefault:
                case DialogMatches.DisplayCardO365ConnectorCard2:
                case DialogMatches.DisplayCardO365ConnectorCard3:
                    return await stepContext.BeginDialogAsync(nameof(O365ConnectorCardDialog));
                case DialogMatches.O365ConnectorCardActionableCardDefault:
                case DialogMatches.DisplayCardO365ConnectorActionableCard2:
                    return await stepContext.BeginDialogAsync(nameof(O365ConnectorCardActionsDialog));
                case DialogMatches.AuthSample:
                    var message = CreateAuthSampleMessage(stepContext);
                    await stepContext.Context.SendActivityAsync(message);
                    return await stepContext.EndDialogAsync();
                case DialogMatches.FacebookLogin:
                    return await stepContext.BeginDialogAsync(nameof(SimpleFacebookAuthDialog));
                default:
                    await stepContext.Context.SendActivityAsync("I don't recognize that option.", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        #region Create Auth Message Card
        private IMessageActivity CreateAuthSampleMessage(WaterfallStepContext context)
        {
            var message = context.Context.Activity;
            var attachment = CreateAuthSampleCard();
            message.Attachments = new List<Attachment> { attachment };
            return message;
        }

        private Attachment CreateAuthSampleCard()
        {
            return new HeroCard
            {
                Title = Strings.AuthSampleCardTitle,
                Buttons = new List<CardAction>
                    {
                        new CardAction(ActionTypes.ImBack, Strings.FBAuthCardCaption, value: Strings.FBAuthCardValue)
                    }
            }.ToAttachment();
        }
        #endregion
    }
}
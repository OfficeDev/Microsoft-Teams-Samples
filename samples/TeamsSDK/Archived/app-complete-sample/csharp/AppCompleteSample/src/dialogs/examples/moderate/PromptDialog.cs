using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using AppCompleteSample;
using AppCompleteSample.src.dialogs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace AppCompleteSample.Dialogs
{
    /// <summary>
    /// This is Game Dialog Class. Here are the steps to play the games -
    ///  1. Its gives 3 options to users to choose.
    ///  2. If user choose any of the option, Bot take confirmation from the user about the choice.
    ///  3. Bot reply to the user based on user choice.
    /// </summary>
    public class PromptDialog : ComponentDialog
    {
        protected readonly IStatePropertyAccessor<RootDialogState> _conversationState;
        private static List<Choice> options = new List<Choice>()
            {
                new Choice(Strings.PlayGameChoice1) { Synonyms = new List<string> { Strings.PlayGameChoice1 } },
                new Choice(Strings.PlayGameChoice2) { Synonyms = new List<string> { Strings.PlayGameChoice2 } },
                new Choice(Strings.PlayGameWrongChoice) { Synonyms = new List<string> { Strings.PlayGameWrongChoice } }
            };
        private static List<Choice> choiceOptions = new List<Choice>()
            {
                new Choice(Strings.OptionYes) { Synonyms = new List<string> { Strings.OptionYes } },
                new Choice(Strings.OptionNo) { Synonyms = new List<string> { Strings.OptionNo } },
            };

        public PromptDialog(IStatePropertyAccessor<RootDialogState> conversationState) : base(nameof(PromptDialog))
        {
            this._conversationState = conversationState;
            InitialDialogId = nameof(WaterfallDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                BeginPromptDialogAsync,
                GetNameAsync,
                GetOptionAsync,
                ResultedOptionsAsync
            }));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }

        private async Task<DialogTurnResult> BeginPromptDialogAsync(
           WaterfallStepContext stepContext,
           CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            //Set the Last Dialog in Conversation Data
            var currentState = await this._conversationState.GetAsync(stepContext.Context, () => new RootDialogState());
            currentState.LastDialogKey = Strings.LastDialogGameDialog;
            await this._conversationState.SetAsync(stepContext.Context, currentState);

            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text(Strings.PlayGamePromptForName),
                },
                cancellationToken);
        }

        private async Task<DialogTurnResult> GetNameAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var userName = stepContext.Result as string;
            await stepContext.Context.SendActivityAsync(Strings.PlayGameAnswerForName + userName);
            return await stepContext.PromptAsync(
                    nameof(ChoicePrompt),
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text(Strings.PlayGameStartMsg),
                        Choices = options,
                        RetryPrompt = MessageFactory.Text(Strings.PromptInvalidMsg)
                    },
                    cancellationToken);
        }

        /// <summary>
        /// In this method, we are taking confirmation from user about the selection.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> GetOptionAsync(
          WaterfallStepContext stepContext,
          CancellationToken cancellationToken = default(CancellationToken))
        {
            var optionSelected = (stepContext.Result as FoundChoice).Value;
            return await stepContext.PromptAsync(
                    nameof(ChoicePrompt),
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text(Strings.PlayGameReplyMsg + "'" + optionSelected + "'?"),
                        Choices = choiceOptions,
                        RetryPrompt = MessageFactory.Text(Strings.PromptInvalidMsg)
                    },
                    cancellationToken);

        }

        /// <summary>
        /// End this dialog and pass the user selection to ResumeAfterFlowGame method in Root Dialog
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ResultedOptionsAsync(WaterfallStepContext stepContext,
          CancellationToken cancellationToken = default(CancellationToken))
        {
            var optionSelected = (stepContext.Result as FoundChoice).Value;
            if (optionSelected == null)
            {
                throw new InvalidOperationException((nameof(optionSelected)) + Strings.NullException);
            }
            if (optionSelected == Strings.OptionYes)
            {
                await stepContext.Context.SendActivityAsync(Strings.PlayGameThanksMsg);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(Strings.PlayGameFailMsg);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }
    }
}
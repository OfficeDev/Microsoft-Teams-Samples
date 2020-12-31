using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Game Dialog Class. Here are the steps to play the games -
    ///  1. Its gives 3 options to users to choose.
    ///  2. If user choose any of the option, Bot take confirmation from the user about the choice.
    ///  3. Bot reply to the user based on user choice.
    /// </summary>
    [Serializable]
    public class PromptDialogExample : IDialog<bool>
    {
        private IEnumerable<string> options = null;
        private IEnumerable<string> choiceOptions = null;

        /// <summary>
        /// In below class constructor, we are initializing the strings Enumerable at runtime. 
        /// </summary>
        public PromptDialogExample()
        {
            options = new string[] { Strings.PlayGameChoice1, Strings.PlayGameChoice2, Strings.PlayGameWrongChoice };
            choiceOptions = new string[] { Strings.OptionYes, Strings.OptionNo };
        }

        /// <summary>
        /// This is start of the Dialog and Prompting for User name
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogGameDialog);

            // This will Prompt for Name of the user.
            await context.PostAsync(Strings.PlayGamePromptForName);
            context.Wait(this.MessageReceivedAsync);
        }

        /// <summary>
        /// Prompt the welcome message. 
        /// Few options for user to choose any.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            if (result == null)
            {
                throw new InvalidOperationException((nameof(result)) + Strings.NullException);
            }

            //Prompt the user with welcome message before game starts
            var resultActivity = await result;
            await context.PostAsync(Strings.PlayGameAnswerForName + resultActivity.Text);

            //Here we are prompting few options for user to choose any.
            PromptDialog.Choice<string>(
                context,
                this.ChooseOptions,
                options,
                Strings.PlayGameStartMsg,
                Strings.PromptInvalidMsg,
                3);
        }

        /// <summary>
        /// In this methos, we are taking confirmation from user about the selection.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task ChooseOptions(IDialogContext context, IAwaitable<string> result)
        {
            var selctedChoice = await result;

            //Once User choose any of the given option, we are taking confirmation from user about the selection.
            PromptDialog.Choice<string>(
                context,
                this.ResultedOptions,
                choiceOptions,
                Strings.PlayGameReplyMsg + "'" + selctedChoice + "'?",
                Strings.PromptInvalidMsg,
                3);
        }

        /// <summary>
        /// End this dialog and pass the user selection to ResumeAfterFlowGame method in Root Dialog
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task ResultedOptions(IDialogContext context, IAwaitable<string> result)
        {
            if (result == null)
            {
                throw new InvalidOperationException((nameof(result)) + Strings.NullException);
            }

            var selctedChoice = await result;
            if (selctedChoice == Strings.OptionYes)
            {
                context.Done(true);
            }
            else
            {
                context.Done(false);
            }
        }
    }
}
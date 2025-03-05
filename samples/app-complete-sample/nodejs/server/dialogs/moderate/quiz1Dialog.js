// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog, ChoicePrompt } = require('botbuilder-dialogs');
const QUIZ1DIALOG = 'Quiz1Dialog';
const CHOICE_PROMPT = 'choiceDialog';

/**
 * Quiz1Dialog class extends ComponentDialog to handle a simple quiz dialog flow.
 */
class Quiz1Dialog extends ComponentDialog {
    /**
     * Constructor for the Quiz1Dialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(QUIZ1DIALOG, [
            this.beginQuiz1Dialog.bind(this),
            this.endQuiz1Dialog.bind(this),
        ]));
        this.addDialog(new ChoicePrompt(CHOICE_PROMPT, this.validateNumberOfAttempts.bind(this)));
    }

    /**
     * Begins the quiz dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginQuiz1Dialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "QuizQ1Dialog";
        return await stepContext.prompt(
            CHOICE_PROMPT, {
                prompt: 'Question 1',
                choices: ['yes', 'no'],
                retryPrompt: 'Not a valid option'
            }
        );
    }

    /**
     * Ends the quiz dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async endQuiz1Dialog(stepContext) {
        const answer = stepContext.result.value;

        if (!answer) {
            // exhausted attempts and no selection, start over
            await stepContext.context.sendActivity('Not a valid option. We\'ll restart the dialog so you can try again!');
            return await stepContext.endDialog();
        }

        if (answer === 'yes') {
            await stepContext.context.sendActivity("You are Right");
            return await stepContext.endDialog();
        }

        if (answer === 'no') {
            await stepContext.context.sendActivity("Wrong Choice");
            return await stepContext.endDialog();
        }

        return await stepContext.endDialog();
    }

    /**
     * Validates the number of attempts for the choice prompt.
     * @param {PromptValidatorContext} promptContext - The prompt validator context.
     * @returns {Promise<boolean>} Whether the validation succeeded.
     */
    async validateNumberOfAttempts(promptContext) {
        if (promptContext.attemptCount > 3) {
            // cancel everything
            await promptContext.context.sendActivity('Oops! Too many attempts :( But don\'t worry, I\'m handling that exception and you can try again!');
            return await promptContext.context.endDialog();
        }

        if (!promptContext.recognized.succeeded) {
            await promptContext.context.sendActivity(promptContext.options.retryPrompt);
            return false;
        }
        return true;
    }
}

exports.Quiz1Dialog = Quiz1Dialog;
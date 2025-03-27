// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog, ChoicePrompt } = require('botbuilder-dialogs');
const QUIZ2DIALOG = 'Quiz2Dialog';
const CHOICE_PROMPT = 'choiceDialog';

/**
 * Dialog to handle the second quiz question.
 */
class Quiz2Dialog extends ComponentDialog {
    /**
     * Creates a new instance of the Quiz2Dialog class.
     * @param {string} id - The dialog ID.
     * @param {Object} conversationDataAccessor - The conversation state accessor.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;

        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(QUIZ2DIALOG, [
            this.beginQuiz2Dialog.bind(this),
            this.endQuiz2Dialog.bind(this),
        ]));
        this.addDialog(new ChoicePrompt(CHOICE_PROMPT, this.validateNumberOfAttempts.bind(this)));
    }

    /**
     * Begins the Quiz2Dialog.
     * @param {Object} stepContext - The step context.
     * @returns {Promise} - A promise representing the asynchronous operation.
     */
    async beginQuiz2Dialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "QuizQ2Dialog";
        return await stepContext.prompt(
            CHOICE_PROMPT, {
                prompt: 'Question 2',
                choices: ['yes', 'no'],
                retryPrompt: 'Not a valid option'
            }
        );
    }

    /**
     * Ends the Quiz2Dialog.
     * @param {Object} stepContext - The step context.
     * @returns {Promise} - A promise representing the asynchronous operation.
     */
    async endQuiz2Dialog(stepContext) {
        const answer = stepContext.result.value;

        if (!answer) {
            // Exhausted attempts and no selection, start over
            await stepContext.context.sendActivity('Not a valid option. We\'ll restart the dialog so you can try again!');
            return await stepContext.endDialog();
        }

        if (answer === 'yes') {
            await stepContext.context.sendActivity("You are Right");
        } else if (answer === 'no') {
            await stepContext.context.sendActivity("Wrong Choice");
        }

        return await stepContext.endDialog();
    }

    /**
     * Validates the number of attempts for the choice prompt.
     * @param {Object} promptContext - The prompt context.
     * @returns {Promise<boolean>} - A promise representing the validation result.
     */
    async validateNumberOfAttempts(promptContext) {
        if (promptContext.attemptCount > 3) {
            // Cancel everything
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

exports.Quiz2Dialog = Quiz2Dialog;
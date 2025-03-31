// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const QUIZFULLDIALOG = 'QuizFullDialog';
const QUIZ1DIALOG = 'Quiz1Dialog';
const QUIZ2DIALOG = 'Quiz2Dialog';
const { Quiz1Dialog } = require('./quiz1Dialog');
const { Quiz2Dialog } = require('./quiz2Dialog');

/**
 * QuizFullDialog class extends ComponentDialog to handle a full quiz dialog flow.
 */
class QuizFullDialog extends ComponentDialog {
    /**
     * Constructor for the QuizFullDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(QUIZFULLDIALOG, [
            this.beginQuizFullDialog.bind(this),
            this.resumeAfterRunQuiz1Dialog.bind(this),
            this.resumeAfterBeginQuiz2.bind(this),
        ]));
        this.addDialog(new Quiz1Dialog(QUIZ1DIALOG, this.conversationDataAccessor));
        this.addDialog(new Quiz2Dialog(QUIZ2DIALOG, this.conversationDataAccessor));
    }

    /**
     * Begins the full quiz dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginQuizFullDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "QuizFullDialog";
        await stepContext.context.sendActivity('Begin quiz dialog');
        return await stepContext.beginDialog(QUIZ1DIALOG);
    }

    /**
     * Resumes after running Quiz 1 dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async resumeAfterRunQuiz1Dialog(stepContext) {
        await stepContext.context.sendActivity('Thanks for Completing Quiz 1');
        return await stepContext.beginDialog(QUIZ2DIALOG);
    }

    /**
     * Resumes after running Quiz 2 dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async resumeAfterBeginQuiz2(stepContext) {
        await stepContext.context.sendActivity('Thanks for playing fun quiz!!');
        return await stepContext.endDialog();
    }
}

exports.QuizFullDialog = QuizFullDialog;
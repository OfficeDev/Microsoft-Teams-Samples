// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog } = require('botbuilder-dialogs');
const QUIZFULLDIALOG = 'QuizFullDialog';
const QUIZ1DIALOG = 'Quiz1Dialog';
const QUIZ2DIALOG = 'Quiz2Dialog';
const { Quiz1Dialog } = require('./quiz1Dialog');
const { Quiz2Dialog } = require('./quiz2Dialog');

class QuizFullDialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(QUIZFULLDIALOG, [
            this.beginQuizFulleDailog.bind(this),
            this.resumeAfterRunQuiz1Dialog.bind(this),
            this.ResumeAfterBeginQuiz2.bind(this),
        ]));
        this.addDialog(new Quiz1Dialog(QUIZ1DIALOG,this.conversationDataAccessor));
        this.addDialog(new Quiz2Dialog(QUIZ2DIALOG,this.conversationDataAccessor));
    }

    async beginQuizFulleDailog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "QuizFullDialog";
        await stepContext.context.sendActivity('Begin quiz dialog');
        return await stepContext.beginDialog(QUIZ1DIALOG);
    }

    async resumeAfterRunQuiz1Dialog(stepContext) {
        await stepContext.context.sendActivity('Thanks for Completing Quiz 1');
        return await stepContext.beginDialog(QUIZ2DIALOG);
    }
    
    async ResumeAfterBeginQuiz2(stepContext) {
        await stepContext.context.sendActivity('Thanks for playing fun quiz!!');
        return await stepContext.endDialog();
    }
}

exports.QuizFullDialog = QuizFullDialog;
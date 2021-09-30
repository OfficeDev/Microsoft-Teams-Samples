// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog, ChoicePrompt } = require('botbuilder-dialogs');
const PROMPTDIALOG = 'PromptDialog';
const CHOICE_PROMPT = 'choiceDialog'

class PromptDialog extends ComponentDialog {
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(PROMPTDIALOG, [
            this.beginPromptDialog.bind(this),
            this.getOptionAsync.bind(this),
            this.resultedOptionsAsync.bind(this)
        ]));
        this.addDialog(new ChoicePrompt(CHOICE_PROMPT, this.validateNumberOfAttempts.bind(this)));
    }

    async beginPromptDialog(stepContext) {
        var currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "PromptDialog";
        return await stepContext.prompt(
            CHOICE_PROMPT, {
            prompt: 'Lets play a game! You pick a button, and I will repeat it.',
            choices: ['choice1', 'choice2', 'Wrong Choice'],
            retryPrompt: 'Not a valid option'
        }
        );
    }

    async getOptionAsync(stepContext) {
        const answer = stepContext.result.value;

        if (!answer) {
            // exhausted attempts and no selection, start over
            await stepContext.context.sendActivity('Not a valid option. We\'ll restart the dialog ' +
                'so you can try again!');
            return await stepContext.endDialog();
        }
        return await stepContext.prompt(
            CHOICE_PROMPT, {
            prompt: "Did you pick" + answer,
            choices: ['yes', 'no'],
            retryPrompt: 'Not a valid option'
        }
        );
    }

    async resultedOptionsAsync(stepContext) {
        const answer = stepContext.result.value;

        if (!answer) {
            // exhausted attempts and no selection, start over
            await stepContext.context.sendActivity('Not a valid option. We\'ll restart the dialog ' +
                'so you can try again!');
            return await stepContext.endDialog();
        }

        if (answer === 'yes') {
            await stepContext.context.sendActivity("Yay!!!! What a fun game!");
            return await stepContext.endDialog();
        }

        if (answer === 'no') {
            await stepContext.context.sendActivity("Awwww man! I'm still in the prototype phase.");
            return await stepContext.endDialog();
        }

        return await stepContext.endDialog();
    }

    async validateNumberOfAttempts(promptContext) {

        if (promptContext.attemptCount > 3) {
            // cancel everything
            await promptContext.context.sendActivity('Oops! Too many attempts :( But don\'t worry, I\'m ' +
                'handling that exception and you can try again!');
            return await promptContext.context.endDialog();
        }

        if (!promptContext.recognized.succeeded) {
            await promptContext.context.sendActivity(promptContext.options.retryPrompt);
            return false;
        }

        return true;
    }
}

exports.PromptDialog = PromptDialog;
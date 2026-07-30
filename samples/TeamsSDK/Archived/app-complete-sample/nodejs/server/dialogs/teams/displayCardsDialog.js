// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { WaterfallDialog, ComponentDialog, ChoicePrompt } = require('botbuilder-dialogs');
const DISPLAYCARDS = 'DisplayCards';
const CHOICE_PROMPT = 'choiceDialog';
const THUMBNAILCARD = 'ThumbnailCard';
const ADAPTIVECARD = "AdaptiveCardDialog";
const O365CONNECTORECARD = 'O365ConnectorCard';
const HEROCARD = 'HeroCard';
const O365CONNECTORCARDACTION = 'O365ConnectorCardAction';
const { HeroCardDialog } = require('../basic/heroCardDialog');
const { ThumbnailCardDialog } = require('../basic/thumbnailCardDialog');
const { AdaptiveCardDialog } = require('../basic/adaptiveCardDialog');
const { O365ConnectorCardDialog } = require('../basic/o365connectorCardDialog');
const { O365ConnectorCardActionDialog } = require('../basic/o365ConnectorCardActionDialog');

/**
 * DisplayCardsDialog class extends ComponentDialog to handle displaying various types of cards.
 */
class DisplayCardsDialog extends ComponentDialog {
    /**
     * Constructor for the DisplayCardsDialog class.
     * @param {string} id - The dialog ID.
     * @param {StatePropertyAccessor} conversationDataAccessor - The state property accessor for conversation data.
     */
    constructor(id, conversationDataAccessor) {
        super(id);
        this.conversationDataAccessor = conversationDataAccessor;
        // Define the conversation flow using a waterfall model.
        this.addDialog(new WaterfallDialog(DISPLAYCARDS, [
            this.beginDisplayCardsDialog.bind(this),
            this.getDisplayCardsDialog.bind(this),
            this.endDisplayCardDialog.bind(this),
        ]));
        this.addDialog(new ChoicePrompt(CHOICE_PROMPT, this.validateNumberOfAttempts.bind(this)));
        this.addDialog(new HeroCardDialog(HEROCARD, this.conversationDataAccessor));
        this.addDialog(new ThumbnailCardDialog(THUMBNAILCARD, this.conversationDataAccessor));
        this.addDialog(new AdaptiveCardDialog(ADAPTIVECARD, this.conversationDataAccessor));
        this.addDialog(new O365ConnectorCardDialog(O365CONNECTORECARD, this.conversationDataAccessor));
        this.addDialog(new O365ConnectorCardActionDialog(O365CONNECTORCARDACTION, this.conversationDataAccessor));
    }

    /**
     * Begins the display cards dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async beginDisplayCardsDialog(stepContext) {
        const currentState = await this.conversationDataAccessor.get(stepContext.context, {});
        currentState.lastDialogKey = "DisplayCardsDialog";
        await stepContext.context.sendActivity("Please select any card");
        return await stepContext.prompt(
            CHOICE_PROMPT, {
                prompt: 'What card would you like to test?',
                choices: ['herocard', 'thumbnailcard', 'adaptivecard', 'connector card 1', 'connector card 2', 'connector card 3', 'Connector Card Actions 2', 'Connector Card Actions'],
                retryPrompt: 'Not a valid option'
            }
        );
    }

    /**
     * Gets the selected card dialog from the user.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async getDisplayCardsDialog(stepContext) {
        const answer = stepContext.result.value;

        if (!answer) {
            // exhausted attempts and no selection, start over
            await stepContext.context.sendActivity('Not a valid option. We\'ll restart the dialog so you can try again!');
            return await stepContext.endDialog();
        }
        switch (answer) {
            case 'herocard':
                return await stepContext.beginDialog(HEROCARD);
            case 'thumbnailcard':
                return await stepContext.beginDialog(THUMBNAILCARD);
            case 'adaptivecard':
                return await stepContext.beginDialog(ADAPTIVECARD);
            case 'connector card 1':
            case 'connector card 2':
            case 'connector card 3':
                return await stepContext.beginDialog(O365CONNECTORECARD);
            case 'Connector Card Actions 2':
            case 'Connector Card Actions':
                return await stepContext.beginDialog(O365CONNECTORCARDACTION);
            default:
                return await stepContext.endDialog();
        }
    }

    /**
     * Ends the display card dialog.
     * @param {WaterfallStepContext} stepContext - The waterfall step context.
     * @returns {Promise<DialogTurnResult>} The result of the dialog turn.
     */
    async endDisplayCardDialog(stepContext) {
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

exports.DisplayCardsDialog = DisplayCardsDialog;
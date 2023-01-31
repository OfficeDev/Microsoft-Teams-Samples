// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityHandler, MessageFactory, CardFactory } = require('botbuilder');
const { ActionTypes } = require('botframework-schema');
const MentionSupport = require('../resources/mentionSupport.json');
const InformationMaskingCard = require('../resources/informationMasking.json');
const SampleAdaptiveCard = require('../resources/sampleAdaptiveWithFullWidth.json');
const StageViewImagesCard = require('../resources/stageViewForImages.json');
const OverFlowMenuCard = require('../resources/overflowMenu.json');
const HTMLConnectorCard = require('../resources/formatHTMLConnectorCard.json');
const CardWithEmoji = require('../resources/adaptiveCardWithEmoji.json');

class BotFormattingCards extends ActivityHandler {
    constructor() {
        super();

        this.onMembersAdded(async (context, next) => {
            //Send welcome message when app installed
            await this.sendWelcomeMessage(context);

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });

        this.onMessage(async (context, next) => {
            const text = context.activity.text;

            // Create an array with the valid card options.
            const adaptiveFormatCards = ['MentionSupport', 'InfoMasking', 'FullWidthCard', 'StageViewImages', 'OverflowMenu', 'HTMLConnector', 'CardWithEmoji'];

            // If the `text` is in the Array, a valid card was selected and sends.
            if (adaptiveFormatCards.includes(text)) {

                switch (text) {
                    case "MentionSupport":
                        await context.sendActivity({ attachments: [this.sendMentionSupportCard()] });
                        break;

                    case "InfoMasking":
                        await context.sendActivity({ attachments: [this.sendInfoMasking()] });
                        break;

                    case "FullWidthCard":
                        await context.sendActivity({ attachments: [this.SendfullWidthCard()] });
                        break;

                    case "StageViewImages":
                        await context.sendActivity({ attachments: [this.sendStageViewImagesCard()] });
                        break;

                    case "OverflowMenu":
                        await context.sendActivity({ attachments: [this.sendOverFlowMenuCard()] });
                        break;

                    case "HTMLConnector":
                        await context.sendActivity({ attachments: [this.sendHTMLConnectorCard()] });
                        break;

                    case "CardWithEmoji":
                        await context.sendActivity({ attachments: [this.sendCardWithEmoji()] });
                        break;
                }

                await context.sendActivity(`You have Selected <b>${text}</b>`);
            }

            // After the bot has responded send the fromat Cards.
            await this.sendAdaptiveCardFromats(context);

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
    }

    /**
     * Send a welcome message along with Adaptive card format actions for the user to click.
     * @param {TurnContext} turnContext A TurnContext instance containing all the data needed for processing this conversation turn.
     */
    async sendWelcomeMessage(turnContext) {
        const { activity } = turnContext;

        // Iterate over all new members added to the conversation.
        for (const idx in activity.membersAdded) {
            if (activity.membersAdded[idx].id !== activity.recipient.id) {
                const welcomeMessage = `Welcome to Adaptive Card Format. This bot will introduce you to different types of formats. Please select the cards from given options`;

                await turnContext.sendActivity(welcomeMessage);

                //send the adaptive card formats.
                await this.sendAdaptiveCardFromats(turnContext);
            }
        }
    }

    /**
    * Sends Mention Support Card
    */
    sendMentionSupportCard() {
        return CardFactory.adaptiveCard(MentionSupport);
    }

    /**
    * Sends Sample Adaptive Card With Full Width
    */
    SendfullWidthCard() {
        return CardFactory.adaptiveCard(SampleAdaptiveCard);
    }

    /**
    * Sends StageView Images Card
    */
    sendStageViewImagesCard() {
        return CardFactory.adaptiveCard(StageViewImagesCard);
    }

    /**
    * Sends InfoMasking Card
    */
    sendInfoMasking() {
        return CardFactory.adaptiveCard(InformationMaskingCard);
    }

   /**
    * Sends OverFlow Menu Card
    */
    sendOverFlowMenuCard() {
        return CardFactory.adaptiveCard(OverFlowMenuCard);
    }

    /**
    * Sends HTML Connector Card
    */
    sendHTMLConnectorCard() {
        return CardFactory.o365ConnectorCard(HTMLConnectorCard);
    }

    /**
    * Sends Card With Emoji
    */
    sendCardWithEmoji() {
        return CardFactory.adaptiveCard(CardWithEmoji);
    }

    /**
   * Send AdaptiveCard Fromats to the user.
   * @param {TurnContext} turnContext A TurnContext instance containing all the data needed for processing this conversation turn.
   */
    async sendAdaptiveCardFromats(turnContext) {
        const cardActions = [
            {
                type: ActionTypes.ImBack,
                title: 'MentionSupport',
                value: 'MentionSupport'
            },
            {
                type: ActionTypes.ImBack,
                title: 'InfoMasking',
                value: 'InfoMasking'
            },
            {
                type: ActionTypes.ImBack,
                title: 'FullWidthCard',
                value: 'FullWidthCard'
            },
            {
                type: ActionTypes.ImBack,
                title: 'StageViewImages',
                value: 'StageViewImages'
            },
            {
                type: ActionTypes.ImBack,
                title: 'OverflowMenu',
                value: 'OverflowMenu'
            },
            {
                type: ActionTypes.ImBack,
                title: 'HTMLConnector',
                value: 'HTMLConnector'
            },
            {
                type: ActionTypes.ImBack,
                title: 'CardWithEmoji',
                value: 'CardWithEmoji'
            },
            {
                type: ActionTypes.ImBack,
                title: '',
                value: ''
            }
        ];

        var reply = MessageFactory.text("Please select a card from given options. ");
        reply.suggestedActions = { "actions": cardActions, "to": [turnContext.activity.from.id] };

        await turnContext.sendActivity(reply);
    }
}

module.exports.BotFormattingCards = BotFormattingCards;

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityHandler, MessageFactory, CardFactory } = require('botbuilder');
const { ActionTypes } = require('botframework-schema');
const AdaptiveCard = require('../resources/adaptiveCard.json');
const Office365ConnectorCard = require('../resources/o365ConnectorCard.json');
const ThumbnailCard = require('../resources/thumbnailCard.json');
const ReceiptCard = require('../resources/receiptCard.json');
const ListCard = require('../resources/listCard.json');
const CollectionCard = require('../resources/collectionsCard.json');

class BotSuggestedCards extends ActivityHandler {
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
            const suggestedCards = ['AdaptiveCard', 'HeroCard', 'ListCard', 'Office365', 'CollectionCard', 'SignIn', 'ThumbnailCard'];

            // If the `text` is in the Array, a valid color was selected and send agreement.
            if (suggestedCards.includes(text)) {

                switch (text) {
                    case "AdaptiveCard":
                        await context.sendActivity({ attachments: [this.sendAdaptiveCard()] });
                        break;

                    case "HeroCard":
                        await context.sendActivity({ attachments: [this.sendHeroCard()] });
                        break;

                    case "Office365":
                        await context.sendActivity({ attachments: [this.sendOffice356Card()] });
                        break;

                    case "ReceiptCard":
                        await context.sendActivity({ attachments: [ReceiptCard] });
                        break;

                    case "ThumbnailCard":
                        await context.sendActivity({ attachments: [ThumbnailCard] });
                        break;

                    case "SignIn":
                        await context.sendActivity({ attachments: [this.sendOAuthCard()] });
                        break;

                    case "OAuth":
                        await context.sendActivity({ attachments: [this.sendOAuthCard()] });
                        break;

                    case "ListCard":
                        await context.sendActivity({ attachments: [ListCard] });
                        break;

                    case "CollectionCard":
                        await context.sendActivity({ attachments: [this.sendCollectionCard()] });
                        break;
                }

                await context.sendActivity(`You have Selected <b>${text}</b>`);
            }

            // After the bot has responded send the suggested Cards.
            await this.sendSuggestedCards(context);

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
    }

    /**
     * Send a welcome message along with suggested actions for the user to click.
     * @param {TurnContext} turnContext A TurnContext instance containing all the data needed for processing this conversation turn.
     */
    async sendWelcomeMessage(turnContext) {
        const { activity } = turnContext;

        // Iterate over all new members added to the conversation.
        for (const idx in activity.membersAdded) {
            if (activity.membersAdded[idx].id !== activity.recipient.id) {
                const welcomeMessage = `Welcome to Cards. This bot will introduce you to suggested cards. Please select the cards from given options` +
                    'Please select an option:';

                await turnContext.sendActivity(welcomeMessage);

                //send the suggested Cards.
                await this.sendSuggestedCards(turnContext);
            }
        }
    }

    // Adaptive Card 
    sendAdaptiveCard() {
        return CardFactory.adaptiveCard(AdaptiveCard);
    }

    // Office356 Connector Card 
    sendOffice356Card() {
        return CardFactory.o365ConnectorCard(Office365ConnectorCard);
    }

    //OAuthCard
    sendOAuthCard() {
        return CardFactory.oauthCard(
            'Sign In',
            'OAuth connection',// Replace with the name of your Azure AD connection
            'BotFramework OAuth Card'
        );
    }

    //SignInCard
    sendSignInCard() {
        return CardFactory.signinCard(
            'Sign In',
            'Azure AD connection', // Replace with the name of your Azure AD connection
            'BotFramework SignIn Card'
        );
    }

    //ReceiptCard
    sendReceiptCard() {
        return CardFactory.receiptCard(ReceiptCard);
    }

    //HeroCard
    sendHeroCard() {
        return CardFactory.heroCard(
            'BotFramework Hero Card',
            CardFactory.images(['https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Seattle_monorail01_2008-02-25.jpg/1024px-Seattle_monorail01_2008-02-25.jpg']),
            CardFactory.actions([
                {
                    type: 'openUrl',
                    title: 'Get started',
                    value: 'https://docs.microsoft.com/en-us/azure/bot-service/'
                }
            ])
        );
    }

    //CollectionCard
    sendCollectionCard() {
        return CardFactory.adaptiveCard(CollectionCard);
    }

    /**
   * Send suggested Carfs to the user.
   * @param {TurnContext} turnContext A TurnContext instance containing all the data needed for processing this conversation turn.
   */
    async sendSuggestedCards(turnContext) {
        const cardActions = [
            {
                type: ActionTypes.ImBack,
                title: 'AdaptiveCard',
                value: 'AdaptiveCard'
            },
            {
                type: ActionTypes.ImBack,
                title: 'HeroCard',
                value: 'HeroCard'
            },
            {
                type: ActionTypes.ImBack,
                title: 'ListCard',
                value: 'ListCard'
            },
            {
                type: ActionTypes.ImBack,
                title: 'Office365',
                value: 'Office365'
            },
            {
                type: ActionTypes.ImBack,
                title: 'CollectionCard',
                value: 'CollectionCard'
            },
            {
                type: ActionTypes.ImBack,
                title: 'SignIn',
                value: 'SignIn'
            },
            {
                type: ActionTypes.ImBack,
                title: 'ThumbnailCard',
                value: 'ThumbnailCard'
            },
            {
                type: ActionTypes.ImBack,
                title: 'Cards',
                value: 'Cards'
            }
        ];

        //Returns a simple text message.
        var reply = MessageFactory.text("Please select a card from given options. ");
        reply.suggestedActions = { "actions": cardActions, "to": [turnContext.activity.from.id] };

        await turnContext.sendActivity(reply);
    }
}

module.exports.BotSuggestedCards = BotSuggestedCards;

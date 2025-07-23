
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityHandler, MessageFactory, CardFactory } = require('botbuilder');
const fs = require('fs');
const path = require('path');
const ACData = require('adaptivecards-templating');

class UserSpecificAdaptiveCardActionBot extends ActivityHandler {
    constructor() {
        super();

        this.onMessage(async (context, next) => {
            const text = context.activity.text;

            // On first message, show the card type selector card
            if (text) {
                await this.sendAdaptiveCard(context, this.getSelectCardTypeCard());
            }

            await this.handleCardActionSubmit(context);
            await next();
        });

        this.onInvokeActivity = async (context) => {
            if (context.activity.name === 'adaptiveCard/action') {
                const actionData = context.activity.value;
                const verb = actionData.action?.verb;
                const userId = context.activity.from.id;

                let card;
                actionData.action.data.refreshCount++;
                switch (verb) {
                    case 'me':
                        card = this.getAutoRefreshForSpecificUserBaseCard(userId, "Me");
                        await this.sendAdaptiveCard(context, card);
                        break;

                    case 'allusers':
                        card = this.getAutoRefreshForAllUsersBaseCard("All Users");
                        await this.sendAdaptiveCard(context, card);
                        break;

                    case 'UpdateBaseCard':
                        card = this.getFinalBaseCard(actionData);
                        const updateActivity = MessageFactory.attachment(card);
                        updateActivity.id = context.activity.replyToId;
                        await context.updateActivity(updateActivity);
                        break;

                    case 'RefreshUserSpecificView':
                        card = this.getUpdatedCardForUser(userId, actionData);
                        return {
                            status: 200,
                            body: {
                                type: card.contentType,
                                value: card.content
                            }
                        };

                    default:
                        break;
                }

                return {
                    status: 200,
                    body: { value: 'Handled invoke' }
                };
            }

            return {
                status: 200,
                body: { value: 'Unhandled invoke' }
            };
        };
    }

    async sendAdaptiveCard(context, card) {
        await context.sendActivity({ attachments: [CardFactory.adaptiveCard(card.content || card)] });
    }

    async handleCardActionSubmit(context) {
        if (context.activity.value) {
            await context.sendActivity(`You submitted: ${JSON.stringify(context.activity.value)}`);
        }
    }

    getSelectCardTypeCard() {
        return this.renderCard('select-card-type.json', {});
    }

    getAutoRefreshForAllUsersBaseCard(cardType) {
        const data = {
            count: 0,
            cardType,
            cardStatus: "Base",
            trigger: "NA",
            view: "Shared",
            message: "Original Message"
        };
        return this.renderCard('refresh-all-users.json', data);
    }

    getAutoRefreshForSpecificUserBaseCard(userMri, cardType) {
        const data = {
            count: 0,
            cardType,
            cardStatus: "Base",
            trigger: "NA",
            view: "Shared",
            userMri,
            message: "Original Message"
        };
        return this.renderCard('refresh-specific-user.json', data);
    }

    getUpdatedCardForUser(userMri, actionData) {
        const data = {
            count: actionData.action.data.refreshCount,
            cardType: actionData.action.data.cardType,
            cardStatus: "Updated",
            trigger: actionData.trigger,
            view: "Personal",
            userMri,
            message: "Updated Message!"
        };
        return this.renderCard('refresh-specific-user.json', data);
    }

    getFinalBaseCard(actionData) {
        const data = {
            count: actionData.action.data.refreshCount,
            cardType: actionData.action.data.cardType,
            cardStatus: "Final",
            trigger: actionData.trigger,
            view: "Shared",
            message: "Final Message!"
        };
        return this.renderCard('updated-base-card.json', data);
    }

    renderCard(templateFileName, data) {
        const templatePath = path.join(__dirname, `../assets/templates/${templateFileName}`);
        const templateJson = fs.readFileSync(templatePath, 'utf-8');
        const template = new ACData.Template(JSON.parse(templateJson));
        const cardPayload = template.expand({ $root: data });

        return CardFactory.adaptiveCard(cardPayload);
    }
}

module.exports.UserSpecificAdaptiveCardActionBot = UserSpecificAdaptiveCardActionBot;

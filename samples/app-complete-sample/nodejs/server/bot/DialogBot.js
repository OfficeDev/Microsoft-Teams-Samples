// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, ActionTypes, CardFactory } = require('botbuilder');
const request = require('request-promise');
const searchApiUrlFormat = "https://en.wikipedia.org/w/api.php?action=query&list=search&srsearch=[keyword]&srlimit=[limit]&sroffset=[offset]&format=json";

/**
 * DialogBot class extends TeamsActivityHandler to handle Teams-specific events and interactions.
 */
class DialogBot extends TeamsActivityHandler {
    /**
     * Constructor for the DialogBot class.
     * @param {ConversationState} conversationState - The state management object for conversation state.
     * @param {UserState} userState - The state management object for user state.
     * @param {Dialog} dialog - The dialog to be used by the bot.
     */
    constructor(conversationState, userState, dialog) {
        super();
        if (!conversationState) throw new Error('[DialogBot]: Missing parameter. conversationState is required');
        if (!userState) throw new Error('[DialogBot]: Missing parameter. userState is required');
        if (!dialog) throw new Error('[DialogBot]: Missing parameter. dialog is required');

        this.conversationState = conversationState;
        this.userState = userState;
        this.dialog = dialog;
        this.dialogState = this.conversationState.createProperty('DialogState');
        this.userStateAccessor = this.userState.createProperty('userdata');

        this.onMessage(async (context, next) => {
            console.log('Running dialog with Message Activity.');
            // Run the Dialog with the new message Activity.
            await this.dialog.run(context, this.dialogState);
            await next();
        });
    }

    /**
     * Override the ActivityHandler.run() method to save state changes after the bot logic completes.
     * @param {TurnContext} context - The context object for the turn.
     */
    async run(context) {
        await super.run(context);

        // Save any state changes. The load happened during the execution of the Dialog.
        await this.conversationState.saveChanges(context, false);
        await this.userState.saveChanges(context, false);
    }

    /**
     * Handles Teams Messaging Extension Query.
     * @param {TurnContext} context - The context object for the turn.
     * @param {Object} query - The query object for the messaging extension.
     */
    async handleTeamsMessagingExtensionQuery(context, query) {
        const manifestInitialRun = "initialRun";
        const manifestParameterName = "query";
        const initialRunParameter = this.getQueryParameterByName(query, manifestInitialRun);
        const queryParameter = this.getQueryParameterByName(query, manifestParameterName);
        const userData = await this.userStateAccessor.get(context, {});
        userData.botId = context.activity.recipient.id;
        await this.userState.saveChanges(context, false);

        if (!userData) {
            return {
                composeExtension: {
                    type: 'message',
                    text: 'ERROR: No user data'
                }
            };
        }

        // Handle various states
        if (query.state) {
            const settingsState = JSON.parse(query.state);
            if (settingsState.cardType) {
                userData.composeExtensionCardType = settingsState.cardType;
                await this.userState.saveChanges(context, false);
            }
            queryParameter = "";
            initialRunParameter = "true";
        }

        if (!userData.composeExtensionCardType && userData.channelId === "msteams") {
            return this.getConfigResponse();
        }

        if (queryParameter.toLowerCase() === 'reset') {
            userData.composeExtensionCardType = null;
            await this.userState.saveChanges(context, false);
            return {
                composeExtension: {
                    type: 'message',
                    text: 'Your compose extension state has been reset'
                }
            };
        }

        if (queryParameter.toLowerCase() === 'setting' || queryParameter.toLowerCase() === 'settings') {
            return this.getConfigResponse();
        }

        if (initialRunParameter) {
            return {
                composeExtension: {
                    type: 'message',
                    text: 'This Compose Extension is used to make queries to Wikipedia. To change your settings either enter the word "settings" or change your settings on the settings menu option. To reset your configuration, simply enter the word "reset".'
                }
            };
        }

        // Call Wikipedia API and create the response for a query
        const searchApiUrl = encodeURI(searchApiUrlFormat.replace("[keyword]", queryParameter).replace("[limit]", query.queryOptions.count).replace("[offset]", query.queryOptions.skip));
        const promisesOfCardsAsAttachments = [];

        return new Promise((resolve, reject) => {
            request(searchApiUrl, (error, res, body) => {
                const wikiResults = JSON.parse(body).query.search;
                wikiResults.forEach((wikiResult) => {
                    let highlightedTitle = wikiResult.title;
                    if (queryParameter) {
                        const matches = highlightedTitle.match(new RegExp(queryParameter, "gi"));
                        if (matches && matches.length > 0) {
                            highlightedTitle = highlightedTitle.replace(new RegExp(queryParameter, "gi"), `<b>${matches[0]}</b>`);
                        }
                    }
                    highlightedTitle = `<a href="https://en.wikipedia.org/wiki/${encodeURI(wikiResult.title)}" target="_blank">${highlightedTitle}</a>`;
                    const cardText = `${wikiResult.snippet} ...`;

                    let card = null;
                    if (userData.channelId !== "msteams") {
                        userData.composeExtensionCardType = "thumbnail";
                    }

                    if (userData.composeExtensionCardType === "thumbnail") {
                        card = CardFactory.thumbnailCard(highlightedTitle, undefined, null, { text: cardText });
                    } else {
                        card = CardFactory.heroCard(highlightedTitle, undefined, null, { text: cardText });
                    }

                    const previewCard = CardFactory.thumbnailCard(highlightedTitle, undefined, null, { text: cardText });
                    const attachment = { ...card, previewCard };
                    promisesOfCardsAsAttachments.push(attachment);
                });

                resolve({
                    composeExtension: {
                        type: 'result',
                        attachmentLayout: 'list',
                        attachments: promisesOfCardsAsAttachments
                    }
                });
            });
        });
    }

    /**
     * Handles Teams Messaging Extension Select Item.
     * @param {TurnContext} context - The context object for the turn.
     * @param {Object} obj - The selected item object.
     */
    async handleTeamsMessagingExtensionSelectItem(context, obj) {
        return {
            composeExtension: {
                type: 'result',
                attachmentLayout: 'list',
                attachments: [CardFactory.thumbnailCard(obj.description)]
            }
        };
    }

    /**
     * Handles Teams O365 Connector Card Action.
     * @param {TurnContext} context - The context object for the turn.
     * @param {Object} query - The query object for the O365 connector card action.
     */
    async handleTeamsO365ConnectorCardAction(context, query) {
        const text = `Thanks, ${context.activity.from.name}\nYour input action ID: ${query.actionId}\nYour input body: ${query.Body}`;
        await context.sendActivity(text);
    }

    /**
     * Returns the value of the specified query parameter.
     * @param {Object} query - The query object.
     * @param {string} name - The name of the query parameter.
     * @returns {string} The value of the query parameter.
     */
    getQueryParameterByName(query, name) {
        const matchingParams = (query.parameters || []).filter(p => p.name === name);
        return matchingParams.length ? matchingParams[0].value : "";
    }

    /**
     * Returns the configuration response for the compose extension.
     * @returns {Object} The configuration response object.
     */
    getConfigResponse() {
        const hardCodedUrl = `${process.env.BaseUri}/composeExtension/composeExtensionSettings.html?width=5000&height=5000`;
        const cardAction = {
            type: ActionTypes.OpenUrl,
            title: "Config",
            value: hardCodedUrl
        };
        return {
            composeExtension: {
                type: 'config',
                text: 'ERROR: No user data',
                suggestedActions: {
                    actions: [cardAction]
                }
            }
        };
    }
}

module.exports.DialogBot = DialogBot;
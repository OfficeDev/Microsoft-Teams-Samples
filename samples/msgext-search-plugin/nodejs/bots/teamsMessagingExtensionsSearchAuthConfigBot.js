// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const {
    TeamsActivityHandler,
    CardFactory
} = require('botbuilder');

const {
} = require('botbuilder-core');

const axios = require('axios');

class TeamsMessagingExtensionsSearchAuthConfigBot extends TeamsActivityHandler {
    /**
     *
     * @param {UserState} User state to persist configuration settings
     */
    constructor(userState) {
        super();
        this.connectionName = process.env.ConnectionName;
        this.userState = userState;
    }

    /**
     * Override the ActivityHandler.run() method to save state changes after the bot logic completes.
     */
    async run(context) {
        await super.run(context);

        // Save state changes
        await this.userState.saveChanges(context);
    }

    // Overloaded function. Receives invoke activities with the name 'composeExtension/query'.
    async handleTeamsMessagingExtensionQuery(context, query) {
        const searchQuery = query.parameters[0].value;
        const attachments = [];

        // When the Bot Service Auth flow completes, the query.State will contain a magic code used for verification.
        const userTokenClient = context.turnState.get(context.adapter.UserTokenClientKey);
        const magicCode =
            context.state && Number.isInteger(Number(context.state))
                ? context.state
                : '';

        const tokenResponse = await userTokenClient.getUserToken(
            context.activity.from.id,
            this.connectionName,
            context.activity.channelId,
            magicCode
        );

        if (!tokenResponse || !tokenResponse.token) {
            // There is no token, so the user has not signed in yet.
            // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
            const { signInLink } = await userTokenClient.getSignInResource(
                this.connectionName,
                context.activity
            );

            return {
                composeExtension: {
                    type: 'silentAuth',
                    suggestedActions: {
                        actions: [
                            {
                                type: 'openUrl',
                                value: signInLink,
                                title: 'Bot Service OAuth'
                            }
                        ]
                    }
                }
            };
        }

        // The user is signed in, so use the token to create a Graph Clilent and show profile
        console.log(tokenResponse.token);

        const filterQuery = searchQuery + ' ' + 'Approved' + ' path:"https://' + process.env.SharePointDomain + '/sites/' + process.env.SharePointSiteName + '/Lists/' + process.env.SharePointListName + '"';
        const response = await axios.post('https://graph.microsoft.com/v1.0/search/query', {
            'requests': [
                {
                    'entityTypes': [
                        'listItem'
                    ],
                    'query': {
                        'queryString': filterQuery
                    },
                    'size': 10
                }
            ]
        }, {
            headers: {
                'Authorization': 'Bearer ' + tokenResponse.token,
                'Content-Type': 'application/json'
            }
        });

        if (response != null && response !== 'undefined' && response.data != null && response.data !== 'undefined') {
            if (response.data.value != null) {
                const hits = response.data.value[0].hitsContainers[0].hits;

                if (hits != null && hits !== 'undefined') {
                    const finalDetails = hits.map(function(obj, index) {
                        return {
                            id: index + 1,
                            method: 'GET',
                            url: '/sites/' + obj.resource.parentReference.siteId + '/lists/' + obj.resource.sharepointIds.listId + '/items/' + obj.resource.sharepointIds.listItemId + '?expand=fields'
                        };
                    });

                    const responseListItems = await axios.post('https://graph.microsoft.com/v1.0/$batch', {
                        'requests': finalDetails
                    }, {
                        headers: {
                            'Authorization': 'Bearer ' + tokenResponse.token,
                            'Content-Type': 'application/json'
                        }
                    });

                    const Field1 = process.env.SharePointMappingField1;
                    const Field2 = process.env.SharePointMappingField2;
                    const Field3 = process.env.SharePointMappingField3;
                    const Field4 = process.env.SharePointMappingField4;
                    const FieldDisplayName1 = process.env.FieldDisplayName1;
                    const FieldDisplayName2 = process.env.FieldDisplayName2;
                    const FieldDisplayName3 = process.env.FieldDisplayName3;
                    const FieldDisplayName4 = process.env.FieldDisplayName4;

                    responseListItems.data.responses.forEach(obj => {
                        const listItemUrl = 'https://' + process.env.SharePointDomain + '/sites/' + process.env.SharePointSiteName + '/Lists/' + process.env.SharePointListName + '/DispForm.aspx?ID=' + obj.body.id;
                        const userCard = CardFactory.adaptiveCard(
                            this.getLinkUnfurlingCard(
                                obj.body.fields[Field1],
                                obj.body.fields[Field2],
                                obj.body.fields[Field3],
                                obj.body.fields[Field4],
                                listItemUrl,
                                FieldDisplayName1,
                                FieldDisplayName2,
                                FieldDisplayName3,
                                FieldDisplayName4
                            ));

                        const preview = CardFactory.thumbnailCard(
                            FieldDisplayName2 + ': ' + (obj.body.fields[process.env.SharePointMappingField2] !== undefined ? obj.body.fields[process.env.SharePointMappingField2]?.substring(0, 100) : 'NA'),
                            FieldDisplayName3 + ': ' + (obj.body.fields[process.env.SharePointMappingField3] !== undefined ? obj.body.fields[process.env.SharePointMappingField3]?.substring(0, 100) : 'NA')
                        );

                        const attachment = { ...userCard, preview };
                        attachments.push(attachment);
                    });

                    return {
                        composeExtension: {
                            type: 'result',
                            attachmentLayout: 'list',
                            attachments: attachments
                        }
                    };
                } else {
                    return null;
                }
            }
        }

        return {
            composeExtension: {
                type: 'result',
                attachmentLayout: 'list',
                attachments: [CardFactory.thumbnailCard('No Data Found.')]
            }
        };
    }

    async onInvokeActivity(context) {
        console.log('onInvoke, ' + context.activity.name);
        const valueObj = context.activity.value;

        if (valueObj.authentication) {
            const authObj = valueObj.authentication;
            if (authObj.token) {
                // If the token is NOT exchangeable, then do NOT deduplicate requests.
                if (await this.tokenIsExchangeable(context)) {
                    return await super.onInvokeActivity(context);
                } else {
                    const response =
                    {
                        status: 412
                    };

                    return response;
                }
            }
        }

        return await super.onInvokeActivity(context);
    }

    async tokenIsExchangeable(context) {
        let tokenExchangeResponse = null;
        try {
            const userId = context.activity.from.id;
            const valueObj = context.activity.value;
            const tokenExchangeRequest = valueObj.authentication;
            console.log('tokenExchangeRequest.token: ' + tokenExchangeRequest.token);

            const userTokenClient = context.turnState.get(context.adapter.UserTokenClientKey);

            tokenExchangeResponse = await userTokenClient.exchangeToken(
                userId,
                this.connectionName,
                context.activity.channelId,
                { token: tokenExchangeRequest.token });

            console.log('tokenExchangeResponse: ' + JSON.stringify(tokenExchangeResponse));
        } catch (err) {
            console.log('tokenExchange error: ' + err);
            // Ignore Exceptions
            // If token exchange failed for any reason, tokenExchangeResponse above stays null , and hence we send back a failure invoke response to the caller.
        }
        if (!tokenExchangeResponse || !tokenExchangeResponse.token) {
            return false;
        }

        console.log('Exchanged token: ' + JSON.stringify(tokenExchangeResponse));
        return true;
    }

    // Adaptive card for link unfurling.
    getLinkUnfurlingCard(
        FieldValue1,
        FieldValue2,
        FieldValue3,
        FieldValue4,
        listItemUrl,
        FieldDisplayName1,
        FieldDisplayName2,
        FieldDisplayName3,
        FieldDisplayName4) {
        const card =
        {
            '$schema': 'http://adaptivecards.io/schemas/adaptive-card.json',
            'type': 'AdaptiveCard',
            'version': '1.4',
            'body': [
                {
                    'type': 'TextBlock',
                    'size': 'Medium',
                    // 'weight': 'Bolder',
                    'text': '**' + FieldDisplayName1 + ':' + '**' + ' ' + (FieldValue1 !== undefined ? FieldValue1 : 'NA'),
                    'wrap': true
                },
                {
                    'type': 'TextBlock',
                    'size': 'Medium',
                    // 'weight': 'Bolder',
                    'text': '**' + FieldDisplayName2 + ':' + '**' + ' ' + (FieldValue2 !== undefined ? FieldValue2 : 'NA'),
                    'wrap': true
                },
                {
                    'type': 'TextBlock',
                    'size': 'Medium',
                    // 'weight': 'Bolder',
                    'text': '**' + FieldDisplayName3 + ':' + '**' + ' ' + (FieldValue3 !== undefined ? FieldValue3 : 'NA'),
                    'wrap': true
                },
                {
                    'type': 'TextBlock',
                    'size': 'Medium',
                    // 'weight': 'Bolder',
                    'text': '**' + FieldDisplayName4 + ':' + '**' + ' ' + (FieldValue4 !== undefined ? FieldValue4 : 'NA'),
                    'wrap': true
                }
            ],
            'actions': [
                {
                    'type': 'Action.OpenUrl',
                    'title': 'View more details',
                    'url': listItemUrl
                }
            ]
        };

        return card;
    }
}

module.exports.TeamsMessagingExtensionsSearchAuthConfigBot = TeamsMessagingExtensionsSearchAuthConfigBot;
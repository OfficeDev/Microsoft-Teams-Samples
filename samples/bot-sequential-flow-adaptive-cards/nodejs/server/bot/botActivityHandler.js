// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { TeamsActivityHandler, CardFactory, TeamsInfo } = require('botbuilder');
const incidentService = require('../services/incidentService');
const adaptiveCards = require('../models/adaptiveCard');

class BotActivityHandler extends TeamsActivityHandler {
    constructor() {
        super();
        /*  Teams bots are Microsoft Bot Framework bots.
            If a bot receives a message activity, the turn handler sees that incoming activity
            and sends it to the onMessage activity handler.
            Learn more: https://aka.ms/teams-bot-basics.

            NOTE:   Ensure the bot endpoint that services incoming conversational bot queries is
                    registered with Bot Framework.
                    Learn more: https://aka.ms/teams-register-bot.
        */
        // Registers an activity event handler for the message event, emitted for every incoming message activity.
        this.onMessage(async (context, next) => {
            console.log('Running on Message Activity.');
            await this.startIncManagement(context);
            await next();
        });
    }

    async onInvokeActivity(context) {
        console.log('Activity: ', context.activity.name);
        const user = context.activity.from;
        const action = context.activity.value.action;

        if(context.activity.name == "composeExtension/submitAction") {
            let choiceset = [];
            const incidents = await incidentService.getAllInc();
            if(context.activity.value.data.msteams != null) {
                incidents.map(inc => {
                    let choiceData = {
                        title: `Incident title: ${inc.title}, Created by: ${inc.createdBy.name}`,
                        value: inc.id
                    }
    
                    choiceset.push(choiceData);
                });

                const incidentCard = CardFactory.adaptiveCard(await adaptiveCards.incidentListCard(choiceset));

                return adaptiveCards.invokeIncidentTaskResponse("Select incident", incidentCard);
            }

            var incidentData = context.activity.value.data;
            const incident = incidents.find(inc => inc.id == incidentData.incidentId);
            var refreshCard = CardFactory.adaptiveCard(await adaptiveCards.refreshBotCard(incident));
            await context.sendActivity({
                attachments: [refreshCard]
            });

            return adaptiveCards.invokeResponse(refreshCard);
        }

        if(context.activity.name == "composeExtension/fetchTask") {
           
            try {
                let choiceset = [];
                const allMembers = await (await TeamsInfo.getMembers(context)).filter(tm => tm.aadObjectId);
                const incidents = await incidentService.getAllInc();

                if(incidents.length == 0) {
                    const noIncidentFound = CardFactory.adaptiveCard({
                        version: '1.0.0',
                        type: 'AdaptiveCard',
                        body: [
                            {
                                type: 'TextBlock',
                                text: 'No incident found.',
                                size: "large",
                                weight: "bolder"
                            },
                            {
                                type: 'TextBlock',
                                text: 'Please create a incident using bot.',
                                size: "medium",
                                weight: "bolder"
                            }
                        ]
                    });

                    return adaptiveCards.invokeTaskResponse("No Incident found", noIncidentFound);
                }

                incidents.map(inc => {
                    let choiceData = {
                        title: `Incident title: ${inc.title}, Created by: ${inc.createdBy.name}`,
                        value: inc.id
                    }
    
                    choiceset.push(choiceData);
                });

                const incidentCard = CardFactory.adaptiveCard(await adaptiveCards.incidentListCard(choiceset));

                return adaptiveCards.invokeIncidentTaskResponse("Select incident", incidentCard);
            }
            catch (error) {

                if(error.code == "BotNotInConversationRoster") {
                    const botInstallationCard = CardFactory.adaptiveCard({
                        version: '1.0.0',
                        type: 'AdaptiveCard',
                        body: [
                            {
                                type: 'TextBlock',
                                text: "Looks like you haven't used bot in team/chat"
                            },
                        ],
                        actions: [
                            {
                              type: "Action.Submit",
                              title: "Continue",
                              data: {
                                msteams: {
                                    justInTimeInstall: true
                                  }
                              }
                            }
                          ]
                    });

                    return adaptiveCards.invokeTaskResponse("Bot is not installed", botInstallationCard); 
                }
            }
        }
        
        if (context.activity.name === 'adaptiveCard/action') {
            const action = context.activity.value.action;
            console.log('Verb: ', action.verb);
            const allMembers = await (await TeamsInfo.getMembers(context)).filter(tm => tm.aadObjectId);
            const responseCard = await adaptiveCards.selectResponseCard(context, user, allMembers);
            return adaptiveCards.invokeResponse(responseCard);
        }
    }

    async startIncManagement(context) {
        await context.sendActivity({
            attachments: [CardFactory.adaptiveCard(adaptiveCards.optionInc())]
        });
    }
}

module.exports.BotActivityHandler = BotActivityHandler;

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Incident Management Bot using Teams SDK v2
// Implements sequential workflow with adaptive cards

const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const incidentService = require('./services/incidentService');
const adaptiveCards = require('./models/adaptiveCard');

// Create storage for conversation history
const storage = new LocalStorage();

const createTokenFactory = () => {
  return async (scope, tenantId) => {
    const managedIdentityCredential = new ManagedIdentityCredential({
      clientId: process.env.CLIENT_ID,
    });
    const scopes = Array.isArray(scope) ? scope : [scope];
    const tokenResponse = await managedIdentityCredential.getToken(scopes, {
      tenantId: tenantId,
    });

    return tokenResponse.token;
  };
};

// Configure authentication using TokenCredentials
const tokenCredentials = {
  clientId: process.env.CLIENT_ID || "",
  token: createTokenFactory(),
};

const credentialOptions =
  config.MicrosoftAppType === "UserAssignedMsi" ? { ...tokenCredentials } : undefined;

// Create the app with storage
const app = new App({
  ...credentialOptions,
  storage,
});

// Error handling for the application
app.on("error", async (context, error) => {
    console.error(`\n [Application error] unhandled error: ${error}`);
});

// Add a generic invoke handler to catch all invokes
app.on("invoke", async (context) => {
    // Handle adaptiveCard/action invokes
    if (context.activity.name === 'adaptiveCard/action') {
        try {
            const user = context.activity.from;
            const data = context.activity.value;
            const verb = data?.action?.verb;
            
            // Get all members for user-specific views
            const allMembers = await getTeamMembers(context);
            
            // Route to appropriate handler based on action verb
            const responseCard = await adaptiveCards.selectResponseCard(context, user, allMembers);
            
            // If this is a save, update, or status action, send the result as a new message in the channel
            if (verb && (verb.startsWith('save_new_inc') || verb.startsWith('update_inc') || verb.startsWith('status_'))) {
                try {
                    // Send the incident card as a new message
                    await context.send({
                        type: 'message',
                        attachments: [{
                            contentType: 'application/vnd.microsoft.card.adaptive',
                            content: responseCard
                        }]
                    });
                    
                    // Return a simple confirmation for the original card
                    const confirmationCard = {
                        $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
                        type: 'AdaptiveCard',
                        version: '1.4',
                        body: [
                            {
                                type: 'TextBlock',
                                size: 'Medium',
                                weight: 'Bolder',
                                text: 'Incident Management'
                            },
                            {
                                type: 'TextBlock',
                                text: verb.startsWith('save_new_inc') ? '✅ Incident saved successfully!' : 
                                      verb.startsWith('update_inc') ? '✅ Incident updated successfully!' :
                                      '✅ Status updated successfully!',
                                wrap: true,
                                color: 'Good',
                                size: 'Large',
                                weight: 'Bolder'
                            },
                            {
                                type: 'TextBlock',
                                text: 'The incident details have been posted below.',
                                wrap: true,
                                spacing: 'Small'
                            }
                        ]
                    };
                    
                    return {
                        statusCode: 200,
                        type: 'application/vnd.microsoft.card.adaptive',
                        value: confirmationCard
                    };
                } catch (sendError) {
                    console.error('Error sending new message:', sendError);
                    // Fall back to returning the response card normally
                }
            }
            
            // Return invoke response to update the card
            return {
                statusCode: 200,
                type: 'application/vnd.microsoft.card.adaptive',
                value: responseCard
            };
        } catch (error) {
            console.error('Error handling adaptive card action:', error);
            return {
                statusCode: 500,
                type: 'application/vnd.microsoft.error',
                value: { message: error.message }
            };
        }
    }
});

// Message handler - handles incoming messages and initiates incident management workflow
// Only respond to actual text messages, not adaptive card actions
app.on("message", async (context) => {
    const activity = context.activity;
    const text = stripMentionsText(activity);

    // Skip if this is an adaptive card action invoke
    if (activity.type === 'invoke' || activity.name === 'adaptiveCard/action') {
        return;
    }
    
    await startIncManagement(context);
});

// Adaptive Card action handler using Teams SDK v2 approach
app.on("adaptiveCard/action", async (context) => {
    try {
        const activity = context.activity;
        const user = activity.from;
        const data = activity.value;
        const verb = data?.action?.verb;        
        // Get all members for user-specific views
        const allMembers = await getTeamMembers(context);
        
        // Route to appropriate handler based on action verb
        const responseCard = await adaptiveCards.selectResponseCard(context, user, allMembers);
        
        // If this is a save, update, or status action, send the result as a new message in the channel
        if (verb && (verb.startsWith('save_new_inc') || verb.startsWith('update_inc') || verb.startsWith('status_'))) {
            try {
                // Send the incident card as a new message
                await context.send({
                    type: 'message',
                    attachments: [{
                        contentType: 'application/vnd.microsoft.card.adaptive',
                        content: responseCard
                    }]
                });
                
                // Return a simple confirmation for the original card
                const confirmationCard = {
                    $schema: 'http://adaptivecards.io/schemas/adaptive-card.json',
                    type: 'AdaptiveCard',
                    version: '1.4',
                    body: [
                        {
                            type: 'TextBlock',
                            size: 'Medium',
                            weight: 'Bolder',
                            text: 'Incident Management'
                        },
                        {
                            type: 'TextBlock',
                            text: verb.startsWith('save_new_inc') ? '✅ Incident saved successfully!' : 
                                  verb.startsWith('update_inc') ? '✅ Incident updated successfully!' :
                                  '✅ Status updated successfully!',
                            wrap: true,
                            color: 'Good',
                            size: 'Large',
                            weight: 'Bolder'
                        },
                        {
                            type: 'TextBlock',
                            text: 'The incident details have been posted below.',
                            wrap: true,
                            spacing: 'Small'
                        }
                    ]
                };
                
                return {
                    statusCode: 200,
                    type: 'application/vnd.microsoft.card.adaptive',
                    value: confirmationCard
                };
            } catch (sendError) {
                console.error('Error sending new message:', sendError);
                // Fall back to returning the response card normally
            }
        }
        
        // Return invoke response to update the card
        return {
            statusCode: 200,
            type: 'application/vnd.microsoft.card.adaptive',
            value: responseCard
        };
    } catch (error) {
        console.error('Error in adaptiveCard/action:', error);
        return {
            statusCode: 500,
            type: 'application/vnd.microsoft.error',
            value: { message: error.message }
        };
    }
});

// Message Extension: Fetch Task handler
app.on("messageExtensions/fetchTask", async (context) => {
    try {
        let choiceset = [];
        const allMembers = await getTeamMembers(context);
        const incidents = await incidentService.getAllInc();

        if (incidents.length === 0) {
            // Return card indicating no incidents found
            const noIncidentFound = {
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
            };

            return adaptiveCards.invokeTaskResponse("No Incident found", noIncidentFound);
        }

        // Build incident choice list
        incidents.map(inc => {
            let choiceData = {
                title: `Incident title: ${inc.title}, Created by: ${inc.createdBy.name}`,
                value: inc.id
            };

            choiceset.push(choiceData);
        });

        const incidentCard = await adaptiveCards.incidentListCard(choiceset);
        return adaptiveCards.invokeIncidentTaskResponse("Select incident", incidentCard);
    } catch (error) {
        // Handle bot not installed in conversation roster error
        if (error.code === "BotNotInConversationRoster") {
            const botInstallationCard = {
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
            };

            return adaptiveCards.invokeTaskResponse("Bot is not installed", botInstallationCard);
        }
        throw error;
    }
});

// Message Extension: Submit Action handler
app.on("messageExtensions/submitAction", async (context) => {
    let choiceset = [];
    const incidents = await incidentService.getAllInc();
    const actionData = context.activity.value.data;
    
    // Check if this is a just-in-time installation flow
    if (actionData && actionData.msteams != null) {
        incidents.map(inc => {
            let choiceData = {
                title: `Incident title: ${inc.title}, Created by: ${inc.createdBy.name}`,
                value: inc.id
            };

            choiceset.push(choiceData);
        });

        const incidentCard = await adaptiveCards.incidentListCard(choiceset);
        return adaptiveCards.invokeIncidentTaskResponse("Select incident", incidentCard);
    }

    // Handle incident selection and send to chat
    var incidentData = actionData;
    const incident = incidents.find(inc => inc.id === incidentData.incidentId);
    var refreshCard = await adaptiveCards.refreshBotCard(incident);
    
    await context.send({
        type: 'message',
        attachments: [{
            contentType: 'application/vnd.microsoft.card.adaptive',
            content: refreshCard
        }]
    });

    return {
        status: 200,
        body: refreshCard
    };
});

// Helper function to get team members with AAD object IDs
// Uses Teams SDK v2 methods to get conversation members
async function getTeamMembers(context) {
    try {
        // In Teams SDK v2, members API returns a promise
        // The structure is: context.api.conversations.members(conversationId).get()
        const conversationId = context.activity.conversation.id;
        
        // Get members using Teams SDK v2 API
        const membersResponse = await context.api.conversations.members(conversationId).get();
        
        // The response might be an array directly or wrapped in a value property
        let membersList = Array.isArray(membersResponse) ? membersResponse : (membersResponse.value || []);
        
        // Normalize and return members
        const normalizedMembers = membersList.map(tm => {
            return {
                id: tm.id,
                name: tm.name || tm.givenName || tm.displayName || 'Unknown User',
                aadObjectId: tm.aadObjectId || tm.objectId || tm.id,
                userPrincipalName: tm.userPrincipalName,
                email: tm.email || tm.userPrincipalName
            };
        });
        
        return normalizedMembers;
    } catch (error) {
        console.error('Error getting team members:', error);
        
        // Fallback: Return current user
        return [{
            id: context.activity.from.id,
            name: context.activity.from.name || 'Current User',
            aadObjectId: context.activity.from.aadObjectId || context.activity.from.id,
            userPrincipalName: context.activity.from.userPrincipalName,
            email: context.activity.from.email
        }];
    }
}

// Send initial incident management card
// Initiates the incident management workflow
async function startIncManagement(context) {
    try {
        const card = adaptiveCards.optionInc();
        
        await context.send({
            type: 'message',
            attachments: [{
                contentType: 'application/vnd.microsoft.card.adaptive',
                content: card
            }]
        });
    } catch (error) {
        console.error('Error sending card:', error);
        throw error;
    }
}

module.exports = app;

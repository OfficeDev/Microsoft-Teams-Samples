// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import AgentsHosting from "@microsoft/agents-hosting";
import faker from 'faker';

const { ActivityHandler, CardFactory } = AgentsHosting;

// Helper function to remove bot mentions from a message
function removeRecipientMention(activity) {
    const text = activity.text || "";
    const recipientId = activity.recipient?.id;

    if (!recipientId) return text;

    // Remove <at>mention</at> tags for the bot
    return text.replace(/<at>.*?<\/at>/g, "").trim();
}

// Bot class that handles messages and messaging extension queries
export class EchoBot extends ActivityHandler {
    constructor() {
        super();

        // Handle incoming text messages
        this.onMessage(async (context, next) => {
            const text = removeRecipientMention(context.activity);

            if (text) {
                await context.sendActivity(`You said: ${text}`);
            }

            await next();
        });

        // Handle new members added to the team
        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (let member of membersAdded) {
                if (member.id !== context.activity.recipient.id) {
                    const welcomeMessage = `Welcome to the App hello world sample ${member.name || 'there'}!`;
                    await context.sendActivity(welcomeMessage);
                }
            }
            await next();
        });
    }

    // Override run method to handle invoke activities for message extensions
    async run(context) {
        // Handle messaging extension query
        if (context.activity.type === 'invoke') {
            if (context.activity.name === 'composeExtension/query') {
                const query = context.activity.value;
                const response = await this.handleTeamsMessagingExtensionQuery(context, query);
                return { status: 200, body: response };
            } 
            // Handle messaging extension item selection
            else if (context.activity.name === 'composeExtension/selectItem') {
                const item = context.activity.value;
                const response = await this.handleTeamsMessagingExtensionSelectItem(context, item);
                return { status: 200, body: response };
            }
        }
        
        // Process other activity types through the base handler
        await super.run(context);
    }

    // Generate 5 random cards for messaging extension query
    async handleTeamsMessagingExtensionQuery(context, query) {
        const title = query?.parameters?.[0]?.value || query?.commandId || faker.lorem.sentence();
        const randomImageUrl = "https://loremflickr.com/200/200";

        if (query.commandId === 'getRandomText') {
            const attachments = [];

            // Create 5 thumbnail cards with random text and images
            for (let i = 0; i < 5; i++) {
                const text = faker.lorem.paragraph();
                const images = [`${randomImageUrl}?random=${i}`];

                // Create main card and preview card
                const thumbnailCard = CardFactory.thumbnailCard(title || faker.lorem.words(3), text, images);
                const preview = CardFactory.thumbnailCard(title || faker.lorem.words(3), text, images);

                attachments.push({ ...thumbnailCard, preview });
            }

            return {
                composeExtension: {
                    type: 'result',
                    attachmentLayout: 'list',
                    attachments: attachments
                }
            };
        }

        return null;
    }

    // Handle selected item from messaging extension results
    async handleTeamsMessagingExtensionSelectItem(context, obj) {
        const { title, text, images } = obj;

        return {
            composeExtension: {
                type: 'result',
                attachmentLayout: 'list',
                attachments: [CardFactory.thumbnailCard(title, text, images)]
            }
        };
    }
}
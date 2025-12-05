// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import AgentsHosting from "@microsoft/agents-hosting";
import * as TeamsExtensions from "@microsoft/agents-hosting-extensions-teams";
import faker from 'faker';

const { CardFactory } = AgentsHosting;
const { TeamsActivityHandler } = TeamsExtensions;

// Helper function to remove bot mentions from a message
function removeRecipientMention(activity) {
    const text = activity.text || "";
    const recipientId = activity.recipient?.id;

    if (!recipientId) return text;

    // Remove <at>mention</at> tags for the bot
    return text.replace(/<at>.*?<\/at>/g, "").trim();
}

// Bot class that handles messages and messaging extension queries
export class EchoBot extends TeamsActivityHandler {
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

    // Generate 5 random cards for messaging extension query
    async handleTeamsMessagingExtensionQuery(context, query) {
        try {
            const title = query?.parameters?.[0]?.value || query?.commandId || faker.lorem.sentence();
            const randomImageUrl = "https://loremflickr.com/200/200";

            if (query?.commandId === 'getRandomText') {
                const attachments = [];
                for (let i = 0; i < 5; i++) {
                    const text = faker.lorem.paragraph();
                    const images = [`${randomImageUrl}?random=${i}`];
                    const cardTitle = title || faker.lorem.words(3);
                    const thumbnailCard = CardFactory.thumbnailCard(cardTitle, text, images);
                    const preview = CardFactory.thumbnailCard(cardTitle, text, images);
                    attachments.push({ ...thumbnailCard, preview });
                }
                return {
                    composeExtension: {
                        type: 'result',
                        attachmentLayout: 'list',
                        attachments
                    }
                };
            }

            // Return an empty list instead of null to avoid "Unable to reach app" UI
            return {
                composeExtension: {
                    type: 'result',
                    attachmentLayout: 'list',
                    attachments: []
                }
            };
        } catch (err) {
            console.error('[msgext] query error', err);
            // Graceful failure response for Teams client
            return {
                composeExtension: {
                    type: 'result',
                    attachmentLayout: 'list',
                    attachments: []
                }
            };
        }
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
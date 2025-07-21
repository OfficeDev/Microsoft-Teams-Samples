import { TeamsActivityHandler, CardFactory } from 'botbuilder';
import faker from 'faker';

export default class MessageExtension extends TeamsActivityHandler {

    /**
     * Handles messaging extension queries such as retrieving random text and images.
     * 
     * @param {TurnContext} context - The context for the current request.
     * @param {object} query - The query data from the messaging extension.
     * @returns {object} A response containing the attachments (cards).
     */
    async handleTeamsMessagingExtensionQuery(context, query) {
        // Default title from user input or fallback to a randomly generated title.
        const title = query.parameters?.[0]?.name === 'cardTitle' ? query.parameters[0].value : faker.lorem.sentence();

        const randomImageUrl = "https://loremflickr.com/200/200"; // Using random image generator (fallback URL)

        switch (query.commandId) {
            case 'getRandomText':
                const attachments = [];

                // Generate 5 results, each with a fake paragraph and image.
                for (let i = 0; i < 5; i++) {
                    const text = faker.lorem.paragraph(); // Generate random text.
                    const images = [`${randomImageUrl}?random=${i}`]; // Ensure unique images per result.

                    // Create a thumbnail card with the generated content.
                    const thumbnailCard = CardFactory.thumbnailCard(title, text, images);
                    const preview = CardFactory.thumbnailCard(title, text, images);

                    // Set up tap action for the preview card.
                    preview.content.tap = { type: 'invoke', value: { title, text, images } };

                    // Push the thumbnail card with preview to the attachments array.
                    attachments.push({ ...thumbnailCard, preview });
                }

                return {
                    composeExtension: {
                        type: 'result',
                        attachmentLayout: 'list',
                        attachments: attachments
                    }
                };

            default:
                // Return null if no matching commandId is found.
                return null;
        }
    }

    /**
     * Handles item selection in the messaging extension.
     * 
     * @param {TurnContext} context - The context for the current request.
     * @param {object} obj - The selected item from the messaging extension.
     * @returns {object} A response containing the selected item (card).
     */
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

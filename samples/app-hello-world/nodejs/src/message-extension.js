import {TeamsActivityHandler, CardFactory} from 'botbuilder';
import faker from 'faker';

/**
 * A class for handle message activities.
 */
export default class MessageExtension extends TeamsActivityHandler {
  async handleTeamsMessagingExtensionQuery(context, query) {
    // If the user supplied a title via the cardTitle parameter then use it or use a fake title
    const title = query.parameters && query.parameters[0].name === 'cardTitle' ?
            query.parameters[0].value :
            faker.lorem.sentence();

    const randomImageUrl = 'https://loremflickr.com/200/200'; // Faker's random images uses lorempixel.com, which has been down a lot

    switch (query.commandId) {
      case 'getRandomText':
        const attachments = [];

        // Generate 5 results to send with fake text and fake images
        for (let i = 0; i < 5; i++) {
          const text = faker.lorem.paragraph();
          const images = [`${randomImageUrl}?random=${i}`];
          const thumbnailCard = CardFactory.thumbnailCard(title, text, images);
          const preview = CardFactory.thumbnailCard(title, text, images);
          preview.content.tap = {type: 'invoke', value: {title, text, images}};
          const attachment = {...thumbnailCard, preview};
          attachments.push(attachment);
        }

        return {
          composeExtension: {
            type: 'result',
            attachmentLayout: 'list',
            attachments: attachments,
          },
        };

      default:
        break;
    }
    return null;
  }

  async handleTeamsMessagingExtensionSelectItem(context, obj) {
    const {title, text, images} = obj;

    return {
      composeExtension: {
        type: 'result',
        attachmentLayout: 'list',
        attachments: [CardFactory.thumbnailCard(title, text, images)],
      },
    };
  }
}

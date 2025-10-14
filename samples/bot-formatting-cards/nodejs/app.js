const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const { ManagedIdentityCredential } = require("@azure/identity");
const path = require('path');
const dotenv = require('dotenv');

// Import card templates
const MentionSupport = require('./resources/mentionSupport.json');
const InformationMaskingCard = require('./resources/informationMasking.json');
const SampleAdaptiveCard = require('./resources/sampleAdaptiveWithFullWidth.json');
const StageViewImagesCard = require('./resources/stageViewForImages.json');
const OverFlowMenuCard = require('./resources/overflowMenu.json');
const HTMLConnectorCard = require('./resources/formatHTMLConnectorCard.json');
const CardWithEmoji = require('./resources/adaptiveCardWithEmoji.json');
const PeoplePersonaCardIcon = require('./resources/adaptivePeoplePersonaCardIcon.json');
const PeoplePersonaCardSetIcon = require('./resources/adaptivePeoplePersonaCardSetIcon.json');
const CodeBlocksCard = require('./resources/codeBlocksCard.json');
const AdaptiveCardResponsiveLayout = require('./resources/AdaptiveCardResponsiveLayout.json');
const AdaptiveCardBorders = require('./resources/adaptiveCardBorders.json');
const AdaptiveCardRoundedCorners = require('./resources/adaptiveCardRoundedCorners.json');
const adaptiveCardFluentIcons = require('./resources/adaptiveCardFluentIcon.json');
const adaptiveCardMediaElements = require('./resources/adaptiveCardMediaElements.json');
const adaptiveCardStarRatings = require('./resources/adaptiveCardStarRatings.json');
const adaptiveCardConditional = require('./resources/adaptiveCardConditional.json');
const adaptiveCardScrollable = require('./resources/adaptiveCardScrollable.json');
const adaptiveCardCompoundButton = require('./resources/adaptiveCardCompoundButton.json');
const adaptiveCardContainerLayouts = require('./resources/adaptiveCardContainerLayouts.json');
const adaptiveCardDonutChart = require('./resources/adaptiveCardDonutChart.json');
const adaptiveCardGaugeChart = require('./resources/adaptiveCardGaugeChart.json');
const adaptiveCardHorizontalBarChart = require('./resources/adaptiveCardHorizontalBarChart.json');
const adaptiveCardHorizontalBarStackedChart = require('./resources/adaptiveCardHorizontalBarStacked.json');
const adaptiveCardLineChart = require('./resources/adaptiveCardLineChart.json');
const adaptiveCardPieChart = require('./resources/adaptiveCardPieChart.json');
const adaptiveCardVerticalBarChart = require('./resources/adaptiveCardVerticalBarChart.json');
const adaptiveCardVerticalBarGroupedChart = require('./resources/adaptiveCardVerticalBarGroupedChart.json');

// Load environment variables
const ENV_FILE = path.join(__dirname, '.env');
dotenv.config({ path: ENV_FILE });

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


/**
 * Handle app installation events to send welcome message
 */
app.on("install.add", async (context) => {
  await context.send('Welcome to Adaptive Card Format. This bot will introduce you to different types of formats. Please select the cards from given options');
  await sendAdaptiveCardFormats(context);
});

/**
 * Handle message events
 */
app.on("message", async (context) => {
  const activity = context.activity;
  const text = stripMentionsText(activity);

  // Define the mapping of card options to their handlers
  const adaptiveFormatCards = {
    'CodeBlock': () => createAdaptiveCardAttachment(CodeBlocksCard),
    'MentionSupport': () => createAdaptiveCardAttachment(MentionSupport),
    'InfoMasking': () => createAdaptiveCardAttachment(InformationMaskingCard),
    'FullWidthCard': () => createAdaptiveCardAttachment(SampleAdaptiveCard),
    'StageViewImages': () => createAdaptiveCardAttachment(StageViewImagesCard),
    'OverflowMenu': () => createAdaptiveCardAttachment(OverFlowMenuCard),
    'HTMLConnector': () => createO365ConnectorCardAttachment(HTMLConnectorCard),
    'CardWithEmoji': () => createAdaptiveCardAttachment(CardWithEmoji),
    'Persona': () => createAdaptiveCardAttachment(PeoplePersonaCardIcon),
    'PersonaSet': () => createAdaptiveCardAttachment(PeoplePersonaCardSetIcon),
    'Layout': () => createAdaptiveCardAttachment(AdaptiveCardResponsiveLayout),
    'Borders': () => createAdaptiveCardAttachment(AdaptiveCardBorders),
    'RoundedCorners': () => createAdaptiveCardAttachment(AdaptiveCardRoundedCorners),
    'FluentIcons': () => createAdaptiveCardAttachment(adaptiveCardFluentIcons),
    'MediaElements': () => createAdaptiveCardAttachment(adaptiveCardMediaElements),
    'StarRatings': () => createAdaptiveCardAttachment(adaptiveCardStarRatings),
    'ConditionalCard': () => createAdaptiveCardAttachment(adaptiveCardConditional),
    'ScrollableContainer': () => createAdaptiveCardAttachment(adaptiveCardScrollable),
    'CompoundButton': () => createAdaptiveCardAttachment(adaptiveCardCompoundButton),
    'ContainerLayout': () => createAdaptiveCardAttachment(adaptiveCardContainerLayouts),
    'DonutChart': () => createAdaptiveCardAttachment(adaptiveCardDonutChart),
    'GaugeChart': () => createAdaptiveCardAttachment(adaptiveCardGaugeChart),
    'HorizontalChart': () => createAdaptiveCardAttachment(adaptiveCardHorizontalBarChart),
    'HorizontalChartStacked': () => createAdaptiveCardAttachment(adaptiveCardHorizontalBarStackedChart),
    'LineChart': () => createAdaptiveCardAttachment(adaptiveCardLineChart),
    'PieChart': () => createAdaptiveCardAttachment(adaptiveCardPieChart),
    'VerticalBarChart': () => createAdaptiveCardAttachment(adaptiveCardVerticalBarChart),
    'VerticalBarGroupedChart': () => createAdaptiveCardAttachment(adaptiveCardVerticalBarGroupedChart)
  };

  // Handle card selection
  if (adaptiveFormatCards[text]) {
    try {
      const attachment = adaptiveFormatCards[text]();
      await context.send({
        type: 'message',
        attachments: [attachment]
      });
      await context.send(`You have Selected <b>${text}</b>`);
    } catch (error) {
      console.error(`Error sending ${text} card:`, error);
      await context.send(`Error sending ${text} card: ${error.message}`);
    }
  } 
  // Handle form submission
  else if (activity.value != null && activity.text == undefined) {
    const activityValue = activity.value;
    if (activityValue.hasOwnProperty('rating1') && activityValue.hasOwnProperty('rating2')) {
      await context.send(`Ratings Feedback: ${JSON.stringify(activityValue)}`);
    }
  }
  await sendAdaptiveCardFormats(context);
});

/**
 * Creates an adaptive card attachment
 * @param {Object} cardJson The adaptive card JSON
 * @returns {Object} The attachment object
 */
function createAdaptiveCardAttachment(cardJson) {
  return {
    contentType: 'application/vnd.microsoft.card.adaptive',
    content: cardJson
  };
}

/**
 * Creates an O365 connector card attachment
 * @param {Object} cardJson The connector card JSON
 * @returns {Object} The attachment object
 */
function createO365ConnectorCardAttachment(cardJson) {
  return {
    contentType: 'application/vnd.microsoft.teams.card.o365connector',
    content: cardJson
  };
}

/**
 * Send Adaptive Card Format options to the user.
 * @param {Object} context The turn context.
 */
async function sendAdaptiveCardFormats(context) {
  await context.send({
    type: 'message',
    attachments: [{
      contentType: 'application/vnd.microsoft.card.hero',
      content: {
        title: 'Please select a card from given options.',
        buttons: [
          { type: 'imBack', title: 'MentionSupport', value: 'MentionSupport' },
          { type: 'imBack', title: 'InfoMasking', value: 'InfoMasking' },
          { type: 'imBack', title: 'FullWidthCard', value: 'FullWidthCard' },
          { type: 'imBack', title: 'StageViewImages', value: 'StageViewImages' },
          { type: 'imBack', title: 'OverflowMenu', value: 'OverflowMenu' },
          { type: 'imBack', title: 'HTMLConnector', value: 'HTMLConnector' },
          { type: 'imBack', title: 'CardWithEmoji', value: 'CardWithEmoji' },
          { type: 'imBack', title: 'Persona', value: 'Persona' },
          { type: 'imBack', title: 'PersonaSet', value: 'PersonaSet' },
          { type: 'imBack', title: 'CodeBlock', value: 'CodeBlock' },
          { type: 'imBack', title: 'Layout', value: 'Layout' },
          { type: 'imBack', title: 'Borders', value: 'Borders' },
          { type: 'imBack', title: 'RoundedCorners', value: 'RoundedCorners' },
          { type: 'imBack', title: 'FluentIcons', value: 'FluentIcons' },
          { type: 'imBack', title: 'MediaElements', value: 'MediaElements' },
          { type: 'imBack', title: 'StarRatings', value: 'StarRatings' },
          { type: 'imBack', title: 'ConditionalCard', value: 'ConditionalCard' },
          { type: 'imBack', title: 'ScrollableContainer', value: 'ScrollableContainer' },
          { type: 'imBack', title: 'CompoundButton', value: 'CompoundButton' },
          { type: 'imBack', title: 'ContainerLayout', value: 'ContainerLayout' },
          { type: 'imBack', title: 'DonutChart', value: 'DonutChart' },
          { type: 'imBack', title: 'GaugeChart', value: 'GaugeChart' },
          { type: 'imBack', title: 'HorizontalChart', value: 'HorizontalChart' },
          { type: 'imBack', title: 'HorizontalChartStacked', value: 'HorizontalChartStacked' },
          { type: 'imBack', title: 'LineChart', value: 'LineChart' },
          { type: 'imBack', title: 'PieChart', value: 'PieChart' },
          { type: 'imBack', title: 'VerticalBarChart', value: 'VerticalBarChart' },
          { type: 'imBack', title: 'VerticalBarGroupedChart', value: 'VerticalBarGroupedChart' }
        ]
      }
    }]
  });
}

module.exports = app;

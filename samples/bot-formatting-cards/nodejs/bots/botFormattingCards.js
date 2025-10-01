// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityHandler, MessageFactory, CardFactory } = require('botbuilder');
const { ActionTypes } = require('botframework-schema');
const MentionSupport = require('../resources/mentionSupport.json');
const InformationMaskingCard = require('../resources/informationMasking.json');
const SampleAdaptiveCard = require('../resources/sampleAdaptiveWithFullWidth.json');
const StageViewImagesCard = require('../resources/stageViewForImages.json');
const OverFlowMenuCard = require('../resources/overflowMenu.json');
const HTMLConnectorCard = require('../resources/formatHTMLConnectorCard.json');
const CardWithEmoji = require('../resources/adaptiveCardWithEmoji.json');
const PeoplePersonaCardIcon = require('../resources/adaptivePeoplePersonaCardIcon.json');
const PeoplePersonaCardSetIcon = require('../resources/adaptivePeoplePersonaCardSetIcon.json');
const CodeBlocksCard = require('../resources/codeBlocksCard.json');
const AdaptiveCardResponsiveLayout = require('../resources/AdaptiveCardResponsiveLayout.json');
const AdaptiveCardBorders = require('../resources/adaptiveCardBorders.json');
const AdaptiveCardRoundedCorners = require('../resources/adaptiveCardRoundedCorners.json');
const adaptiveCardFluentIcons = require('../resources/adaptiveCardFluentIcon.json');
const adaptiveCardMediaElements = require('../resources/adaptiveCardMediaElements.json');
const adaptiveCardStarRatings = require('../resources/adaptiveCardStarRatings.json');
const adaptiveCardConditional = require('../resources/adaptiveCardConditional.json');
const adaptiveCardScrollable = require('../resources/adaptiveCardScrollable.json');
const adaptiveCardCompoundButton = require('../resources/adaptiveCardCompoundButton.json');
const adaptiveCardContainerLayouts = require('../resources/adaptiveCardContainerLayouts.json');
const adaptiveCardDonutChart = require('../resources/adaptiveCardDonutChart.json');
const adaptiveCardGaugeChart = require('../resources/adaptiveCardGaugeChart.json');
const adaptiveCardHorizontalBarChart = require('../resources/adaptiveCardHorizontalBarChart.json');
const adaptiveCardHorizontalBarStackedChart = require('../resources/adaptiveCardHorizontalBarStacked.json');
const adaptiveCardLineChart = require('../resources/adaptiveCardLineChart.json');
const adaptiveCardPieChart = require('../resources/adaptiveCardPieChart.json');
const adaptiveCardVerticalBarChart = require('../resources/adaptiveCardVerticalBarChart.json');
const adaptiveCardVerticalBarGroupedChart = require('../resources/adaptiveCardVerticalBarGroupedChart.json');

/**
 * BotFormattingCards class handles the bot's activities and responses.
 */
class BotFormattingCards extends ActivityHandler {
    constructor() {
        super();

        this.onMembersAdded(async (context, next) => {
            await this.sendWelcomeMessage(context);
            await next();
        });

        this.onMessage(async (context, next) => {
            const text = context.activity.text;

            const adaptiveFormatCards = {
                'CodeBlock': this.sendCodeBlock,
                'MentionSupport': this.sendMentionSupportCard,
                'InfoMasking': this.sendInfoMasking,
                'FullWidthCard': this.sendFullWidthCard,
                'StageViewImages': this.sendStageViewImagesCard,
                'OverflowMenu': this.sendOverFlowMenuCard,
                'HTMLConnector': this.sendHTMLConnectorCard,
                'CardWithEmoji': this.sendCardWithEmoji,
                'Persona': this.sendPersonaCardIcons,
                'PersonaSet': this.sendPersonaCardSetIcons,
                'Layout': this.sendLayoutCard,
                'Borders': this.sendBordersCard,
                'RoundedCorners': this.sendRoundedCornersCard,
                'FluentIcons': this.sendFluentIconsCard,
                'MediaElements': this.sendMediaElementsCard,
                'StarRatings': this.sendStarRatingsCard,
                'ConditionalCard': this.sendConditionalCard,
                'ScrollableContainer': this.sendScrollableCard,
                'CompoundButton': this.sendCompoundButtonCard,
                'ContainerLayout': this.sendContainerLayoutCard,
                'DonutChart': this.sendDonutChartCard,
                'GaugeChart': this.sendGaugeChartCard,
                'HorizontalChart': this.sendHorizontalBarChartCard,
                'HorizontalChartStacked': this.sendHorizontalBarStackedChartCard,
                'LineChart': this.sendLineChartCard,
                'PieChart': this.sendPieChartCard,
                'VerticalBarChart': this.sendVerticalBarChartCard,
                'VerticalBarGroupedChart': this.sendVerticalBarGroupedChartCard
            };

            if (adaptiveFormatCards[text]) {
                await context.sendActivity({ attachments: [adaptiveFormatCards[text].call(this)] });
                await context.sendActivity(`You have Selected <b>${text}</b>`);
            } else if (context.activity.value != null && context.activity.text == undefined) {
                const activityValue = context.activity.value;
                if (activityValue.hasOwnProperty('rating1') && activityValue.hasOwnProperty('rating2')) {
                    await context.sendActivity(`Ratings Feedback: ${JSON.stringify(activityValue)}`);
                }
            }

            await this.sendAdaptiveCardFormats(context);
            await next();
        });
    }

    /**
     * Send a welcome message along with Adaptive card format actions for the user to click.
     * @param {TurnContext} turnContext A TurnContext instance containing all the data needed for processing this conversation turn.
     */
    async sendWelcomeMessage(turnContext) {
        const { activity } = turnContext;
        for (const idx in activity.membersAdded) {
            if (activity.membersAdded[idx].id !== activity.recipient.id) {
                const welcomeMessage = `Welcome to Adaptive Card Format. This bot will introduce you to different types of formats. Please select the cards from given options`;
                await turnContext.sendActivity(welcomeMessage);
                await this.sendAdaptiveCardFormats(turnContext);
            }
        }
    }

    /**
     * Sends Mention Support Card
     */
    sendMentionSupportCard() {
        return CardFactory.adaptiveCard(MentionSupport);
    }

    sendCodeBlock() {
        return CardFactory.adaptiveCard(CodeBlocksCard);
    }

    /**
     * Adaptive Card updated to be responsive using targetWidth.
     */
    sendLayoutCard() {
        return CardFactory.adaptiveCard(AdaptiveCardResponsiveLayout);
    }

    /**
     * Sends Sample Adaptive Card With Full Width
     */
    sendFullWidthCard() {
        return CardFactory.adaptiveCard(SampleAdaptiveCard);
    }

    /**
     * Sends StageView Images Card
     */
    sendStageViewImagesCard() {
        return CardFactory.adaptiveCard(StageViewImagesCard);
    }

    /**
     * Sends InfoMasking Card
     */
    sendInfoMasking() {
        return CardFactory.adaptiveCard(InformationMaskingCard);
    }

    /**
     * Sends OverFlow Menu Card
     */
    sendOverFlowMenuCard() {
        return CardFactory.adaptiveCard(OverFlowMenuCard);
    }

    /**
     * Sends HTML Connector Card
     */
    sendHTMLConnectorCard() {
        return CardFactory.o365ConnectorCard(HTMLConnectorCard);
    }

    /**
     * Sends Card With Emoji
     */
    sendCardWithEmoji() {
        return CardFactory.adaptiveCard(CardWithEmoji);
    }

    /**
     * Persona card Icon in an Adaptive Card
     * If you want to show a single user in an Adaptive Card, the Adaptive Card displays the people icon and the name of the user.
     */
    sendPersonaCardIcons() {
        return CardFactory.adaptiveCard(PeoplePersonaCardIcon);
    }

    /**
     * Persona Card Set Icon in an Adaptive Card
     * If you want to show multiple users in an Adaptive Card, the Adaptive Card displays only the people icon of the users.
     */
    sendPersonaCardSetIcons() {
        return CardFactory.adaptiveCard(PeoplePersonaCardSetIcon);
    }

    /**
     * Sends Card showing the use of Borders on columns, columnsets, containers, etc.
     */
    sendBordersCard() {
        return CardFactory.adaptiveCard(AdaptiveCardBorders);
    }

    /**
     * Sends Card showing the use of Rounded Corners on columns, columnsets, containers, tables, etc.
     */
    sendRoundedCornersCard() {
        return CardFactory.adaptiveCard(AdaptiveCardRoundedCorners);
    }

    /**
     * Generates an Adaptive Card attachment that includes Fluent icons.
     * This method reads the adaptive card JSON from a resource file and deserializes it to be included as an attachment with Fluent icons in the card.
     * Fluent icons provide a modern and visually appealing way to enhance the UI within Adaptive Cards.
     */
    sendFluentIconsCard() {
        return CardFactory.adaptiveCard(adaptiveCardFluentIcons);
    }

    /**
     * Creates and returns an Attachment containing an adaptive card with media elements.
     */
    sendMediaElementsCard() {
        return CardFactory.adaptiveCard(adaptiveCardMediaElements);
    }

    /**
     * Sends a star ratings card as an attachment for displaying or collecting user feedback.
     */
    sendStarRatingsCard() {
        return CardFactory.adaptiveCard(adaptiveCardStarRatings);
    }

    /**
     * Sends a Conditional Action.submit button enable card
     */
    sendConditionalCard() {
        return CardFactory.adaptiveCard(adaptiveCardConditional);
    }

    /**
     * Sends a Scrollable container adaptive card
     */
    sendScrollableCard() {
        return CardFactory.adaptiveCard(adaptiveCardScrollable);
    }

    /**
     * Sends a Compound Button adaptive card
     */
    sendCompoundButtonCard() {
        return CardFactory.adaptiveCard(adaptiveCardCompoundButton);
    }

    /**
     * Sends a Container Layout adaptive card
     */
    sendContainerLayoutCard() {
        return CardFactory.adaptiveCard(adaptiveCardContainerLayouts);
    }

    /**
     * Sends a Donut Chart adaptive card
     */
    sendDonutChartCard() {
        return CardFactory.adaptiveCard(adaptiveCardDonutChart);
    }

    /**
     * Sends a Gauge Chart adaptive card
     */
    sendGaugeChartCard() {
        return CardFactory.adaptiveCard(adaptiveCardGaugeChart);
    }

    /**
     * Sends a Horizontal Bar Chart adaptive card
     */
    sendHorizontalBarChartCard() {
        return CardFactory.adaptiveCard(adaptiveCardHorizontalBarChart);
    }

    /**
     * Sends a Horizontal Bar Stacked Chart adaptive card
     */
    sendHorizontalBarStackedChartCard() {
        return CardFactory.adaptiveCard(adaptiveCardHorizontalBarStackedChart);
    }

    /**
     * Sends a Line Chart adaptive card
     */
    sendLineChartCard() {
        return CardFactory.adaptiveCard(adaptiveCardLineChart);
    }

    /**
     * Sends a Pie Chart adaptive card
     */
    sendPieChartCard() {
        return CardFactory.adaptiveCard(adaptiveCardPieChart);
    }

    /**
     * Sends a Vertical Bar Chart adaptive card
     */
    sendVerticalBarChartCard() {
        return CardFactory.adaptiveCard(adaptiveCardVerticalBarChart);
    }

    /**
     * Sends a Vertical Bar Grouped Chart adaptive card
     */
    sendVerticalBarGroupedChartCard() {
        return CardFactory.adaptiveCard(adaptiveCardVerticalBarGroupedChart);
    }

    /**
     * Send AdaptiveCard Formats to the user.
     * @param {TurnContext} turnContext A TurnContext instance containing all the data needed for processing this conversation turn.
     */
    async sendAdaptiveCardFormats(turnContext) {
        const cardActions = [
            { type: ActionTypes.ImBack, title: 'MentionSupport', value: 'MentionSupport' },
            { type: ActionTypes.ImBack, title: 'InfoMasking', value: 'InfoMasking' },
            { type: ActionTypes.ImBack, title: 'FullWidthCard', value: 'FullWidthCard' },
            { type: ActionTypes.ImBack, title: 'StageViewImages', value: 'StageViewImages' },
            { type: ActionTypes.ImBack, title: 'OverflowMenu', value: 'OverflowMenu' },
            { type: ActionTypes.ImBack, title: 'HTMLConnector', value: 'HTMLConnector' },
            { type: ActionTypes.ImBack, title: 'CardWithEmoji', value: 'CardWithEmoji' },
            { type: ActionTypes.ImBack, title: 'Persona', value: 'Persona' },
            { type: ActionTypes.ImBack, title: 'PersonaSet', value: 'PersonaSet' },
            { type: ActionTypes.ImBack, title: 'CodeBlock', value: 'CodeBlock' },
            { type: ActionTypes.ImBack, title: 'Layout', value: 'Layout' },
            { type: ActionTypes.ImBack, title: 'Borders', value: 'Borders' },
            { type: ActionTypes.ImBack, title: 'RoundedCorners', value: 'RoundedCorners' },
            { type: ActionTypes.ImBack, title: 'FluentIcons', value: 'FluentIcons' },
            { type: ActionTypes.ImBack, title: 'MediaElements', value: 'MediaElements' },
            { type: ActionTypes.ImBack, title: 'StarRatings', value: 'StarRatings' },
            { type: ActionTypes.ImBack, title: 'ConditionalCard', value: 'ConditionalCard' },
            { type: ActionTypes.ImBack, title: 'ScrollableContainer', value: 'ScrollableContainer' },
            { type: ActionTypes.ImBack, title: 'CompoundButton', value: 'CompoundButton' },
            { type: ActionTypes.ImBack, title: 'ContainerLayout', value: 'ContainerLayout' },
            { type: ActionTypes.ImBack, title: 'DonutChart', value: 'DonutChart' },
            { type: ActionTypes.ImBack, title: 'GaugeChart', value: 'GaugeChart' },
            { type: ActionTypes.ImBack, title: 'HorizontalChart', value: 'HorizontalChart' },
            { type: ActionTypes.ImBack, title: 'HorizontalChartStacked', value: 'HorizontalChartStacked' },
            { type: ActionTypes.ImBack, title: 'LineChart', value: 'LineChart' },
            { type: ActionTypes.ImBack, title: 'PieChart', value: 'PieChart' },
            { type: ActionTypes.ImBack, title: 'VerticalBarChart', value: 'VerticalBarChart' },
            { type: ActionTypes.ImBack, title: 'VerticalBarGroupedChart', value: 'VerticalBarGroupedChart' }
        ];

        await this.sendWelcomeCard(turnContext, cardActions);
    }

    /**
     * Sends a welcome card with given card actions.
     * @param {TurnContext} context A TurnContext instance containing all the data needed for processing this conversation turn.
     * @param {Array} cardActions Array of card actions to be included in the welcome card.
     */
    async sendWelcomeCard(context, cardActions) {
        const card = CardFactory.heroCard(
            'Please select a card from given options.',
            '',
            null,
            cardActions
        );
        await context.sendActivity(MessageFactory.attachment(card));
    }
}

module.exports.BotFormattingCards = BotFormattingCards;
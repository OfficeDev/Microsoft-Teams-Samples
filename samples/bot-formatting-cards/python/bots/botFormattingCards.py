# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core.teams import TeamsActivityHandler
from adaptive_cards import cards
from botbuilder.schema.teams import TeamInfo, TeamsChannelAccount
from botbuilder.schema import ActionTypes, Activity, ActivityTypes
from botbuilder.core import MessageFactory, CardFactory, TurnContext
from botbuilder.schema import ActionTypes, HeroCard, CardAction, Attachment

class BotFormattingCards(TeamsActivityHandler):
    def __init__(self):
        # Constructor for the TeamsBot class. Initializes the parent class (TeamsActivityHandler)
        super().__init__()

    async def on_teams_members_added(
        self,
        teams_members_added: list[TeamsChannelAccount],
        team_info: TeamInfo,
        turn_context: TurnContext,
    ):

        await self.send_welcome_message(turn_context)

    async def on_message_activity(self, turn_context: TurnContext):
        text = turn_context.activity.text

        # List of valid card options
        adaptive_format_cards = [
            "CodeBlock",
            "MentionSupport",
            "InfoMasking",
            "FullWidthCard",
            "StageViewImages",
            "OverflowMenu",
            "HTMLConnector",
            "CardWithEmoji",
            "Persona",
            "PersonaSet",
            "Layout",
            "Borders",
            "RoundedCorners",
            "FluentIcons",
            "MediaElements",
            "StarRatings",
            "ConditionalCard",
            "ScrollableContainer",
            "CompoundButton",
            "ContainerLayout",
            "DonutChart",
            "GaugeChart",
            "HorizontalChart",
            "HorizontalChartStacked",
            "LineChart",
            "PieChart",
            "VerticalBarChart",
            "VerticalBarGroupedChart",
        ]

        # Check if the received text is a valid card option
        if text in adaptive_format_cards:
            card_method_map = {
                "MentionSupport": cards.mentionSupport(),
                "InfoMasking": cards.informationMaskingCard(),
                "FullWidthCard": cards.sampleAdaptiveWithFullWidth(),
                "StageViewImages": cards.stageViewForImages(),
                "OverflowMenu": cards.overflowMenu(),
                "HTMLConnector": cards.formatHTMLConnectorCard(),
                "CardWithEmoji": cards.adaptiveCardWithEmoji(),
                "Persona": cards.adaptivePeoplePersonaCardIcon(),
                "PersonaSet": cards.adaptivePeoplePersonaCardSetIcon(),
                "CodeBlock": cards.codeBlocksCard(),
                "Layout": cards.AdaptiveCardResponsiveLayout(),
                "Borders": cards.adaptiveCardBorders(),
                "RoundedCorners": cards.adaptiveCardRoundedCorners(),
                "FluentIcons": cards.adaptiveCardFluentIcon(),
                "MediaElements": cards.adaptiveCardMediaElements(),
                "StarRatings": cards.adaptiveCardStarRatings(),
                "ConditionalCard": cards.adaptiveCardConditional(),
                "ScrollableContainer": cards.adaptiveCardScrollable(),
                "CompoundButton": cards.adaptiveCardCompoundButton(),
                "ContainerLayout": cards.adaptiveCardContainerLayouts(),
                "DonutChart": cards.adaptiveCardDonutChart(),
                "GaugeChart": cards.adaptiveCardGaugeChart(),
                "HorizontalChart": cards.adaptiveCardHorizontalBarChart(),
                "HorizontalChartStacked": cards.adaptiveCardHorizontalBarStacked(),
                "LineChart": cards.adaptiveCardLineChart(),
                "PieChart": cards.adaptiveCardPieChart(),
                "VerticalBarChart": cards.adaptiveCardVerticalBarChart(),
                "VerticalBarGroupedChart": cards.adaptiveCardVerticalBarGroupedChart(),
            }

        # Call the respective method dynamically
        if text in card_method_map:
            # Get the attachment directly (it is already an object, not a function)
            card_attachment = card_method_map[text]

            # Ensure the attachment is a list as expected
            if isinstance(card_attachment, list):
                attachments = card_attachment
            else:
                attachments = [card_attachment]

            # For only HTML Connector Card
            if text == "HTMLConnector":
                attachment = Attachment(
                    content_type="application/vnd.microsoft.teams.card.o365connector",
                    content=cards.formatHTMLConnectorCard(),
                )
                await turn_context.send_activity(
                    Activity(type=ActivityTypes.message, attachments=[attachment])
                )
            else:
                await turn_context.send_activity(
                    Activity(type=ActivityTypes.message, attachments=attachments)
                )

            await turn_context.send_activity(f"You have selected <b>{text}</b>")

        elif (
            turn_context.activity.value is not None
            and turn_context.activity.text is None
        ):
            activity_value = turn_context.activity.value

            # Check if the activity contains star rating feedback
            if "rating1" in activity_value and "rating2" in activity_value:
                await turn_context.send_activity(f"Ratings Feedback: {activity_value}")

        # Send adaptive card formats
        await self.send_adaptive_card_formats(turn_context)

    async def send_welcome_message(self, turn_context: TurnContext):
        activity = turn_context.activity

        # Iterate over all new members added to the conversation
        for member in activity.members_added:
            if member.id != activity.recipient.id:
                welcome_message = (
                    "Welcome to Adaptive Card Format. This bot will introduce you to different types of formats. "
                    "Please select a card from the options provided."
                )

            await turn_context.send_activity(welcome_message)

            # Send the adaptive card formats
            await self.send_adaptive_card_formats(turn_context)

    def send_mention_support_card(self):
        return cards.mentionSupport()

    def send_info_masking_card(self):
        return cards.informationMaskingCard()

    def send_fullwidthcard_card(self):
        return cards.sampleAdaptiveWithFullWidth()

    async def send_adaptive_card_formats(self, turn_context: TurnContext):
        card_actions = [
            CardAction(
                type=ActionTypes.im_back, title="MentionSupport", value="MentionSupport"
            ),
            CardAction(
                type=ActionTypes.im_back, title="InfoMasking", value="InfoMasking"
            ),
            CardAction(
                type=ActionTypes.im_back, title="FullWidthCard", value="FullWidthCard"
            ),
            CardAction(
                type=ActionTypes.im_back,
                title="StageViewImages",
                value="StageViewImages",
            ),
            CardAction(
                type=ActionTypes.im_back, title="OverflowMenu", value="OverflowMenu"
            ),
            CardAction(
                type=ActionTypes.im_back, title="HTMLConnector", value="HTMLConnector"
            ),
            CardAction(
                type=ActionTypes.im_back, title="CardWithEmoji", value="CardWithEmoji"
            ),
            CardAction(type=ActionTypes.im_back, title="Persona", value="Persona"),
            CardAction(
                type=ActionTypes.im_back, title="PersonaSet", value="PersonaSet"
            ),
            CardAction(type=ActionTypes.im_back, title="CodeBlock", value="CodeBlock"),
            CardAction(type=ActionTypes.im_back, title="Layout", value="Layout"),
            CardAction(type=ActionTypes.im_back, title="Borders", value="Borders"),
            CardAction(
                type=ActionTypes.im_back, title="RoundedCorners", value="RoundedCorners"
            ),
            CardAction(
                type=ActionTypes.im_back, title="FluentIcons", value="FluentIcons"
            ),
            CardAction(
                type=ActionTypes.im_back, title="MediaElements", value="MediaElements"
            ),
            CardAction(
                type=ActionTypes.im_back, title="StarRatings", value="StarRatings"
            ),
            CardAction(
                type=ActionTypes.im_back,
                title="ConditionalCard",
                value="ConditionalCard",
            ),
            CardAction(
                type=ActionTypes.im_back,
                title="ScrollableContainer",
                value="ScrollableContainer",
            ),
            CardAction(
                type=ActionTypes.im_back, title="CompoundButton", value="CompoundButton"
            ),
            CardAction(
                type=ActionTypes.im_back,
                title="ContainerLayout",
                value="ContainerLayout",
            ),
            CardAction(
                type=ActionTypes.im_back, title="DonutChart", value="DonutChart"
            ),
            CardAction(
                type=ActionTypes.im_back, title="GaugeChart", value="GaugeChart"
            ),
            CardAction(
                type=ActionTypes.im_back,
                title="HorizontalChart",
                value="HorizontalChart",
            ),
            CardAction(
                type=ActionTypes.im_back,
                title="HorizontalChartStacked",
                value="HorizontalChartStacked",
            ),
            CardAction(type=ActionTypes.im_back, title="LineChart", value="LineChart"),
            CardAction(type=ActionTypes.im_back, title="PieChart", value="PieChart"),
            CardAction(
                type=ActionTypes.im_back,
                title="VerticalBarChart",
                value="VerticalBarChart",
            ),
            CardAction(
                type=ActionTypes.im_back,
                title="VerticalBarGroupedChart",
                value="VerticalBarGroupedChart",
            ),
        ]

        await self.send_welcome_card(turn_context, card_actions)

    async def send_welcome_card(self, context: TurnContext, card_actions):
        card = HeroCard(
            title="Please select a card from given options.", buttons=card_actions
        )

        attachment = CardFactory.hero_card(card)
        await context.send_activity(MessageFactory.attachment(attachment))

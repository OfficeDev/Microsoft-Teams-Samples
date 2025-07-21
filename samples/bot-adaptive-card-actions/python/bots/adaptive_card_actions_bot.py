from botbuilder.core import ActivityHandler, MessageFactory, TurnContext, CardFactory
from botbuilder.schema import Activity, ActionTypes, CardAction, SuggestedActions

class AdaptiveCardActionsBot(ActivityHandler):
    async def on_teams_members_added(self, members_added, turn_context: TurnContext):
        for member in members_added:
            if member.id != turn_context.activity.recipient.id:
                await self.send_welcome_message(turn_context)

    async def on_message_activity(self, turn_context: TurnContext):
        text = turn_context.activity.text

        if text:
            if "Card Actions" in text:
                user_card = CardFactory.adaptive_card(self.adaptive_card_actions())
                await turn_context.send_activity(Activity(attachments=[user_card]))
            elif "Suggested Actions" in text:
                user_card = CardFactory.adaptive_card(self.suggested_actions_card())
                await turn_context.send_activity(Activity(attachments=[user_card]))
            elif any(color in text for color in ["Red", "Blue", "Yellow"]):
                valid_colors = ["Red", "Blue", "Yellow"]

                if text in valid_colors:
                    await turn_context.send_activity(f"I agree, {text} is the best color.")
                await self.send_suggested_actions(turn_context)
            elif "ToggleVisibility" in text:
                user_card = CardFactory.adaptive_card(self.toggle_visible_card())
                await turn_context.send_activity(Activity(attachments=[user_card]))
            else:
                await turn_context.send_activity(
                    "Please use one of these commands: **Card Actions** for Adaptive Card Actions, "
                    "**Suggested Actions** for Bot Suggested Actions, and **ToggleVisibility** for Action ToggleVisible Card."
                )

        await self.send_data_on_card_actions(turn_context)

    async def send_welcome_message(self, turn_context: TurnContext):
        welcome_message = (
            "Welcome to Adaptive Card Action and Suggested Action Bot. "
            "This bot will introduce you to suggested actions. Please select an option:"
        )
        await turn_context.send_activity(welcome_message)
        await turn_context.send_activity(
            "Please use one of these commands: **1** for Adaptive Card Actions, "
            "**2** for Bot Suggested Actions, and **3** for Toggle Visible Card."
        )
        await self.send_suggested_actions(turn_context)

    async def send_data_on_card_actions(self, turn_context: TurnContext):
        if turn_context.activity.value:
            reply_text = f"Data Submitted: {turn_context.activity.value.get('name', 'N/A')}"
            await turn_context.send_activity(MessageFactory.text(reply_text))

    async def send_suggested_actions(self, turn_context: TurnContext):
        card_actions = [
            CardAction(type=ActionTypes.im_back, title="Red", value="Red"),
            CardAction(type=ActionTypes.im_back, title="Yellow", value="Yellow"),
            CardAction(type=ActionTypes.im_back, title="Blue", value="Blue"),
        ]

        reply = MessageFactory.text("What is your favorite color?")
        reply.suggested_actions = SuggestedActions(actions=card_actions)
        await turn_context.send_activity(reply)

    def adaptive_card_actions(self):
        return {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.0",
            "body": [
                {"type": "TextBlock", "text": "Adaptive Card Actions"}
            ],
            "actions": [
                {
                    "type": "Action.OpenUrl",
                    "title": "Action Open URL",
                    "url": "https://adaptivecards.io"
                },
                {
                    "type": "Action.ShowCard",
                    "title": "Action Submit",
                    "card": {
                        "type": "AdaptiveCard",
                        "version": "1.5",
                        "body": [
                            {
                                "type": "Input.Text",
                                "id": "name",
                                "label": "Please enter your name:",
                                "isRequired": True,
                                "errorMessage": "Name is required"
                            }
                        ],
                        "actions": [
                            {"type": "Action.Submit", "title": "Submit"}
                        ]
                    }
                },
                {
                    "type": "Action.ShowCard",
                    "title": "Action ShowCard",
                    "card": {
                        "type": "AdaptiveCard",
                        "version": "1.0",
                        "body": [
                            {
                                "type": "TextBlock",
                                "text": "This card's action will show another card"
                            }
                        ],
                        "actions": [
                            {
                                "type": "Action.ShowCard",
                                "title": "Action.ShowCard",
                                "card": {
                                    "type": "AdaptiveCard",
                                    "body": [
                                        {"type": "TextBlock", "text": "**Welcome To New Card**"},
                                        {
                                            "type": "TextBlock",
                                            "text": "This is your new card inside another card"
                                        }
                                    ]
                                }
                            }
                        ]
                    }
                }
            ]
        }

    def toggle_visible_card(self):
        return {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.0",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "**Action.ToggleVisibility example**: click the button to show or hide a welcome message"
                },
                {
                    "type": "TextBlock",
                    "id": "helloWorld",
                    "isVisible": False,
                    "text": "**Hello World!**",
                    "size": "extraLarge"
                }
            ],
            "actions": [
                {
                    "type": "Action.ToggleVisibility",
                    "title": "Click me!",
                    "targetElements": ["helloWorld"]
                }
            ]
        }

    def suggested_actions_card(self):
        return {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.0",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "**Welcome to bot Suggested actions** please use below commands."
                },
                {
                    "type": "TextBlock",
                    "text": "please use below commands, to get response form the bot."
                },
                {
                    "type": "TextBlock",
                    "text": "- Red \r- Blue \r - Yellow",
                    "wrap": True
                }
            ]
        }

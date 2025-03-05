from botbuilder.core import TurnContext, MessageFactory,ActivityHandler,CardFactory
from botbuilder.schema import ActionTypes
from botbuilder.core.teams import TeamsInfo

class TeamsConversationBot(ActivityHandler):
    def __init__(self):
        super().__init__()

    async def on_message_activity(self, turn_context: TurnContext):
        # Get a paginated list of members in the current Teams conversation.
        members = await TeamsInfo.get_paged_members(turn_context)

        # Remove the bot's mention from the message text to clean up the user's input.
        TurnContext.remove_recipient_mention(turn_context.activity)

        # Normalize the message text for case-insensitive comparison.
        text = turn_context.activity.text.strip().lower()

        # If the message contains 'list', list the members. Otherwise, send a welcome card.
        if 'list' in text:
            await self.list_members_async(turn_context, members)
        else:
            await self.card_activity_async(turn_context)

    async def on_members_added_activity(self, members_added, turn_context: TurnContext):
        for member in members_added:
            if member.id != turn_context.activity.recipient.id:
                # Fetch detailed information about the new member.
                member_details = await TeamsInfo.get_member(turn_context, member.aad_object_id)

                # Create an Adaptive Card to welcome the new member and display their details.
                member_card = {
                    "type": "AdaptiveCard",
                    "body": [
                        {
                            "type": "TextBlock",
                            "text": f"Welcome to the team, {member_details.name}!",
                            "weight": "Bolder",
                            "size": "ExtraLarge"
                        },
                        {
                            "type": "TextBlock",
                            "text": "Here are your details:",
                            "weight": "Bolder",
                            "size": "Large",
                            "spacing": "Large"
                        },
                        {
                            "type": "FactSet",
                            "facts": [
                                {"title": "Name:", "value": member_details.name},
                                {"title": "Email:", "value": member_details.email},
                                {"title": "Given Name:", "value": member_details.given_name},
                                {"title": "Surname:", "value": member_details.surname},
                                {"title": "User Principal Name:", "value": member_details.user_principal_name},
                                {"title": "Tenant Id:", "value": member_details.tenant_id}
                            ],
                            "spacing": "ExtraLarge"
                        }
                    ],
                    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                    "version": "1.4"
                }

                # Send the Adaptive Card as an activity to the user.
                await turn_context.send_activity(
                    MessageFactory.attachment(CardFactory.adaptive_card(member_card))
                )

    async def list_members_async(self, turn_context: TurnContext, members):
        # Construct the Adaptive Card JSON structure with the list of members.
        adaptive_card_json = {
            "type": "AdaptiveCard",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "List of Members",
                    "weight": "Bolder",
                    "size": "Medium"
                },
                {
                    "type": "Container",
                    "items": [
                        {"type": "TextBlock", "text": f"- {item.name}", "wrap": True}
                        for item in members.members
                    ]
                }
            ],
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.4"
        }

        # Create an Activity object containing the Adaptive Card as an attachment.
        reply = MessageFactory.attachment(CardFactory.adaptive_card(adaptive_card_json))

        # Send the Activity with the list of members to the user.
        await turn_context.send_activity(reply)

    async def card_activity_async(self, turn_context: TurnContext):
        # Define the action button to list all members.
        card_actions = [
            {
                "type": ActionTypes.message_back,
                "title": "List all members",
                "value": None,
                "text": "List"
            }
        ]

        # Create a Hero Card with the defined action button.
        card = CardFactory.hero_card(
            title="Welcome card",
            buttons=card_actions
        )

        # Send the Hero Card as an activity to the user.
        await turn_context.send_activity(MessageFactory.attachment(card))
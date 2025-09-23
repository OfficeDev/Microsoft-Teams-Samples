from botbuilder.core import CardFactory
from botbuilder.schema import Attachment

def create_talk_to_expert_card(user_principal_name, user_name, chat_items, status) -> Attachment:
    card_body = []

    if chat_items:
        card_body.append(
            {
                "type": "TextBlock",
                "text": f"Conversation History with {user_name}",
                "weight": "Bolder",
                "spacing": "small"
            }
        )
        card_body.append(
            {
                "type": "Container",
                "id": "chat",
                "items": [
                    {
                        "type": "Container",
                        "items": chat_items,
                        "spacing": "small",
                        "padding": "None"
                    }
                ], 
                "spacing": "small",
                "padding": "None"
            }
        )
    else:
        card_body.append(
            {
                "type": "TextBlock",
                "text": f"{user_name} is requesting help.",  
                "weight": "Bolder",
                "spacing": "small"
            }
        )

    card_body.append(
        {
            "type": "TextBlock",
            "text": f"Status : {status}",
            "weight": "Bolder",
            "spacing": "small"
        }
    )

    chat_link = f"https://teams.microsoft.com/l/chat/0/0?users={user_principal_name}"  # Construct the chat link

    card_body.append(
        {
            "type": "ActionSet",
            "actions": [
                {
                    "type": "Action.OpenUrl",
                    "title": "Chat with User",
                    "url": chat_link,
                    "spacing": "small",
                    "padding": "None"
                },
                {
                    "type": "Action.Submit",
                    "title": "Assign Ticket to Me",
                    "data": {
                        "verb": "resolve_ticket",
                        "chat_items": chat_items,
                        "user_principal_name": user_principal_name,
                        "user_name": user_name
                    },
                    "spacing": "small",
                    "padding": "None"
                },
                {
                    "type": "Action.Submit",
                    "title": "Close Ticket",
                    "data": {
                        "verb": "close_ticket",
                        "chat_items": chat_items,
                        "user_name": user_name
                    },
                    "spacing": "small",
                    "padding": "None"
                }
            ],
            "spacing": "small",
            "padding": "None"
        }
    )

    return CardFactory.adaptive_card(
        {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.5",
            "type": "AdaptiveCard",
            "body": card_body,
            "padding": "None"
        }
    )
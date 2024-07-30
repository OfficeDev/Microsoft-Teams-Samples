from botbuilder.core import CardFactory
from botbuilder.schema import Attachment

def close_request_card(userName, chat_items) -> Attachment:
    card_body = []

    if chat_items:
        card_body.append(
            {
                "type": "TextBlock",
                "text": f"Conversation History with {userName}",
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
                "text": f"{userName} is requesting help.",
                "weight": "Bolder",
                "spacing": "small"
            }
        )

    card_body.append(
        {
            "type": "TextBlock",
            "text": f"Status : Resolved",
            "weight": "Bolder",
            "spacing": "small"
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
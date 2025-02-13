from botbuilder.core import CardFactory
from botbuilder.schema import Attachment


def adaptive_card(task_details) -> Attachment:
    return CardFactory.adaptive_card(
        {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.2",
            "body": [
                {
                    "type": "TextBlock",
                    "size": "Default",
                    "weight": "Bolder",
                    "text": f"{task_details['title']}",
                },
                {
                    "type": "TextBlock",
                    "size": "Default",
                    "text": f"Task title: {task_details['title']}",
                    "wrap": True,
                },
                {
                    "type": "TextBlock",
                    "size": "Default",
                    "text": f"Task description: {task_details['description']}",
                    "wrap": True,
                },
            ],
        }
    )

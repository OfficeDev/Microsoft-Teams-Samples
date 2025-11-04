import os

def adaptive_card_for_tab_stage_view() -> dict:
    base_url = os.getenv("BaseUrl", "")
    return {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "type": "AdaptiveCard",
        "version": "1.4",
        "body": [
            {
                "type": "TextBlock",
                "size": "Medium",
                "weight": "Bolder",
                "text": "Click the button to open the url in tab stage view"
            },
            {
                "type": "ActionSet",
                "actions": [
                    {
                        "type": "Action.Submit",
                        "title": "View via card",
                        "data": {
                            "msteams": {
                                "type": "invoke",
                                "value": {
                                    "type": "tab/tabInfoAction",
                                    "tabInfo": {
                                        "contentUrl": f"{base_url}/content",
                                        "websiteUrl": f"{base_url}/content",
                                        "name": "Stage view",
                                        "entityId": "stageViewTask"
                                    }
                                }
                            }
                        }
                    }
                ]
            }
        ]
    }

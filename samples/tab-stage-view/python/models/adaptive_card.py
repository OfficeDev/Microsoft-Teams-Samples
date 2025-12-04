import os

def adaptive_card_for_tab_stage_view(base_url_for_open_url: str) -> dict:
    base_url = os.getenv("BaseUrl", "")
    teams_app_id = os.getenv("TeamsAppId", "")

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
                    },
                    {
                        "type": "Action.OpenUrl",
                        "title": "View via Deep Link",
                        "url": f"https://teams.microsoft.com/l/stage/{teams_app_id}/0?context=%7B%22contentUrl%22%3A%22https%3A%2F%2F{base_url_for_open_url}%2Fcontent%22%2C%22websiteUrl%22%3A%22https%3A%2F%2F{base_url_for_open_url}%2Fcontent%22%2C%22name%22%3A%22DemoStageView%22%7D"
                    }
                ]
            }
        ]
    }

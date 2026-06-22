import os
import json
import requests
from flask import jsonify
from datetime import datetime

GRAPH_BASE_URL = "https://graph.microsoft.com/v1.0"
POLLY_APP_ID = os.getenv("POLLY_ID")
HEADERS = lambda token: {
    "Authorization": f"Bearer {token}",
    "Content-Type": "application/json"
}

def get_adaptive_card(req):
    token = req.json.get("token")
    try:
        users_resp = requests.get(f"{GRAPH_BASE_URL}/users", headers=HEADERS(token))
        users_resp.raise_for_status()
        users = users_resp.json().get("value", [])

        with open("resources/adaptiveCard.json", "r") as f:
            template_payload = json.load(f)

        card_data = {}
        for i in range(min(6, len(users))):
            card_data[f"user{i+1}Title"] = users[i]["displayName"]
            card_data[f"user{i+1}Id"] = users[i]["id"]

        template_str = json.dumps(template_payload)
        for key, value in card_data.items():
            template_str = template_str.replace(f"${{{key}}}", str(value))
        card = json.loads(template_str)

        return jsonify({"type": "AdaptiveCard", "content": card})
    except Exception as e:
        return jsonify({"error": str(e)}), 500

def create_group_chat(req):
    token = req.json.get("token")
    user_ids = req.json.get("users", "").split(",")
    user_id_main = req.json.get("userId")
    title = req.json.get("title")

    try:
        client_headers = HEADERS(token)

        chat_payload = {
            "chatType": "group",
            "topic": title,
            "members": [
                {
                    "@odata.type": "#microsoft.graph.aadUserConversationMember",
                    "roles": ["owner"],
                    "user@odata.bind": f"{GRAPH_BASE_URL}/users('{user_ids[0]}')"
                },
                {
                    "@odata.type": "#microsoft.graph.aadUserConversationMember",
                    "roles": ["owner"],
                    "user@odata.bind": f"{GRAPH_BASE_URL}/users('{user_id_main}')"
                }
            ]
        }

        chat_resp = requests.post(f"{GRAPH_BASE_URL}/chats", headers=client_headers, json=chat_payload)
        chat_resp.raise_for_status()
        chat = chat_resp.json()
        chat_id = chat["id"]

        if len(user_ids) == 2:
            add_members_with_history(chat_id, user_ids[1], token)
            delete_first_member(chat_id, token)

        elif len(user_ids) == 3:
            add_members_with_history(chat_id, user_ids[1], token)
            add_members_without_history(chat_id, user_ids[2], token)
            delete_first_member(chat_id, token)

        elif len(user_ids) >= 4:
            add_members_with_history(chat_id, user_ids[1], token)
            add_members_without_history(chat_id, user_ids[2], token)
            add_members_with_days(chat_id, user_ids[3:], token)
            delete_first_member(chat_id, token)

        # Install Polly app
        install_payload = {
            "teamsApp@odata.bind": f"{GRAPH_BASE_URL}/appCatalogs/teamsApps/{POLLY_APP_ID}"
        }
        requests.post(f"{GRAPH_BASE_URL}/chats/{chat_id}/installedApps", headers=client_headers, json=install_payload)

        tab_payload = {
            "displayName": "Polly",
            "teamsApp@odata.bind": f"{GRAPH_BASE_URL}/appCatalogs/teamsApps/{POLLY_APP_ID}",
            "configuration": {
                "entityId": "pollyapp",
                "contentUrl": "https://teams.polly.ai/msteams/content/meeting/tab?theme={theme}",
                "removeUrl": "https://teams.polly.ai/msteams/content/tabdelete?theme={theme}"
            }
        }
        requests.post(f"{GRAPH_BASE_URL}/chats/{chat_id}/tabs", headers=client_headers, json=tab_payload)

        return jsonify(True)
    except Exception as e:
        return jsonify({"error": str(e)}), 500


def add_members_with_history(chat_id, user_id, token):
    payload = {
        "@odata.type": "#microsoft.graph.aadUserConversationMember",
        "roles": ["owner"],
        "user@odata.bind": f"{GRAPH_BASE_URL}/users/{user_id}",
        "visibleHistoryStartDateTime": "0001-01-01T00:00:00Z"
    }
    requests.post(f"{GRAPH_BASE_URL}/chats/{chat_id}/members", headers=HEADERS(token), json=payload)

def add_members_without_history(chat_id, user_id, token):
    payload = {
        "@odata.type": "#microsoft.graph.aadUserConversationMember",
        "roles": ["owner"],
        "user@odata.bind": f"{GRAPH_BASE_URL}/users/{user_id}"
    }
    requests.post(f"{GRAPH_BASE_URL}/chats/{chat_id}/members", headers=HEADERS(token), json=payload)

def add_members_with_days(chat_id, user_ids, token):
    for uid in user_ids:
        payload = {
            "@odata.type": "#microsoft.graph.aadUserConversationMember",
            "roles": ["owner"],
            "user@odata.bind": f"{GRAPH_BASE_URL}/users/{uid}",
            "visibleHistoryStartDateTime": datetime.utcnow().isoformat() + "Z"
        }
        requests.post(f"{GRAPH_BASE_URL}/chats/{chat_id}/members", headers=HEADERS(token), json=payload)

def delete_first_member(chat_id, token):
    members_resp = requests.get(f"{GRAPH_BASE_URL}/chats/{chat_id}?$expand=members", headers=HEADERS(token))
    members = members_resp.json().get("members", [])
    if members:
        member_id = members[0]["id"]
        requests.delete(f"{GRAPH_BASE_URL}/chats/{chat_id}/members/{member_id}", headers=HEADERS(token))

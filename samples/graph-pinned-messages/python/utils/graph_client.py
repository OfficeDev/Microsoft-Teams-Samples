#!/usr/bin/env python3
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import requests

GRAPH_API_BASE_URL = "https://graph.microsoft.com/beta"

class GraphClient:
    def __init__(self, access_token: str):
        if not access_token:
            raise ValueError("Access token is required for GraphClient.")
            
        self.headers = {
            "Authorization": f"Bearer {access_token}",
            "Content-Type": "application/json"
        }

    # Retrieves all pinned messages from a Teams chat.
    def get_pinned_messages(self, chat_id: str):
        url = f"{GRAPH_API_BASE_URL}/chats/{chat_id}/pinnedMessages?$expand=message"
        response = requests.get(url, headers=self.headers)
        if response.status_code == 404:
            return {"value": []}
        elif response.status_code != 200:
            response.raise_for_status()
        return response.json()

    # Retrieves the most recent messages from a Teams chat.
    def get_recent_messages(self, chat_id: str, top: int = 20):
        url = f"{GRAPH_API_BASE_URL}/chats/{chat_id}/messages?$orderby=createdDateTime desc&$top={top}"
        response = requests.get(url, headers=self.headers)
        response.raise_for_status()
        return response.json()

    # Pins a specific message in a Teams chat.
    def pin_message(self, chat_id: str, message_id: str):
        url = f"{GRAPH_API_BASE_URL}/chats/{chat_id}/pinnedMessages"
        payload = {
            "message@odata.bind": f"{GRAPH_API_BASE_URL}/chats/{chat_id}/messages/{message_id}"
        }
        response = requests.post(url, json=payload, headers=self.headers)
        response.raise_for_status()
        return response.status_code

    # Unpins a message from a Teams chat.
    def unpin_message(self, chat_id: str, pinned_message_id: str):
        url = f"{GRAPH_API_BASE_URL}/chats/{chat_id}/pinnedMessages/{pinned_message_id}"
        response = requests.delete(url, headers=self.headers)
        if response.status_code not in [204, 202]:
            response.raise_for_status()
        return response.status_code

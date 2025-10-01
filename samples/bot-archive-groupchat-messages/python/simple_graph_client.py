# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import aiohttp
import json
from typing import Dict, Any


class SimpleGraphClient:
    """
    This class is a wrapper for the Microsoft Graph API.
    See: https://developer.microsoft.com/en-us/graph for more information.
    """

    def __init__(self, token: str):
        if not token or not token.strip():
            raise ValueError("SimpleGraphClient: Invalid token received.")
        
        self._token = token
        self._base_url = "https://graph.microsoft.com"

    async def get_messages(self, chat_id: str) -> Dict[str, Any]:
        """
        Gets messages from a specific chat using Microsoft Graph API.
        
        Args:
            chat_id (str): The ID of the chat to fetch messages from
            
        Returns:
            Dict[str, Any]: The response containing chat messages
        """
        url = f"{self._base_url}/beta/chats/{chat_id}/messages"
        
        headers = {
            "Authorization": f"Bearer {self._token}",
            "Content-Type": "application/json"
        }

        async with aiohttp.ClientSession() as session:
            async with session.get(url, headers=headers) as response:
                if response.status == 200:
                    return await response.json()
                else:
                    response_text = await response.text()
                    raise Exception(f"Error fetching messages: {response.status} - {response_text}")

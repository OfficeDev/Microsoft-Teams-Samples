# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import aiohttp
import asyncio
import base64
import logging
from typing import Optional, Dict, Any


class SimpleGraphClient:
    """
    This class is a wrapper for the Microsoft Graph API.
    See: https://developer.microsoft.com/en-us/graph for more information.
    """

    def __init__(self, token: str):
        """
        Creates an instance of SimpleGraphClient.
        
        Args:
            token (str): The token issued to the user.
        """
        if not token or not token.strip():
            raise ValueError('SimpleGraphClient: Invalid token received.')

        self._token = token
        self._base_url = 'https://graph.microsoft.com/v1.0'
        self._headers = {
            'Authorization': f'Bearer {self._token}',
            'Content-Type': 'application/json'
        }

    async def get_Tag(self, teamId):
        try:
            async with aiohttp.ClientSession() as session:
                async with session.get(
                    f'{self._base_url}/teams/{teamId}/tags',
                    headers=self._headers
                ) as response:
                    if response.status == 200:
                        return await response.json()
                    else:
                        logging.error(f'Error getting user information: {response.status} - {await response.text()}')
                        return None
        except Exception as error:
            logging.error(f'Error getting user information: {error}')
            raise error
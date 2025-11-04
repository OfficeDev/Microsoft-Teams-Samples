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

    async def get_me(self) -> Optional[Dict[str, Any]]:
        """
        Collects information about the user in the bot.
        
        Returns:
            Optional[Dict[str, Any]]: The user information.
        """
        try:
            async with aiohttp.ClientSession() as session:
                async with session.get(
                    f'{self._base_url}/me',
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

    async def get_photo_async(self, token: str) -> Optional[str]:
        """
        Gets the user's photo.
        
        Args:
            token (str): The token issued to the user.
            
        Returns:
            Optional[str]: The user's photo as a base64 encoded string.
        """
        # First try to get photo metadata to check if photo exists
        photo_metadata_endpoint = f'{self._base_url}/me/photo'
        photo_value_endpoint = f'{self._base_url}/me/photo/$value'
        
        headers = {
            'Authorization': f'Bearer {token}'
        }

        try:
            async with aiohttp.ClientSession() as session:
                # Check if photo exists first
                async with session.get(photo_metadata_endpoint, headers=headers) as metadata_response:
                    if metadata_response.status != 200:
                        logging.info(f'User photo metadata not available: {metadata_response.status}')
                        return None
                
                # If metadata exists, try to get the actual photo
                async with session.get(photo_value_endpoint, headers=headers) as response:
                    if response.status == 200:
                        image_data = await response.read()
                        image_base64 = base64.b64encode(image_data).decode('utf-8')
                        image_uri = f'data:image/png;base64,{image_base64}'
                        return image_uri
                    elif response.status == 404:
                        logging.info('User photo not found (404). This is normal for users without profile photos.')
                        return None
                    else:
                        error_text = await response.text()
                        logging.error(f'Error fetching photo: {response.status} - {error_text}')
                        return None
        except Exception as error:
            logging.error(f'Error fetching photo: {error}')
            return None
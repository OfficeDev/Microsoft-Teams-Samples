# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

"""
MessageSender - Sends proactive messages using Teams SDK with retry logic.

This module demonstrates how to:
1. Send proactive messages using the Teams SDK
2. Handle rate limiting and throttling (HTTP 429)
3. Implement exponential backoff retry policies
"""

import asyncio
import os
import random
import logging
from typing import Optional, Dict, Any
from microsoft_teams.apps import App
from microsoft_teams.api import MessageActivityInput


class MessageSender:
    """Handles sending proactive messages with retry logic using Teams SDK."""
    
    def __init__(self, app_id: str, app_password: str, tenant_id: str):
        """
        Initialize the message sender.
        
        Args:
            app_id: Bot Application (Client) ID
            app_password: Bot Application Secret
            tenant_id: Microsoft Entra ID Tenant ID
        """
        self.app_id = app_id
        self.app_password = app_password
        self.tenant_id = tenant_id
        
        # Choose a random port to avoid conflicts with running bots
        self.port = random.randint(4000, 5000)
        
        # Set the port via environment variable before creating the app
        os.environ['PORT'] = str(self.port)
        
        # Create Teams SDK app for proactive messaging (without starting HTTP server)
        self.app = App(
            storage={},
            client_id=app_id,
            client_secret=app_password
        )
        # Track the server task
        self._server_task = None
        self._initialized = False
    
    
    async def _ensure_initialized(self):
        """Ensure the app is initialized for proactive messaging."""
        if not self._initialized:
            self._server_task = asyncio.create_task(self.app.start())
            await asyncio.sleep(2)
            self._initialized = True
    
    async def send_to_conversation(
        self,
        service_url: str,
        conversation_id: str,
        message: str,
        max_retries: int = 3,
        retry_delay: float = 1.0
    ) -> Optional[Dict[str, Any]]:
        """
        Send a proactive message to a conversation with retry logic.
        
        Args:
            service_url: The service URL from the coordinate logger
            conversation_id: The conversation ID from the coordinate logger
            message: The message text to send
            max_retries: Maximum number of retry attempts
            retry_delay: Initial delay between retries (exponential backoff)
            
        Returns:
            Response information or None on failure
        """
        await self._ensure_initialized()
        
        # Send with retry logic
        attempt = 0
        current_delay = retry_delay
        
        while attempt <= max_retries:
            try:
                attempt += 1
                
                if attempt > 1:
                    await asyncio.sleep(current_delay)
                
                activity = MessageActivityInput(text=message)
                result = await self.app.send(
                    conversation_id=conversation_id,
                    activity=activity
                )
                
                return {'id': str(result) if result else 'sent', 'status': 'success'}
                
            except Exception as error:
                error_str = str(error).lower()
                
                if '429' in error_str or 'throttl' in error_str or 'too many requests' in error_str:
                    retry_after = self._extract_retry_after(str(error))
                    if retry_after:
                        current_delay = retry_after
                    else:
                        current_delay *= 2
                    
                    if attempt <= max_retries:
                        continue
                
                elif '401' in error_str or 'unauthorized' in error_str:
                    print(f"Authentication failed: {error}")
                    raise
                
                elif '404' in error_str or 'not found' in error_str:
                    print(f"Conversation not found: {error}")
                    raise
                
                else:
                    if attempt <= max_retries:
                        current_delay *= 1.5
                        continue
                    else:
                        raise
        
        return None
    
    async def cleanup(self):
        """Clean up resources and stop the app."""
        if self._initialized:
            try:
                logging.getLogger("uvicorn.error").setLevel(logging.CRITICAL)
                logging.getLogger("starlette").setLevel(logging.CRITICAL)
                
                try:
                    await asyncio.wait_for(self.app.stop(), timeout=3.0)
                except asyncio.TimeoutError:
                    pass
                except Exception:
                    pass
                
                await asyncio.sleep(0.5)
                
                if self._server_task and not self._server_task.done():
                    self._server_task.cancel()
                    try:
                        await self._server_task
                    except (asyncio.CancelledError, Exception):
                        pass
            except Exception:
                pass
    
    def _extract_retry_after(self, error_message: str) -> Optional[float]:
        """
        Extract Retry-After value from error message.
        
        Args:
            error_message: The error message string
            
        Returns:
            Retry-after delay in seconds, or None
        """
        # Try to parse retry-after from error message
        import re
        match = re.search(r'retry[_-]?after[:\s]+(\d+)', error_message, re.IGNORECASE)
        if match:
            return float(match.group(1))
        return None

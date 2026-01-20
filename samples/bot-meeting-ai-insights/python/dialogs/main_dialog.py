# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import MessageFactory
from botbuilder.dialogs import (
    WaterfallDialog,
    WaterfallStepContext,
    DialogTurnResult,
    PromptOptions,
)
from botbuilder.dialogs.prompts import OAuthPrompt, OAuthPromptSettings, ConfirmPrompt

from dialogs import LogoutDialog
import logging
import aiohttp
import os
import json
from urllib.parse import quote

# Set the logging level to INFO
logging.basicConfig(level=logging.INFO)

class MainDialog(LogoutDialog):
    def __init__(self, connection_name: str):
        # Initializes the MainDialog with OAuthPrompt and WaterfallDialog.
        super(MainDialog, self).__init__(MainDialog.__name__, connection_name)

        self.add_dialog(
            OAuthPrompt(
                OAuthPrompt.__name__,
                OAuthPromptSettings(
                    connection_name=connection_name,
                    text="Please Sign In",
                    title="Sign In",
                    timeout=300000
                )
            )
        )

        self.add_dialog(ConfirmPrompt(ConfirmPrompt.__name__))

        self.add_dialog(
            WaterfallDialog(
                "WFDialog",
                [
                    self.prompt_step,
                    self.login_step,
                ],
            )
        )

        self.initial_dialog_id = "WFDialog"

    # Prompts the user to sign in using the OAuthPrompt.
    async def prompt_step(self, step_context: WaterfallStepContext) -> DialogTurnResult:
        logging.info("Reached OAuthPrompt step — prompting for login.")
        return await step_context.begin_dialog(OAuthPrompt.__name__)

    # Handles the login result and retrieves AI insights.
    async def login_step(self, step_context: WaterfallStepContext) -> DialogTurnResult:
        token_response = step_context.result
        if token_response and token_response.token:
            sso_token = token_response.token
            
            if sso_token:
                user_id = os.environ.get('UserId')
                meeting_url = os.environ.get('MeetingJoinUrl')
                
                online_meeting_id = await self.get_meeting_id(sso_token, meeting_url)
                ai_insight_id = await self.get_ai_insight_id(sso_token, user_id, online_meeting_id)
                
                if ai_insight_id:
                    ai_insight = await self.get_ai_insight_details(sso_token, user_id, online_meeting_id, ai_insight_id)
                    if ai_insight:
                        formatted_message = ''
                        if isinstance(ai_insight, list):
                            formatted_message = '\n'.join([
                                f"## {insight.get('title', '')}\n{insight.get('text', '')}\n"
                                for insight in ai_insight
                            ])
                        
                        await step_context.context.send_activity(
                            formatted_message or 'No insights found in the expected format.'
                        )
                    else:
                        await step_context.context.send_activity('Failed to retrieve AI Insight details.')
                else:
                    await step_context.context.send_activity('Failed to retrieve AI Insight ID.')
            else:
                await step_context.context.send_activity('Failed to retrieve access token for downstream API.')
            
            return await step_context.end_dialog()
        
        await step_context.context.send_activity('Login was not successful, please try again.')
        return await step_context.end_dialog()

    async def get_meeting_id(self, access_token: str, meeting_url: str) -> str:
        """
        Calls the Graph API to get the meeting ID from the meeting URL.
        
        Args:
            access_token: The access token for the Graph API.
            meeting_url: The meeting join URL.
            
        Returns:
            The online meeting ID or None if failed.
        """
        try:
            url = f"https://graph.microsoft.com/beta/me/onlineMeetings?$filter=JoinWebUrl eq '{meeting_url}'"
            
            headers = {
                'Authorization': f'Bearer {access_token}'
            }
            
            async with aiohttp.ClientSession() as session:
                async with session.get(url, headers=headers) as response:
                    if response.status == 200:
                        data = await response.json()
                        if data.get('value') and len(data['value']) > 0:
                            return data['value'][0]['id']
                    else:
                        logging.error(f"Error retrieving meeting ID: {response.status}")
                        
        except Exception as error:
            logging.error(f'Error retrieving meeting ID: {error}')
            
        return None

    async def get_ai_insight_id(self, access_token: str, user_id: str, online_meeting_id: str) -> str:
        """
        Calls the Graph API to get the AI Insight ID.
        
        Args:
            access_token: The access token for the Graph API.
            user_id: The user ID.
            online_meeting_id: The online meeting ID.
            
        Returns:
            The AI Insight ID or None if failed.
        """
        try:
            url = f"https://graph.microsoft.com/beta/copilot/users/{user_id}/onlineMeetings/{online_meeting_id}/aiInsights"
            
            headers = {
                'Authorization': f'Bearer {access_token}'
            }
            
            async with aiohttp.ClientSession() as session:
                async with session.get(url, headers=headers) as response:
                    if response.status == 200:
                        data = await response.json()
                        if data.get('value') and len(data['value']) > 0:
                            return data['value'][0]['id']
                    else:
                        logging.error(f"Error retrieving AI Insight ID: {response.status}")
                        
        except Exception as error:
            logging.error(f'Error retrieving AI Insight ID: {error}')
            
        return None

    async def get_ai_insight_details(self, access_token: str, user_id: str, online_meeting_id: str, ai_insight_id: str):
        """
        Calls the Graph API to get the AI Insight details.
        
        Args:
            access_token: The access token for the Graph API.
            user_id: The user ID.
            online_meeting_id: The online meeting ID.
            ai_insight_id: The AI Insight ID.
            
        Returns:
            The meeting notes from AI insights or None if failed.
        """
        try:
            url = f"https://graph.microsoft.com/beta/copilot/users/{user_id}/onlineMeetings/{online_meeting_id}/aiInsights/{ai_insight_id}"
            
            headers = {
                'Authorization': f'Bearer {access_token}'
            }
            
            async with aiohttp.ClientSession() as session:
                async with session.get(url, headers=headers) as response:
                    if response.status == 200:
                        data = await response.json()
                        return data.get('meetingNotes')
                    else:
                        logging.error(f"Error retrieving AI Insight details: {response.status}")
                        
        except Exception as error:
            logging.error(f'Error retrieving AI Insight details: {error}')
            
        return None

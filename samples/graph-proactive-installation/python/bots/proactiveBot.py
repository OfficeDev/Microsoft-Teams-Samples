# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import TurnContext, MessageFactory
from botbuilder.schema import ConversationReference
from botbuilder.core.teams import TeamsInfo, TeamsActivityHandler
from typing import Dict
import asyncio 
from Models.Proactive_App_Installation_Helper import ProactiveAppInstallationHelper 

class ProactiveBot(TeamsActivityHandler):
    def __init__(self, conversation_references: Dict[str, ConversationReference]):
        """Proactive_App_Installation_Helper
        Initializes the ProactiveBot class.
        
        :param conversation_references: Dictionary to store conversation references for users.
        """
        super().__init__()
        self.conversation_references = conversation_references

    async def on_conversation_update_activity(self, turn_context: TurnContext):
        """
        Handles conversation update activities (e.g., when users join the chat).
        Adds the conversation reference to track users.
        
        :param turn_context: Context object containing information about the activity.
        """
        self.add_conversation_reference(turn_context.activity)

    async def on_teams_members_added(self, members_added, turn_context: TurnContext):
        """
        Handles the event when new members are added to the team.
        Adds conversation references for each new member.
        
        :param members_added: List of members added to the team.
        :param turn_context: Context object containing information about the activity.
        """
        for member in members_added:
            if member.id != turn_context.activity.recipient.id:
                self.add_conversation_reference(turn_context.activity)

    async def on_message_activity(self, turn_context: TurnContext):
        """
        Handles incoming messages from users and triggers appropriate actions.
        
        :param turn_context: Context object containing the user's message.
        """
        TurnContext.remove_recipient_mention(turn_context.activity)  # Removes bot mentions from messages
        text = turn_context.activity.text.strip().lower()
        
        if 'install' in text:
            await self.install_app_in_teams_and_chat_members_personal_scope(turn_context)
        elif 'send' in text:
            await self.send_notification_to_all_users_async(turn_context)

    async def install_app_in_teams_and_chat_members_personal_scope(self, turn_context: TurnContext):
        """
        Installs the app for all team members in their personal scope if not already installed.
        
        :param turn_context: Context object containing information about the activity.
        """
        new_app_install_count = 0
        existing_app_install_count = 0
        
        proactive_helper = ProactiveAppInstallationHelper()
        team_members = await TeamsInfo.get_paged_members(turn_context)  # Fetch all team members
        
        # Create installation tasks for users who haven't been tracked yet
        tasks = [
            proactive_helper.install_app_in_personal_scope(turn_context.activity.conversation.tenant_id, member.aad_object_id)
            for member in team_members.members if member.aad_object_id not in self.conversation_references
        ]
        
        results = await asyncio.gather(*tasks)  # Execute all installation tasks concurrently
        
        for status_code in results:
            if status_code == 409:
                existing_app_install_count += 1  # App is already installed
            elif status_code == 201:
                new_app_install_count += 1  # App was newly installed
        
        # Send a summary message to the user
        await turn_context.send_activity(
            MessageFactory.text(f"Existing: {existing_app_install_count}\n\nNewly Installed: {new_app_install_count}")
        )

    async def send_notification_to_all_users_async(self, turn_context: TurnContext):
        """
        Sends a proactive message to all team members.
        
        :param turn_context: Context object containing information about the activity.
        """
        team_members = await TeamsInfo.get_paged_members(turn_context)  # Fetch all team members
        sent_msg_count = len(team_members.members)  # Count of messages sent
        
        for member in team_members.members:
            ref = TurnContext.get_conversation_reference(turn_context.activity)
            ref.user = member  # Set the recipient to the current team member
            
            async def send_proactive_message(context: TurnContext):
                await context.send_activity("Proactive hello.")
            
            await turn_context.adapter.create_conversation(ref, send_proactive_message)
        
        # Send a confirmation message to the user who triggered the command
        await turn_context.send_activity(MessageFactory.text(f"Message sent: {sent_msg_count}"))

    def add_conversation_reference(self, activity):
        """
        Stores the conversation reference for a user so the bot can send proactive messages later.
        
        :param activity: The activity object containing conversation details.
        """
        conversation_reference = TurnContext.get_conversation_reference(activity)
        self.conversation_references[conversation_reference.user.aad_object_id] = conversation_reference


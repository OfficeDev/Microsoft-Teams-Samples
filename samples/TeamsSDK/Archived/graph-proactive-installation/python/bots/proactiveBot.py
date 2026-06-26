# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio

from botbuilder.core import TurnContext, MessageFactory
from botbuilder.core.teams import TeamsInfo, TeamsActivityHandler
from botbuilder.schema import ConversationParameters

from config import DefaultConfig
from Models.Proactive_App_Installation_Helper import ProactiveAppInstallationHelper


class ProactiveBot(TeamsActivityHandler):
    def __init__(self, config: DefaultConfig):
        super().__init__()
        self.config = config
        self.conversation_references = {}

    async def on_conversation_update_activity(self, turn_context: TurnContext):
        self._add_conversation_reference(turn_context.activity)

    async def on_teams_members_added(self, members_added, turn_context: TurnContext):
        for member in members_added:
            if member.id != turn_context.activity.recipient.id:
                self._add_conversation_reference(turn_context.activity)

    async def on_message_activity(self, turn_context: TurnContext):
        TurnContext.remove_recipient_mention(turn_context.activity)
        text = turn_context.activity.text.strip().lower()

        if "install" in text:
            await self._install_app_in_personal_scope(turn_context)
        elif "send" in text:
            await self._send_notification_to_all_users(turn_context)

    async def _install_app_in_personal_scope(self, turn_context: TurnContext):
        new_app_install_count = 0
        existing_app_install_count = 0

        helper = ProactiveAppInstallationHelper(self.config)
        team_members = await TeamsInfo.get_paged_members(turn_context)

        tasks = [
            helper.install_app_in_personal_scope(
                turn_context.activity.conversation.tenant_id, member.aad_object_id
            )
            for member in team_members.members
            if member.aad_object_id not in self.conversation_references
        ]

        results = await asyncio.gather(*tasks)

        for status_code in results:
            if status_code == 409:
                existing_app_install_count += 1
            elif status_code == 201:
                new_app_install_count += 1

        await turn_context.send_activity(
            MessageFactory.text(
                f"Existing: {existing_app_install_count}\n\nNewly Installed: {new_app_install_count}"
            )
        )

    async def _send_notification_to_all_users(self, turn_context: TurnContext):
        team_members = await TeamsInfo.get_paged_members(turn_context)
        sent_msg_count = len(team_members.members)

        service_url = turn_context.activity.service_url
        channel_id = turn_context.activity.channel_id
        bot_app_id = self.config.APP_ID
        tenant_id = turn_context.activity.conversation.tenant_id

        for member in team_members.members:
            conversation_parameters = ConversationParameters(
                is_group=False,
                bot=turn_context.activity.recipient,
                members=[member],
                tenant_id=tenant_id,
            )

            async def _callback(context: TurnContext):
                await context.send_activity("Proactive hello.")

            await turn_context.adapter.create_conversation(
                bot_app_id,
                _callback,
                conversation_parameters,
                channel_id,
                service_url,
            )

        await turn_context.send_activity(
            MessageFactory.text(f"Message sent: {sent_msg_count}")
        )

    def _add_conversation_reference(self, activity):
        conversation_reference = TurnContext.get_conversation_reference(activity)
        self.conversation_references[
            conversation_reference.user.aad_object_id
        ] = conversation_reference


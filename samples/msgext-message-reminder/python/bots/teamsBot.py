import os
from datetime import datetime
from botbuilder.core import ActivityHandler, TurnContext, CardFactory
from botbuilder.schema import Activity, Attachment
from botbuilder.schema.teams import TeamInfo, TeamsChannelAccount
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema.teams import (
    MessagingExtensionAction,
    MessagingExtensionActionResponse,
    TaskModuleResponse,
    TaskModuleContinueResponse,
    TaskModuleTaskInfo,
    MessagingExtensionResponse,
    MessagingExtensionResult,
    MessagingExtensionAttachment
)
from config import DefaultConfig
import schedule
import time

from cards import cards
import threading
from adaptivecards.adaptivecard import AdaptiveCard
import asyncio


class TeamsBot(TeamsActivityHandler):
    def __init__(self, config: DefaultConfig):
        super().__init__()
        self.__base_url = config.BASE_URL
        self.conversation_references = {}
        self.scheduler = None  # Assuming you have a scheduler set up elsewhere
        # Start the scheduler in a separate thread
        threading.Thread(target=self.run_scheduler, daemon=True).start()

    async def on_teams_members_added(
        self,
        teams_members_added: list[TeamsChannelAccount],
        team_info: TeamInfo,
        turn_context: TurnContext,
    ):
        for member in teams_members_added:
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity(
                    "Hello and welcome! With this sample, you can schedule a message reminder by selecting `...` over the message, "
                    "then selecting more actions and creating a reminder. You will get a reminder for the message at the scheduled date and time."
                )

    async def on_teams_messaging_extension_fetch_task(
        self, turn_context: TurnContext, action: MessagingExtensionAction
    ) -> TaskModuleResponse:
        title = "Testing Task Module"
        description = "Description"
        print(action)
        print(action.message_payload)

        if (action.message_payload, "subject") and action.message_payload.subject:
            title = (
                action.message_payload.body.content
                if (action.message_payload.body, "content")
                else ""
            )
            description = action.message_payload.subject
        else:
            title = (
                action.message_payload.body.content
                if (action.message_payload.body, "content")
                else ""
            )

        task_info = TaskModuleTaskInfo(
            width=350,
            height=350,
            title="Schedule task",
            url=f"{self.__base_url}/scheduleTask?title={title}&description={description}",
        )

        return TaskModuleResponse(task=TaskModuleContinueResponse(value=task_info))

    async def on_teams_messaging_extension_submit_action(
        self, context: TurnContext, action: MessagingExtensionAction
    ) -> MessagingExtensionActionResponse:
        task_details = {
            "title": action.data["title"],
            "dateTime": action.data["dateTime"],
            "description": action.data["description"],
        }

        await context.send_activity(
            "Task submitted successfully. You will be reminded at the scheduled time."
        )

        current_user = context.activity.from_property.id
        self.conversation_references[current_user] = (
            TurnContext.get_conversation_reference(context.activity)
        )
        self.adapter = context.adapter

        date_local = datetime.strptime(task_details["dateTime"], "%Y-%m-%dT%H:%M:%SZ")
        schedule_time = date_local.strftime("%H:%M")

        schedule.every().day.at(schedule_time).do(
            lambda: threading.Thread(target=self.send_reminder, args=(current_user, task_details)).start()
        )

    def send_reminder(self, user_id, task_details):
        if user_id in self.conversation_references:
            conversation_reference = self.conversation_references[user_id]

        async def continue_conversation(turn_context: TurnContext):
            card = cards.adaptive_card(task_details)
            await turn_context.send_activity(Activity(attachments=[card]))

        async def run_conversation():
            await self.adapter.continue_conversation(
                conversation_reference, continue_conversation, None
            )

        # Schedule in event loop
        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)
        loop.create_task(run_conversation())

    def run_scheduler(self):
        while True:
            schedule.run_pending()
            time.sleep(60)

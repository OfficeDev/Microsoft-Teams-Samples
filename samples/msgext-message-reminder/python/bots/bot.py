# bot.py
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import schedule
import asyncio
import threading
import time
from datetime import datetime
from botbuilder.schema.teams import (
    MessagingExtensionAction,
    MessagingExtensionActionResponse,
    TaskModuleContinueResponse,
    TaskModuleTaskInfo,
)
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.core import (
    TurnContext,
    CardFactory,
    MessageFactory
)
from botbuilder.schema import Activity

conversation_references = {}
global_adapter = None
task_storage = {}

# Global flag to ensure scheduler thread is started only once
scheduler_started = False

def run_scheduler():
    """Run the schedule checker in a background thread"""
    while True:
        schedule.run_pending()
        time.sleep(1)

class MsgextReminderBot(TeamsActivityHandler):
    def __init__(self, base_url: str, adapter):
        super().__init__()
        self.base_url = base_url
        self.adapter = adapter
        self._start_scheduler()

    async def on_members_added_activity(self, members_added, turn_context: TurnContext):
        """Handle when members are added to the conversation"""
        for member in members_added:
            if member.id != turn_context.activity.recipient.id:
                welcome_text = (
                    "Hello and welcome! With this sample you can schedule a message reminder by "
                    "selecting `...` over the message then select more action and then create-reminder "
                    "and you will get reminder of the message at scheduled date and time."
                )
                await turn_context.send_activity(MessageFactory.text(welcome_text))

    def _start_scheduler(self):
        """Start the scheduler thread if not already started"""
        global scheduler_started
        if not scheduler_started:
            scheduler_thread = threading.Thread(target=run_scheduler, daemon=True)
            scheduler_thread.start()
            scheduler_started = True

    async def on_teams_messaging_extension_fetch_task(
        self, turn_context: TurnContext, action: MessagingExtensionAction
    ) -> MessagingExtensionActionResponse:
        message = action.message_payload
        title = message.body.content if message.body else ""
        description = message.subject if message.subject else ""

        task_info = TaskModuleTaskInfo(
            title="Schedule task",
            height=350,
            width=350,
            url=f"{self.base_url}/scheduleTask?title={title}&description={description}",
        )
        return MessagingExtensionActionResponse(
            task=TaskModuleContinueResponse(value=task_info)
        )

    async def on_teams_messaging_extension_submit_action(
        self, turn_context: TurnContext, action: MessagingExtensionAction
    ) -> MessagingExtensionActionResponse:
        global global_adapter
        global_adapter = turn_context.adapter

        task_data = action.data
        title = task_data.get("taskTitle") 
        description = task_data.get("description")
        date_time_str = task_data.get("dateTime")

        user_id = turn_context.activity.from_property.id
        conversation_references[user_id] = TurnContext.get_conversation_reference(turn_context.activity)

        try:
            if 'T' in date_time_str and len(date_time_str) == 16:
                date_utc = datetime.fromisoformat(date_time_str)
            else:
                try:
                    date_utc = datetime.fromisoformat(date_time_str.replace('Z', '+00:00'))
                except ValueError:
                    import re
                    clean_datetime = re.sub(r'^[A-Za-z]{3},?\s*', '', date_time_str)
                    clean_datetime = re.sub(r'\s+GMT.*$', '', clean_datetime)
                    date_utc = datetime.strptime(clean_datetime, '%d %b %Y %H:%M:%S')
                    
        except Exception as e:
            await turn_context.send_activity(f"Error parsing date/time: {e}")
            return None
            
        task_storage[user_id] = {
            "title": title,
            "description": description,
        }

        # Schedule the reminder
        reminder_time = date_utc.strftime("%H:%M")
        
        schedule.every().day.at(reminder_time).do(
            self._send_reminder, user_id
        ).tag(f"reminder_{user_id}")

        await turn_context.send_activity("Task submitted successfully. You will get a reminder at the scheduled time.")
        return MessagingExtensionActionResponse()

    def _send_reminder(self, user_id):
        task = task_storage.get(user_id)
        if not task:
            return
        
        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)
        try:
            loop.run_until_complete(self._send_reminder_async(user_id, task))
        finally:
            loop.close()

    async def _send_reminder_async(self, user_id, task):
        if user_id not in conversation_references:
            return

        ref = conversation_references[user_id]

        async def send_task(turn_context: TurnContext):
            card = CardFactory.adaptive_card({
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "type": "AdaptiveCard",
                "version": "1.2",
                "body": [
                    {
                        "type": "TextBlock",
                        "size": "Medium",
                        "weight": "Bolder",
                        "text": "Task Reminder!"
                    },
                    {
                        "type": "TextBlock",
                        "text": f"**Title:** {task['title']}",
                        "wrap": True
                    },
                    {
                        "type": "TextBlock",
                        "text": f"**Description:** {task['description']}",
                        "wrap": True
                    },
                    {
                        "type": "TextBlock",
                        "text": f"**Time:** {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}",
                        "wrap": True,
                        "size": "Small"
                    }
                ]
            })
            
            message = MessageFactory.attachment(card)
            await turn_context.send_activity(message)

        try:
            app_id = global_adapter.settings.app_id if global_adapter and global_adapter.settings else None
            
            await global_adapter.continue_conversation(
                ref,
                send_task,
                app_id
            )
            
        except Exception as e:
            pass

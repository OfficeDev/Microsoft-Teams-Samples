# bot.py

import os
from datetime import datetime
from typing import Dict

from apscheduler.schedulers.asyncio import AsyncIOScheduler
from botbuilder.core import TurnContext, CardFactory
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema import Activity
from botbuilder.schema.teams import TaskModuleTaskInfo, TaskModuleMessageResponse, TaskModuleResponse

from models.task_module_response import TaskModuleResponseFactory

conversation_references: Dict[str, dict] = {}
adapter = None
scheduler = AsyncIOScheduler()

class DailyReminderBot(TeamsActivityHandler):
    def __init__(self, app_id: str, app_password: str):
        super().__init__()
        self.base_url = os.environ.get("BaseUrl", "https://your-domain.com")
        self.task_details = {}
        self.scheduler_started = False
        self._app_id = app_id
        self._app_password = app_password

    async def on_members_added_activity(self, members_added, turn_context: TurnContext):
        for member in members_added:
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity("Hello and welcome! With this python sample, you can schedule a recurring task and receive reminders at the scheduled time. Use the command 'create-reminder' to start.")

    async def on_message_activity(self, turn_context: TurnContext):
        if not self.scheduler_started:
            scheduler.start()
            self.scheduler_started = True

        text = turn_context.activity.text.strip().lower()
        if text == "create-reminder":
            card = CardFactory.adaptive_card(self.adaptive_card_for_task_module())
            await turn_context.send_activity(Activity(attachments=[card]))

    async def on_teams_task_module_fetch(self, turn_context: TurnContext, task_module_request):
        if task_module_request.data.get("id") == "schedule":
            task_info = TaskModuleTaskInfo(
                url=f"{self.base_url}/scheduletask",
                fallback_url=f"{self.base_url}/scheduletask",
                height=350,
                width=350,
                title="Schedule a task"
            )
            return TaskModuleResponseFactory.to_task_module_response(task_info)
        return None

    async def on_teams_task_module_submit(self, turn_context: TurnContext, task_module_request):
        global adapter

        data = task_module_request.data
        task_details = {
            "title": data.get("title"),
            "dateTime": data.get("dateTime"),
            "description": data.get("description"),
            "selectedDays": data.get("selectedDays") or [],
        }
        self.save_task_details(task_details)

        await turn_context.send_activity("Task submitted successfully. You will get a reminder as scheduled.")

        user_id = turn_context.activity.from_property.id
        conversation_references[user_id] = TurnContext.get_conversation_reference(turn_context.activity)
        adapter = turn_context.adapter

        dt = datetime.fromisoformat(task_details["dateTime"])
        selected_days = task_details["selectedDays"]

        async def reminder_job():
            await adapter.continue_conversation(
                conversation_references[user_id],
                lambda ctx: ctx.send_activity(Activity(attachments=[CardFactory.adaptive_card(self.reminder_card(task_details))])),
                os.environ.get("MicrosoftAppId")
            )

        def run_async_job(coro):
            import asyncio
            loop = asyncio.new_event_loop()
            asyncio.set_event_loop(loop)
            loop.run_until_complete(coro)
            loop.close()

        scheduler.add_job(lambda: run_async_job(reminder_job()), trigger="date", run_date=dt)

        if selected_days:
            cron_days = ','.join(str(self.convert_day(d)) for d in selected_days)
            scheduler.add_job(lambda: run_async_job(reminder_job()), trigger="cron", day_of_week=cron_days, hour=dt.hour, minute=dt.minute)

        return TaskModuleResponse(task=TaskModuleMessageResponse(value="Task scheduled!"))

    def save_task_details(self, task_details):
        self.task_details = task_details

    def convert_day(self, day_str):
        mapping = {
            "SUN": 0, "MON": 1, "TUE": 2,
            "WED": 3, "THU": 4, "FRI": 5, "SAT": 6
        }
        return mapping.get(day_str.upper())

    def adaptive_card_for_task_module(self) -> dict:
        return {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.2",
            "body": [
                {
                    "type": "TextBlock",
                    "size": "Default",
                    "weight": "Bolder",
                    "text": "Please click on schedule to schedule a task"
                },
                {
                    "type": "ActionSet",
                    "actions": [
                        {
                            "type": "Action.Submit",
                            "title": "Schedule task",
                            "data": {
                                "msteams": {"type": "task/fetch"},
                                "id": "schedule"
                            }
                        }
                    ]
                }
            ]
        }

    def reminder_card(self, task_details) -> dict:
        return {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.2",
            "body": [
                {
                    "type": "TextBlock",
                    "size": "Default",
                    "weight": "Bolder",
                    "text": "Reminder for a scheduled task!"
                },
                {
                    "type": "TextBlock",
                    "text": f"Task title: {task_details['title']}",
                    "wrap": True
                },
                {
                    "type": "TextBlock",
                    "text": f"Task description: {task_details['description']}",
                    "wrap": True
                }
            ]
        }

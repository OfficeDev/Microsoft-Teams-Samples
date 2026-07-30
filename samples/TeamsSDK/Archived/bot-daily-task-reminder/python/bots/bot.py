# bot.py

import os
from datetime import datetime

from apscheduler.schedulers.asyncio import AsyncIOScheduler
from botbuilder.core import TurnContext, CardFactory
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema import Activity
from botbuilder.schema.teams import TaskModuleTaskInfo

from models.task_module_response import TaskModuleResponseFactory

conversation_references: dict[str, dict] = {}
adapter = None
scheduler = AsyncIOScheduler()

class DailyReminderBot(TeamsActivityHandler):
    def __init__(self):
        super().__init__()
        self.base_url = os.environ.get("BaseUrl", "")
        self.scheduler_started = False

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
            card = CardFactory.adaptive_card(self._adaptive_card_for_task_module())
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
            return TaskModuleResponseFactory.create_response(task_info)
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

        await turn_context.send_activity("Task submitted successfully. You will get a reminder as scheduled.")

        user_id = turn_context.activity.from_property.id
        conversation_references[user_id] = TurnContext.get_conversation_reference(turn_context.activity)
        adapter = turn_context.adapter

        dt = datetime.fromisoformat(task_details["dateTime"])
        selected_days = task_details["selectedDays"]

        async def reminder_job():
            await adapter.continue_conversation(
                conversation_references[user_id],
                lambda ctx: ctx.send_activity(Activity(attachments=[CardFactory.adaptive_card(self._reminder_card(task_details))])),
                os.environ.get("MicrosoftAppId")
            )

        scheduler.add_job(reminder_job, trigger="date", run_date=dt)

        if selected_days:
            cron_days = ','.join(d.lower() for d in selected_days)
            scheduler.add_job(reminder_job, trigger="cron", day_of_week=cron_days, hour=dt.hour, minute=dt.minute)

        return TaskModuleResponseFactory.create_response("Task scheduled!")

    @staticmethod
    def _adaptive_card_for_task_module() -> dict:
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

    @staticmethod
    def _reminder_card(task_details) -> dict:
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

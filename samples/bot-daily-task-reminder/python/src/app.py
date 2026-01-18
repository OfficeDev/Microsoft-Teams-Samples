# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
import os
from datetime import datetime
from typing import Dict

from apscheduler.schedulers.asyncio import AsyncIOScheduler
from azure.identity import ManagedIdentityCredential
from microsoft_teams.api import (
    MessageActivity, 
    MessageActivityInput,
    InstalledActivity,
    Attachment
)
from microsoft_teams.api.activities.invoke.task import TaskFetchInvokeActivity, TaskSubmitInvokeActivity
from microsoft_teams.api.models import (
    TaskModuleInvokeResponse,
    UrlTaskModuleTaskInfo,
    TaskModuleMessageResponse,
    TaskModuleContinueResponse
)
from microsoft_teams.apps import ActivityContext, App
from config import Config
from fastapi.responses import FileResponse, HTMLResponse

config = Config()

# Get base URL from environment - use BOT_ENDPOINT if available, otherwise fallback
base_url = os.environ.get("BOT_ENDPOINT", os.environ.get("BaseUrl", "https://your-domain.com"))
print(f"[DEBUG] BOT_ENDPOINT configured as: {base_url}")

# Global state for conversation references and scheduler
conversation_references: Dict[str, dict] = {}
task_details_store: Dict[str, dict] = {}
scheduler = AsyncIOScheduler()
scheduler_started = False

def create_token_factory():
    def get_token(scopes, tenant_id=None):
        credential = ManagedIdentityCredential(client_id=config.APP_ID)
        if isinstance(scopes, str):
            scopes_list = [scopes]
        else:
            scopes_list = scopes
        token = credential.get_token(*scopes_list)
        return token.token
    return get_token

app = App(
    token=create_token_factory() if config.APP_TYPE == "UserAssignedMsi" else None
)

def convert_day(day_str: str) -> int:
    """Convert day string to cron day number."""
    mapping = {
        "SUN": 0, "MON": 1, "TUE": 2,
        "WED": 3, "THU": 4, "FRI": 5, "SAT": 6
    }
    return mapping.get(day_str.upper(), 0)

def adaptive_card_for_task_module() -> dict:
    """Create adaptive card with button to open task module."""
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

def reminder_card(task_details: dict) -> dict:
    """Create reminder card with task details."""
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
                "text": f"Task title: {task_details.get('title', '')}",
                "wrap": True
            },
            {
                "type": "TextBlock",
                "text": f"Task description: {task_details.get('description', '')}",
                "wrap": True
            }
        ]
    }

@app.on_install_add
async def handle_install_add(ctx: ActivityContext[InstalledActivity]) -> None:
    """Handle bot installation - send welcome message."""
    await ctx.send("Hello and welcome! With this python sample, you can schedule a recurring task and receive reminders at the scheduled time. Use the command 'create-reminder' to start.")

@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]):
    """Handle message activities."""
    global scheduler_started
    
    # Start scheduler if not already started
    if not scheduler_started:
        scheduler.start()
        scheduler_started = True
    
    text = ctx.activity.text.strip().lower() if ctx.activity.text else ""
    
    if text == "create-reminder":
        card = adaptive_card_for_task_module()
        attachment = Attachment(
            content_type="application/vnd.microsoft.card.adaptive",
            content=card
        )
        message = MessageActivityInput(attachments=[attachment])
        await ctx.send(message)

@app.on_dialog_open
async def handle_dialog_open(ctx: ActivityContext[TaskFetchInvokeActivity]) -> TaskModuleInvokeResponse:
    """Handle task module fetch - return task module configuration."""
    data = ctx.activity.value.data if ctx.activity.value else {}
    print(f"[DEBUG] on_dialog_open called with data: {data}")
    
    if data.get("id") == "schedule":
        task_url = f"{base_url}/scheduletask"
        print(f"[DEBUG] Opening task module with URL: {task_url}")
        task_info = UrlTaskModuleTaskInfo(
            url=task_url,
            fallback_url=task_url,
            height=350,
            width=350,
            title="Schedule a task"
        )
        return TaskModuleInvokeResponse(task=TaskModuleContinueResponse(value=task_info))
    
    return TaskModuleInvokeResponse(task=TaskModuleMessageResponse(value="Invalid request"))

@app.on_dialog_submit
async def handle_dialog_submit(ctx: ActivityContext[TaskSubmitInvokeActivity]) -> TaskModuleInvokeResponse:
    """Handle task module submit - schedule the reminder."""
    data = ctx.activity.value.data if ctx.activity.value else {}
    
    task_details = {
        "title": data.get("title", ""),
        "dateTime": data.get("dateTime", ""),
        "description": data.get("description", ""),
        "selectedDays": data.get("selectedDays", []),
    }
    
    # Store conversation reference for later use
    user_id = ctx.activity.from_.id
    conversation_references[user_id] = {
        "conversation": {
            "id": ctx.activity.conversation.id,
            "conversation_type": ctx.activity.conversation.conversation_type,
            "tenant_id": ctx.activity.conversation.tenant_id,
        },
        "service_url": ctx.activity.service_url,
        "channel_id": ctx.activity.channel_id,
    }
    task_details_store[user_id] = task_details
    
    # Send confirmation message
    await ctx.send("Task submitted successfully. You will get a reminder as scheduled.")
    
    # Schedule the reminder
    try:
        dt = datetime.fromisoformat(task_details["dateTime"])
        selected_days = task_details["selectedDays"]
        
        # Create reminder function that will be scheduled
        async def send_reminder_job():
            """Send reminder to user."""
            try:
                print(f"[DEBUG] send_reminder_job called for user_id: {user_id}")
                ref = conversation_references.get(user_id)
                task_info = task_details_store.get(user_id)
                print(f"[DEBUG] ref: {ref}, task_info: {task_info}")
                if ref and task_info:
                    card = reminder_card(task_info)
                    attachment = Attachment(
                        content_type="application/vnd.microsoft.card.adaptive",
                        content=card
                    )
                    
                    # Send proactive message using app.send
                    message = MessageActivityInput(attachments=[attachment])
                    conversation_id = ref["conversation"]["id"]
                    print(f"[DEBUG] Sending reminder to conversation_id: {conversation_id}")
                    await app.send(conversation_id, message)
                    print(f"[DEBUG] Reminder sent successfully")
            except Exception as e:
                print(f"[ERROR] Error sending reminder: {e}")
                import traceback
                traceback.print_exc()
        
        # Schedule one-time reminder - use the async function directly
        print(f"[DEBUG] Scheduling one-time reminder for: {dt}")
        scheduler.add_job(
            send_reminder_job,
            trigger="date",
            run_date=dt,
            misfire_grace_time=60  # Allow 60 seconds grace period
        )
        
        # Schedule recurring reminder if days are selected
        if selected_days:
            cron_days = ','.join(str(convert_day(d)) for d in selected_days)
            print(f"[DEBUG] Scheduling recurring reminder for days: {cron_days} at {dt.hour}:{dt.minute}")
            scheduler.add_job(
                send_reminder_job,
                trigger="cron",
                day_of_week=cron_days,
                hour=dt.hour,
                minute=dt.minute,
                misfire_grace_time=60  # Allow 60 seconds grace period
            )
    except Exception as e:
        print(f"[ERROR] Error scheduling task: {e}")
        import traceback
        traceback.print_exc()
    
    return TaskModuleInvokeResponse(task=TaskModuleMessageResponse(value="Task scheduled!"))

# Setup web server for serving the HTML page
@app.http.get("/scheduletask", response_class=HTMLResponse)
async def scheduletask_handler():
    """Serve the schedule task HTML page."""
    file_path = os.path.join(os.path.dirname(__file__), 'views', 'scheduletask.html')
    return FileResponse(file_path, media_type="text/html")

if __name__ == "__main__":
    asyncio.run(app.start())

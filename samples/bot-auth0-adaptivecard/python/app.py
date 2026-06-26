#!/usr/bin/env python3

import uvicorn
from fastapi import FastAPI, Request, Response
from fastapi.responses import HTMLResponse
from fastapi.staticfiles import StaticFiles
from botbuilder.core import (
    BotFrameworkAdapterSettings,
    BotFrameworkAdapter,
    TurnContext,
)
from botbuilder.schema import Activity
from config import DefaultConfig
from bots.teams_conversation_bot import TeamsConversationBot
from controllers.auth_routes import router as auth_router
import json
import asyncio

app = FastAPI()

# Configuration
CONFIG = DefaultConfig()

# Adapter setup
adapter_settings = BotFrameworkAdapterSettings(CONFIG.APP_ID, CONFIG.APP_PASSWORD)
adapter = BotFrameworkAdapter(adapter_settings)

# Catch-all error handler
async def on_error(context: TurnContext, error: Exception):
    print(f"[on_turn_error] unhandled error: {error}")
    await context.send_activity("The bot encountered an error or bug.")
    await context.send_activity("To continue to run this bot, please fix the bot source code.")

adapter.on_turn_error = on_error

# Bot instance
bot = TeamsConversationBot()

# Root page
@app.get("/", response_class=HTMLResponse)
async def index():
    return """
    <html>
        <head><title>Teams Bot + Auth0</title></head>
        <body>
            <h1>Teams Bot + Auth0</h1>
            <p>This bot is running. You can interact with it in Microsoft Teams.</p>
        </body>
    </html>
    """

# Bot message endpoint
@app.post("/api/messages")
async def messages(request: Request):
    body = await request.json()
    activity = Activity().deserialize(body)
    auth_header = request.headers.get("Authorization", "")

    async def call_bot(turn_context: TurnContext):
        await bot.on_turn(turn_context)

    await adapter.process_activity(activity, auth_header, call_bot)
    return Response(status_code=200)

# Auth callback route
app.include_router(auth_router, prefix="/api/auth")
app.mount("/src/views", StaticFiles(directory="src/views"), name="static")

# Run app
if __name__ == "__main__":
    # Use the correct module name (replace `app` with your filename if different)
    uvicorn.run("app:app", host="0.0.0.0", port=CONFIG.PORT, reload=True)
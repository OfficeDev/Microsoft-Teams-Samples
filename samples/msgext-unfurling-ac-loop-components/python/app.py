"""
Main application file for the Teams bot.
This file sets up and configures the bot using aiohttp and botbuilder.
"""

import sys
import traceback
from datetime import datetime
from aiohttp import web
from aiohttp.web import Request, Response, json_response
from botbuilder.core import (
    BotFrameworkAdapter,
    BotFrameworkAdapterSettings,
    TurnContext,
)
from botbuilder.schema import Activity, ActivityTypes

from bots.teams_bot import TeamsBot
from config import config


def create_app() -> web.Application:
    """Create and configure the aiohttp application."""
    
    # Create adapter
    # See https://aka.ms/about-bot-adapter to learn more about adapters.
    settings = BotFrameworkAdapterSettings(
        app_id=config.BOT_ID,
        app_password=config.BOT_PASSWORD
    )
    adapter = BotFrameworkAdapter(settings)

    # Catch-all for errors
    async def on_error(context: TurnContext, error: Exception):
        # This check writes out errors to console log .vs. app insights.
        # NOTE: In production environment, you should consider logging this to Azure
        #       application insights. See https://aka.ms/bottelemetry for telemetry
        #       configuration instructions.
        
        error_message = str(error)
        
        # Don't log common messaging extension errors that are expected
        if "BotNotInConversationRoster" in error_message:
            return
        elif "PermissionError" in error_message and "Invalid AppId" in error_message:
            return
        
        # Log unexpected errors
        print(f"[ERROR] {error}")
        traceback.print_exc()

        # Don't try to send error messages for messaging extensions
        # as they may not have permission to message users
        # await context.send_activity("Sorry, it looks like something went wrong.")

    adapter.on_turn_error = on_error

    # Create the bot that will handle incoming messages.
    bot = TeamsBot()

    # Define the main messaging endpoint
    async def messages(req: Request) -> Response:
        """Main messaging endpoint for the bot."""
        try:
            if "application/json" in req.headers.get("Content-Type", ""):
                body = await req.json()
            else:
                return Response(status=415)

            activity = Activity().deserialize(body)
            auth_header = req.headers.get("Authorization", "")
            
            response = await adapter.process_activity(activity, auth_header, bot.on_turn)
            if response:
                return json_response(data=response.body, status=response.status)
            return Response(status=201)
            
        except Exception as e:
            return Response(status=500)

    # Create the application
    app = web.Application()
    app.router.add_post("/api/messages", messages)
    
    return app


def main():
    """Main function to start the bot application."""
    try:
        app = create_app()
        web.run_app(app, host="0.0.0.0", port=config.PORT)
    except Exception as error:
        print(f"Failed to start bot: {error}")
        raise error


if __name__ == "__main__":
    main()

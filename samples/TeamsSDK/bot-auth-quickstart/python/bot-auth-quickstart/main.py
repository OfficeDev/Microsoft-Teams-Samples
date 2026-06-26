"""
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.

"""

import asyncio
import os
import logging

from azure.core.exceptions import ClientAuthenticationError
from microsoft_teams.api import MessageActivity
from microsoft_teams.apps import ActivityContext, App, AppOptions, ErrorEvent, SignInEvent
from microsoft_teams.graph import get_graph_client

logger = logging.getLogger(__name__)

app_options = AppOptions(default_connection_name=os.getenv("CONNECTION_NAME", "graph"))
app = App(**app_options)

async def get_authenticated_graph_client(ctx: ActivityContext[MessageActivity]):
    """
    Helper function to handle authentication and create Graph client using Token pattern.
    """
    if not ctx.is_signed_in:
        await ctx.send("🔐 Please sign in first to access Microsoft Graph.")
        await ctx.sign_in()
        return None

    try:
        return get_graph_client(ctx.user_token)

    except Exception as e:
        ctx.logger.error(f"Failed to create Graph client: {e}")
        await ctx.send("🔐 Failed to create authenticated client. Trying to sign in again.")
        await ctx.sign_in()
        return None

@app.on_message_pattern("signin")
async def handle_signin_command(ctx: ActivityContext[MessageActivity]):
    """Handle sign-in command."""
    if ctx.is_signed_in:
        await ctx.send("✅ You are already signed in!")
    else:
        await ctx.send("🔐 Signing you in to access Microsoft Graph...")
        await ctx.sign_in()


@app.event("sign_in")
async def handle_sign_in_event(event: SignInEvent):
    """Handle successful sign-in events."""
    await event.activity_ctx.send(
        "✅ **Successfully signed in!**\n\n"
        "You can now use these commands:\n\n"
        "• **profile** - View your profile\n\n"
        "• **signout** - Sign out when done"
    )

@app.on_message_pattern("signout")
async def handle_signout_command(ctx: ActivityContext[MessageActivity]):
    """Handle sign-out command."""
    if not ctx.is_signed_in:
        await ctx.send("ℹ️ You are not currently signed in.")
    else:
        await ctx.sign_out()
        await ctx.send("👋 You have been signed out successfully!")

@app.on_message_pattern("profile")
async def handle_profile_command(ctx: ActivityContext[MessageActivity]):
    """Handle profile command using Graph API with TokenProtocol pattern."""
    try:
        graph = await get_authenticated_graph_client(ctx)
        if not graph:
            return

        # Fetch user profile
        me = await graph.me.get()

        if me:
            profile_info = (
                f"👤 **Your Profile**\n\n"
                f"**Name:** {me.display_name or 'N/A'}\n\n"
                f"**Email:** {me.user_principal_name or 'N/A'}\n\n"
                f"**Job Title:** {me.job_title or 'N/A'}\n\n"
                f"**Department:** {me.department or 'N/A'}\n\n"
                f"**Office:** {me.office_location or 'N/A'}"
            )
            await ctx.send(profile_info)
        else:
            await ctx.send("❌ Could not retrieve your profile information.")

    except ClientAuthenticationError as e:
        ctx.logger.error(f"Authentication error: {e}")
        await ctx.send("🔐 Authentication failed. Please try signing in again.")

    except Exception as e:
        ctx.logger.error(f"Error getting profile: {e}")
        await ctx.send(f"❌ Failed to get your profile: {str(e)}")


@app.on_message
async def handle_default_message(ctx: ActivityContext[MessageActivity]):
    """Handle default message when no pattern matches."""
    await ctx.send(
        "👋 **Hello! I'm a Teams Auth Quickstart and Graph bot.**\n\n"
        "**Available commands:**\n\n"
        "• **signin** - Sign in to your Microsoft account\n\n"
        "• **signout** - Sign out\n\n"
        "• **profile** - Show your profile information\n\n"
    )

@app.event("error")
async def handle_error_event(event: ErrorEvent):
    """Handle error events."""
    logger.error(f"Error occurred: {event.error}")
    if event.context:
        logger.error(f"Context: {event.context}")

if __name__ == "__main__":
    asyncio.run(app.start())

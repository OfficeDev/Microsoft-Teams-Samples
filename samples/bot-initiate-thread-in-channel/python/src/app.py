# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
import re

from azure.identity import ManagedIdentityCredential
from microsoft_teams.api import MessageActivity
from microsoft_teams.apps import ActivityContext, App
from config import Config

config = Config()

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


async def list_team_channels(ctx: ActivityContext[MessageActivity]) -> None:
    """Lists all channels in the current team."""
    try:
        activity = ctx.activity
        team_id = activity.channel_data.team.id if hasattr(activity.channel_data, 'team') and hasattr(activity.channel_data.team, 'id') else None

        if not team_id:
            await ctx.reply("This command can only be used in a team channel.")
            return

        # Use the HTTP client directly to get team channels
        service_url = ctx.api.teams.service_url
        url = f"{service_url}/v3/teams/{team_id}/conversations"
        
        # Make the HTTP request using the teams client's http instance
        response = await ctx.api.teams.http.get(url)
        
        # Parse the JSON response - it should have a 'conversations' field
        response_data = response.json()
        channels_data = response_data if isinstance(response_data, list) else response_data.get('conversations', [])
        
        if not channels_data or len(channels_data) == 0:
            await ctx.reply("No channels found in this team.")
            return

        # Display channels in a formatted list
        message = "```\n"
        message += "LIST OF CHANNELS\n"
        message += "═══════════════════════════════════════\n\n"
        for i, ch in enumerate(channels_data, 1):
            channel_name = ch.get('name') if isinstance(ch, dict) else getattr(ch, 'name', 'General')
            message += f"{i}. {channel_name or 'General'}\n"
        message += "```"
        
        await ctx.reply(message)
    except Exception as e:
        await ctx.reply(f"An error occurred while trying to list the channels. Error details: {str(e)}")


async def start_new_thread_in_channel(ctx: ActivityContext[MessageActivity]) -> None:
    """Starts a new thread in the current Teams channel."""
    try:
        activity = ctx.activity
        teams_channel_id = activity.channel_data.channel.id if hasattr(activity.channel_data, 'channel') and hasattr(activity.channel_data.channel, 'id') else None

        if not teams_channel_id:
            await ctx.reply("This command can only be used in a team channel.")
            return

        # Send initial message to the channel to start a new thread
        service_url = ctx.api.conversations.service_url
        url = f"{service_url}/v3/conversations/{teams_channel_id}/activities"
        
        message_payload = {
            "type": "message",
            "text": "This will start a new thread in the channel."
        }
        
        response = await ctx.api.conversations.http.post(url, json=message_payload)
        response_data = response.json()
        message_id = response_data.get('id')

        # Send a follow-up message in the same thread
        if message_id:
            thread_url = f"{service_url}/v3/conversations/{teams_channel_id};messageid={message_id}/activities"
            thread_payload = {
                "type": "message",
                "text": "This will be the first response to the new thread."
            }
            await ctx.api.conversations.http.post(thread_url, json=thread_payload)
            await ctx.reply("Successfully started a new thread in the channel!")
    except Exception as e:
        await ctx.reply(f"Error starting thread: {str(e)}")


async def get_team_member(ctx: ActivityContext[MessageActivity]) -> None:
    """Retrieves the details of the user who sent the message."""
    try:
        activity = ctx.activity
        aad_object_id = getattr(activity.from_, 'aad_object_id', None)
        team_id = activity.channel_data.team.id if hasattr(activity.channel_data, 'team') and hasattr(activity.channel_data.team, 'id') else None

        if not team_id:
            await ctx.reply("This command can only be used in a team channel.")
            return

        if not aad_object_id:
            await ctx.reply("Unable to retrieve user information.")
            return

        # Get team member information
        team_member = await ctx.api.conversations.members(activity.conversation.id).get(aad_object_id)

        if not team_member:
            await ctx.reply("Team member not found.")
            return

        # Display user information in a formatted way
        user_info = "```\n"
        user_info += "USER INFORMATION\n"
        user_info += "═══════════════════════════════════════\n\n"
        user_info += f"Name:                  {getattr(team_member, 'name', 'N/A') or 'N/A'}\n"
        user_info += f"Email:                 {getattr(team_member, 'email', 'N/A') or 'N/A'}\n"
        user_info += f"Given Name:            {getattr(team_member, 'given_name', 'N/A') or 'N/A'}\n"
        user_info += f"Surname:               {getattr(team_member, 'surname', 'N/A') or 'N/A'}\n"
        user_info += f"Role:                  {getattr(team_member, 'role', 'N/A') or 'N/A'}\n"
        user_info += f"User Principal Name:   {getattr(team_member, 'user_principal_name', 'N/A') or 'N/A'}\n"
        user_info += "```"

        await ctx.reply(user_info)
    except Exception as e:
        await ctx.reply(f"Error retrieving team member: {str(e)}")


async def get_paged_team_members(ctx: ActivityContext[MessageActivity]) -> None:
    """Retrieves all team members in a paginated manner."""
    try:
        activity = ctx.activity
        team_id = activity.channel_data.team.id if hasattr(activity.channel_data, 'team') and hasattr(activity.channel_data.team, 'id') else None

        if not team_id:
            await ctx.reply("This command can only be used in a team channel.")
            return

        # Get all members in the conversation
        members = await ctx.api.conversations.members(activity.conversation.id).get_all()

        if not members or len(members) == 0:
            await ctx.reply("No team members found.")
            return

        # Display team members in a formatted list
        members_list = "```\n"
        members_list += f"TEAM MEMBERS ({len(members)} total)\n"
        members_list += "═══════════════════════════════════════\n\n"

        for index, member in enumerate(members):
            name = getattr(member, 'name', 'Unknown') or 'Unknown'
            members_list += f"{index + 1}. {name.upper()}\n"
            members_list += f"   Email:      {getattr(member, 'email', 'N/A') or 'N/A'}\n"
            members_list += f"   Given Name: {getattr(member, 'given_name', 'N/A') or 'N/A'}\n"
            members_list += f"   Surname:    {getattr(member, 'surname', 'N/A') or 'N/A'}\n"
            members_list += f"   Role:       {getattr(member, 'role', 'N/A') or 'N/A'}\n"
            members_list += f"   UPN:        {getattr(member, 'user_principal_name', 'N/A') or 'N/A'}\n"
            if index < len(members) - 1:
                members_list += "\n───────────────────────────────────────\n\n"
        
        members_list += "```"

        await ctx.reply(members_list)
    except Exception as e:
        await ctx.reply(f"Error retrieving team members: {str(e)}")


@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]):
    """Handle message activities."""
    activity = ctx.activity
    # Remove mentions and clean the text
    text = re.sub(r'<at>.*?</at>', '', activity.text or '').strip().lower()

    if 'listchannels' in text:
        await list_team_channels(ctx)
    elif 'threadchannel' in text:
        await start_new_thread_in_channel(ctx)
    elif 'getteammember' in text:
        await get_team_member(ctx)
    elif 'getpagedteammembers' in text:
        await get_paged_team_members(ctx)
    else:
        await ctx.reply("I didn't understand that command. Please try again.")


if __name__ == "__main__":
    asyncio.run(app.start())


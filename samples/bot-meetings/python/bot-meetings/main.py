# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
import os
import re
from azure.identity import ClientSecretCredential
from dotenv import load_dotenv
from kiota_abstractions.base_request_configuration import RequestConfiguration
from microsoft_teams.api import (
    MeetingEndEventActivity,
    MeetingStartEventActivity,
    MeetingParticipantJoinEventActivity,
    MeetingParticipantLeaveEventActivity
)
from microsoft_teams.apps import ActivityContext, App
from microsoft_teams.cards import AdaptiveCard, OpenUrlAction, TextBlock
from microsoft_teams.graph import get_graph_client

load_dotenv()

# Authenticate the app, needed for the transcript API
_credential = ClientSecretCredential(
    tenant_id=os.environ.get('TENANT_ID', ''),
    client_id=os.environ.get('CLIENT_ID', ''),
    client_secret=os.environ.get('CLIENT_SECRET', ''),
)

def _get_token() -> str:
    token = _credential.get_token('https://graph.microsoft.com/.default')
    return token.token

graph_client = get_graph_client(_get_token)

app = App()

async def get_meeting_transcript(meeting_resource_id: str, user_id: str) -> str:
    # Retrieve metadata for all the transcripts
    transcripts_metadata = await (
        graph_client.users.by_user_id(user_id)
        .online_meetings.by_online_meeting_id(meeting_resource_id)
        .transcripts.get()
    )

    if not transcripts_metadata or not transcripts_metadata.value:
        return ''

    latest_transcript = max(transcripts_metadata.value, key=lambda t: t.created_date_time or '')
    transcript_id = latest_transcript.id
    request_config = RequestConfiguration()
    request_config.headers.add("Accept", "text/vtt")

    # Retrieve the transcript content
    content = await (
        graph_client.users.by_user_id(user_id)
        .online_meetings.by_online_meeting_id(meeting_resource_id)
        .transcripts.by_call_transcript_id(transcript_id)
        .content.get(request_configuration=request_config)
    )

    return content.decode('utf-8') if content else ''


@app.on_meeting_start
async def handle_meeting_start(ctx: ActivityContext[MeetingStartEventActivity]) -> None:
    value = ctx.activity.value

    card = AdaptiveCard(
        body=[
            TextBlock(text="The meeting has started.", weight="Bolder", size="Large", wrap=True),
            TextBlock(text=f"**Title:** {value.title}", wrap=True),
            TextBlock(text=f"**Start Time:** {value.start_time}", wrap=True)
        ],
        actions=[OpenUrlAction(url=value.join_url, title="Join Meeting")],
    )
    await ctx.send(card)


def parse_vtt(vtt: str) -> str:
    """Convert a WebVTT transcript to 'Speaker: text' lines."""
    lines = []
    for line in vtt.splitlines():
        line = line.strip()
        if not line or line.startswith("WEBVTT") or "-->" in line:
            continue
        # Replace <v Speaker Name>text with Speaker Name: text
        line = re.sub(r"<v ([^>]+)>", r"\1: ", line)
        # Strip any remaining VTT tags like </v>, <c>, etc.
        line = re.sub(r"<[^>]+>", "", line).strip()
        if line:
            lines.append(line)
    return "\n".join(lines)


@app.on_meeting_end
async def handle_meeting_end(ctx: ActivityContext[MeetingEndEventActivity]) -> None:
    value = ctx.activity.value
    meeting_id = ctx.activity.channel_data.meeting.id
    ms_graph_resource_id = getattr(ctx.activity.channel_data.meeting.details, 'ms_graph_resource_id', None)
    meeting_info = await ctx.api.meetings.get_by_id(meeting_id)

    # Retrieve the user ID of the organizer for the transcript API
    user_id = ""
    if meeting_info and meeting_info.organizer:
        user_id = getattr(meeting_info.organizer, 'aadObjectId', None) or ""

    if not ms_graph_resource_id and meeting_info and meeting_info.details:
        ms_graph_resource_id = meeting_info.details.ms_graph_resource_id

    transcript = ''
    if ms_graph_resource_id:
        vtt_transcript = await get_meeting_transcript(ms_graph_resource_id, user_id)
        if vtt_transcript:
            transcript = parse_vtt(vtt_transcript)

    transcript_blocks = (
        [TextBlock(text=line, wrap=True) for line in transcript.splitlines() if line]
        if transcript
        else [TextBlock(text="Transcript not available for this meeting.", wrap=True)]
    )
    card = AdaptiveCard(
        body=[
            TextBlock(text="The meeting has ended.", weight="Bolder", size="Large", wrap=True),
            TextBlock(text=f"**End Time:** {value.end_time}", wrap=True),
            TextBlock(text="**Transcript:**", weight="Bolder", wrap=True),
            *transcript_blocks,
        ],
    )
    await ctx.send(card)

@app.on_meeting_participant_join
async def handle_meeting_participant_join(ctx: ActivityContext[MeetingParticipantJoinEventActivity]):
    meeting_data = ctx.activity.value
    member = meeting_data.members[0].user.name
    role = meeting_data.members[0].meeting.role if hasattr(meeting_data.members[0].meeting, "role") else "a participant"

    card = AdaptiveCard(
        body=[
            TextBlock(
                text=f"{member} has joined the meeting as {role}.",
                wrap=True,
                weight="Bolder",
            )
        ]
    )

    await ctx.send(card)


@app.on_meeting_participant_leave
async def handle_meeting_participant_leave(ctx: ActivityContext[MeetingParticipantLeaveEventActivity]):
    meeting_data = ctx.activity.value
    member = meeting_data.members[0].user.name

    card = AdaptiveCard(
        body=[
            TextBlock(
                text=f"{member} has left the meeting.",
                wrap=True,
                weight="Bolder",
            )
        ]
    )

    await ctx.send(card)


if __name__ == "__main__":
    asyncio.run(app.start())

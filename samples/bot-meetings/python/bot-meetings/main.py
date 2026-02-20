# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
import os
from typing import Dict
import httpx

from fastapi import Response, Request
from dotenv import load_dotenv
from microsoft_teams.api import MessageActivityInput
from microsoft_teams.api.activities.event.meeting_participant_join import MeetingParticipantJoinEventActivity
from microsoft_teams.api.activities.event.meeting_participant_leave import MeetingParticipantLeaveEventActivity
from microsoft_teams.apps import ActivityContext, App
from microsoft_teams.cards import AdaptiveCard, TextBlock, SubmitAction

# Load environment variables
load_dotenv()

# Constants
GRAPH_API_ENDPOINT = 'https://graph.microsoft.com/beta'

# Global transcript storage
transcripts_dictionary = []

# Global meeting data storage for tracking start times
meeting_data = {}

# Create app instance
app = App()

async def get_access_token() -> str:
    try:
        client_id = os.environ.get('CLIENT_ID', '')
        client_secret = os.environ.get('CLIENT_SECRET', '')
        tenant_id = os.environ.get('TENANT_ID', '')
        token_url = f"https://login.microsoftonline.com/{tenant_id}/oauth2/v2.0/token"        
        data = {
            'grant_type': 'client_credentials',
            'client_id': client_id,
            'client_secret': client_secret,
            'scope': 'https://graph.microsoft.com/.default'
        }        
        async with httpx.AsyncClient() as client:
            response = await client.post(token_url, data=data)
            if response.status_code == 200:
                result = response.json()
                return result.get('access_token', '')
            else:
                error_text = response.text
                print(f"Error getting token: {response.status_code} - {error_text}")
                return ''
    except Exception as e:
        print(f"Exception getting access token: {str(e)}")
        return ''


async def get_meeting_transcript(ms_graph_resource_id: str) -> str:
    try:
        access_token = await get_access_token()
        if not access_token:
            return ""        
        user_id = os.environ.get('USER_ID', '')
        if not user_id:
            return ""        
        async with httpx.AsyncClient() as client:
            headers = {'Authorization': f'Bearer {access_token}'}            
            transcripts_url = f"{GRAPH_API_ENDPOINT}/users/{user_id}/onlineMeetings/{ms_graph_resource_id}/transcripts"            
            trans_response = await client.get(transcripts_url, headers=headers)
            if trans_response.status_code == 200:
                transcripts_data = trans_response.json()
                transcripts = transcripts_data.get('value', [])                    
                if transcripts:
                    transcript_id = transcripts[0].get('id')
                    content_url = f"{transcripts_url}/{transcript_id}/content?$format=text/vtt"                        
                    content_response = await client.get(content_url, headers=headers)
                    if content_response.status_code == 200:
                        transcript_content = content_response.text
                        return transcript_content
                    else:
                        error_text = content_response.text
                        print(f"[ERROR] Getting transcript content: {content_response.status_code} - {error_text}")            
            return ""    
    except Exception as ex:
        print(f"[ERROR] Exception in get_meeting_transcript: {str(ex)}")
        return ""


@app.on_invoke
async def handle_task_fetch(ctx: ActivityContext) -> Dict:
    if hasattr(ctx.activity, 'name') and ctx.activity.name == 'task/fetch':
        try:
            meeting_id = None
            if hasattr(ctx.activity, 'value') and ctx.activity.value:
                value_data = ctx.activity.value
                if isinstance(value_data, dict):
                    data_section = value_data.get('data', {})
                    meeting_id = data_section.get('meetingId')
                elif hasattr(value_data, 'data'):
                    data_section = value_data.data
                    meeting_id = getattr(data_section, 'meetingId', None) if hasattr(data_section, 'meetingId') else data_section.get('meetingId') if isinstance(data_section, dict) else None            
            base_url = os.environ.get('APP_BASE_URL', '')
            task_module_url = f"{base_url}/home"
            if meeting_id:
                task_module_url += f"?meetingId={meeting_id}"            
            response = {
                "task": {
                    "type": "continue",
                    "value": {
                        "title": "Meeting Transcript",
                        "height": 600,
                        "width": 600,
                        "url": task_module_url
                    }
                }
            }
            return response
        except Exception as ex:
            print(f"[ERROR] Task fetch handler error: {str(ex)}")
            return {
                "task": {
                    "type": "continue",
                    "value": {
                        "title": "Meeting Transcript",
                        "height": 600,
                        "width": 600,
                        "url": f"{os.environ.get('APP_BASE_URL', '')}/home"
                    }
                }
            }

@app.on_meeting_start
async def handle_meeting_start(ctx: ActivityContext) -> None:
    """Handle meeting start event."""
    try:
        meeting_id = None
        start_time = None
        start_time_str = ""
        join_url = ""
        meeting_title = ""
        
        # Get start_time from activity.value (it's a datetime object)
        if hasattr(ctx.activity, 'value') and ctx.activity.value:
            start_time_dt = getattr(ctx.activity.value, 'start_time', None)
            if start_time_dt:
                # Convert datetime to ISO string format for storage
                start_time = start_time_dt.isoformat()
                # Format for display - match meetings-events format (JavaScript toString())
                from datetime import datetime
                print(f"[DEBUG] start_time_dt: {start_time_dt}, type: {type(start_time_dt)}")
                # Convert to string similar to JavaScript's toString()
                start_time_str = str(start_time_dt)
            
            # Get join URL and title
            join_url = getattr(ctx.activity.value, 'join_url', '')
            meeting_title = getattr(ctx.activity.value, 'title', '')
            
        # Get meeting_id from channel_data
        if hasattr(ctx.activity, 'channel_data') and ctx.activity.channel_data:
            if hasattr(ctx.activity.channel_data, 'meeting') and ctx.activity.channel_data.meeting:
                meeting_id = ctx.activity.channel_data.meeting.id
        
        # Store start time for duration calculation
        if meeting_id and start_time:
            global meeting_data
            meeting_data[meeting_id] = {'start_time': start_time}
            print(f"[DEBUG] Stored meeting start - ID: {meeting_id}, Time: {start_time}")
        else:
            print(f"[DEBUG] Missing data - meeting_id: {meeting_id}, start_time: {start_time}")
        
        # Build card body
        card_body = [
            TextBlock(
                text="Meeting has started!",
                weight="Bolder",
                size="Large",
                wrap=True
            )
        ]
        
        if meeting_title:
            card_body.append(TextBlock(
                text=f"**Title:** {meeting_title}",
                wrap=True
            ))
        
        if start_time_str:
            card_body.append(TextBlock(
                text=f"**Start Time:** {start_time_str}",
                wrap=True
            ))
        
        # Add actions if join URL is available
        actions = []
        if join_url:
            # Use raw action for opening URL since SDK might not have OpenUrlAction
            actions.append({
                "type": "Action.OpenUrl",
                "title": "Join Meeting",
                "url": join_url
            })
        
        card = AdaptiveCard(
            body=card_body
        )
        
        # Convert to dict and add actions manually
        card_dict = card.model_dump(exclude_none=True, by_alias=True)
        if actions:
            card_dict["actions"] = actions
        
        attachment = {
            "contentType": "application/vnd.microsoft.card.adaptive",
            "content": card_dict
        }
        
        activity = MessageActivityInput(
            text="",
            attachments=[attachment]
        )
        await ctx.send(activity)
    except Exception as error:
        print(f"[ERROR] Meeting start handler exception: {str(error)}")

@app.on_meeting_end
async def handle_meeting_end(ctx: ActivityContext) -> None:
    """Handle meeting end event."""
    try:
        meeting_id = None
        ms_graph_resource_id = None
        duration_text = ""
        end_time_str = ""
        
        if hasattr(ctx.activity, 'channel_data') and ctx.activity.channel_data:
            if hasattr(ctx.activity.channel_data, 'meeting') and ctx.activity.channel_data.meeting:
                meeting_id = ctx.activity.channel_data.meeting.id                    
                if hasattr(ctx.activity.channel_data.meeting, 'details'):
                    ms_graph_resource_id = getattr(ctx.activity.channel_data.meeting.details, 'ms_graph_resource_id', None)
        if meeting_id and not ms_graph_resource_id:
            try:
                meeting_info = await ctx.api.meetings.get_by_id(meeting_id)
                if meeting_info and meeting_info.details:
                    ms_graph_resource_id = meeting_info.details.ms_graph_resource_id
            except Exception as ex:
                pass            
        if meeting_id:
            # Calculate meeting duration
            global meeting_data
            print(f"[DEBUG] Meeting end - ID: {meeting_id}, Stored data: {meeting_data}")
            if meeting_id in meeting_data:
                start_time = meeting_data[meeting_id].get('start_time')
                if start_time and hasattr(ctx.activity, 'value') and ctx.activity.value:
                    # Get end_time (lowercase with underscore, datetime object)
                    end_time_dt = getattr(ctx.activity.value, 'end_time', None)
                    if end_time_dt:
                        from datetime import datetime
                        # Convert to ISO string if it's a datetime object
                        end_time = end_time_dt.isoformat() if hasattr(end_time_dt, 'isoformat') else end_time_dt
                        start_dt = datetime.fromisoformat(start_time.replace('Z', '+00:00'))
                        end_dt = datetime.fromisoformat(end_time.replace('Z', '+00:00'))
                        duration_seconds = int((end_dt - start_dt).total_seconds())
                        minutes = duration_seconds // 60
                        seconds = duration_seconds % 60
                        duration_text = f"{minutes} min {seconds} sec" if minutes >= 1 else f"{seconds} sec"
                        # Format end time for display - match meetings-events format (JavaScript toString())
                        print(f"[DEBUG] end_dt: {end_dt}, type: {type(end_dt)}")
                        end_time_str = str(end_dt)
                # Clean up stored data
                del meeting_data[meeting_id]
            
            result = None
            if ms_graph_resource_id:
                result = await get_meeting_transcript(ms_graph_resource_id)                
            if result:
                result = result.replace("<v", "")
                global transcripts_dictionary
                found_index = next((i for i, item in enumerate(transcripts_dictionary) if item['id'] == ms_graph_resource_id), -1)                    
                if found_index != -1:
                    transcripts_dictionary[found_index]['data'] = result
                else:
                    transcripts_dictionary.append({
                        'id': ms_graph_resource_id,
                        'data': result
                    })
                
                # Build card body with end time and duration
                card_body = [
                    TextBlock(
                        text="Meeting has ended!",
                        weight="Bolder",
                        size="Large",
                        wrap=True
                    )
                ]
                
                if end_time_str:
                    card_body.append(TextBlock(
                        text=f"**End Time:** {end_time_str}",
                        wrap=True
                    ))
                
                if duration_text:
                    card_body.append(TextBlock(
                        text=f"**Duration:** {duration_text}",
                        wrap=True
                    ))
                
                card_body.append(TextBlock(
                    text="Transcript is available below:",
                    wrap=True
                ))
                
                card = AdaptiveCard(
                    body=card_body,
                    actions=[
                        SubmitAction(
                            title="View Transcript",
                            data={
                                "msteams": {
                                    "type": "task/fetch"
                                },
                                "meetingId": ms_graph_resource_id
                            }
                        )
                    ]
                )                    
                attachment = {
                    "contentType": "application/vnd.microsoft.card.adaptive",
                    "content": card.model_dump(exclude_none=True, by_alias=True)
                }                    
                activity = MessageActivityInput(
                    text="",
                    attachments=[attachment]
                )
                await ctx.send(activity)
            else:
                not_found_card = AdaptiveCard(
                    body=[
                        TextBlock(
                            text="Transcript not found for this meeting.",
                            weight="Bolder",
                            size="Large"
                        )
                    ]
                )                    
                attachment = {
                    "contentType": "application/vnd.microsoft.card.adaptive",
                    "content": not_found_card.model_dump(exclude_none=True, by_alias=True)
                }                    
                activity = MessageActivityInput(
                    text="",
                    attachments=[attachment]
                )
                await ctx.send(activity)
    except Exception as error:
        print(f"[ERROR] Meeting end handler exception: {str(error)}")

@app.http.get('/home')
async def home_handler(request: Request):
    try:
        transcript = "Transcript not found."
        meeting_id = request.query_params.get('meetingId')       
        if meeting_id:
            global transcripts_dictionary
            found_index = next((i for i, item in enumerate(transcripts_dictionary) if item['id'] == meeting_id), -1)            
            if found_index != -1:
                transcript = f"Format: {transcripts_dictionary[found_index]['data']}"
            else:
                result = await get_meeting_transcript(meeting_id)                
                if result:
                    transcripts_dictionary.append({
                        'id': meeting_id,
                        'data': result
                    })
                    transcript = f"Format: {result}"
        html_content = f'''
        <!DOCTYPE html>
        <html>
        <head>
            <title>Meeting Transcript</title>
            <style>
                body {{ font-family: Arial, sans-serif; padding: 20px; }}
                h1 {{ color: #464775; }}
                pre {{ white-space: pre-wrap; word-wrap: break-word; }}
            </style>
        </head>
        <body>
            <h1>Meeting Transcript</h1>
            <pre>{transcript}</pre>
        </body>
        </html>
        '''
        return Response(content=html_content, media_type='text/html')
    except Exception as error:
        print(f"[ERROR] /home endpoint: {str(error)}")
        return Response(content=f"Error: {str(error)}", status_code=500, media_type='text/plain')

if __name__ == "__main__":
    asyncio.run(app.start())

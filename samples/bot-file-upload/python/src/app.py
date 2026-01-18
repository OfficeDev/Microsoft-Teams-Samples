# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
import os
import base64
from pathlib import Path
from azure.identity import ManagedIdentityCredential
from microsoft_teams.api import MessageActivity, InvokeActivity, InstalledActivity, FileConsentInvokeActivity, Attachment, MessageActivityInput
from microsoft_teams.apps import ActivityContext, App
from config import Config
import aiohttp
import aiofiles

config = Config()

# Get the directory where the script is located and construct the files directory path
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
FILES_DIR = os.path.join(BASE_DIR, "files")
DEFAULT_FILE_NAME = "teams-logo.png"

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

async def download_file(url: str, file_path: str) -> None:
    """Download file from URL and save to disk."""
    async with aiohttp.ClientSession() as session:
        async with session.get(url) as response:
            if response.status == 200:
                # Ensure directory exists
                os.makedirs(os.path.dirname(file_path), exist_ok=True)
                async with aiofiles.open(file_path, 'wb') as f:
                    await f.write(await response.read())

async def upload_file(url: str, file_path: str) -> None:
    """Upload file to Teams."""
    file_size = os.path.getsize(file_path)
    
    async with aiofiles.open(file_path, 'rb') as f:
        file_content = await f.read()
        
    headers = {
        "Content-Length": str(file_size),
        "Content-Range": f"bytes 0-{file_size - 1}/{file_size}"
    }
    
    async with aiohttp.ClientSession() as session:
        async with session.put(url, data=file_content, headers=headers) as response:
            if response.status not in [200, 201]:
                raise Exception(f"Upload failed with status {response.status}")

async def send_file_card(ctx: ActivityContext, filename: str) -> None:
    """Send a FileConsentCard to get user consent to upload a file."""
    file_path = os.path.join(FILES_DIR, filename)
    
    print(f"[DEBUG] Looking for file at: {file_path}")
    print(f"[DEBUG] File exists: {os.path.exists(file_path)}")
    
    if not os.path.exists(file_path):
        await ctx.send(f"<b>File upload failed.</b> Error: <pre>File not found at {file_path}</pre>")
        return
    
    file_size = os.path.getsize(file_path)
    consent_context = {"filename": filename}
    
    file_card = {
        "description": "This is the file I want to send you",
        "sizeInBytes": file_size,
        "acceptContext": consent_context,
        "declineContext": consent_context
    }
    
    attachment = Attachment(
        content=file_card,
        content_type="application/vnd.microsoft.teams.card.file.consent",
        name=filename
    )
    
    message = MessageActivityInput(attachments=[attachment])
    await ctx.send(message)

async def generate_file_name() -> str:
    """Generate a unique file name for inline images."""
    filename_prefix = "UserAttachment"
    files = os.listdir(FILES_DIR) if os.path.exists(FILES_DIR) else []
    filtered_files = [
        int(f.split(filename_prefix)[1].split('.')[0])
        for f in files
        if filename_prefix in f and f.split(filename_prefix)[1].split('.')[0].isdigit()
    ]
    max_seq = max(filtered_files) if filtered_files else 0
    return f"{filename_prefix}{max_seq + 1}.png"

@app.on_install_add
async def handle_install(ctx: ActivityContext[InstalledActivity]) -> None:
    """Handle membersAdded event to send welcome message."""
    print("[DEBUG] Install event triggered")
    await ctx.send(
        'Welcome to the File Upload Bot! You can send me files and I can also send files to you. '
        'Try saying "send file" to get a file from me.'
    )

@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]) -> None:
    """Handle message activities."""
    activity = ctx.activity
    text = activity.text or ""
    attachments = activity.attachments or []
    
    print(f"[DEBUG] Received message: '{text}'")
    print(f"[DEBUG] Attachments: {len(attachments)}")
    
    # Handle file-related commands
    if "send file" in text.lower() or "file" in text.lower():
        print("[DEBUG] Triggering send_file_card")
        await send_file_card(ctx, DEFAULT_FILE_NAME)
        return
    
    # Handle attachments - only process actual file attachments
    if attachments:
        attachment = attachments[0]
        content_type = attachment.content_type or ""
        
        print(f"[DEBUG] Attachment content type: {content_type}")
        
        # Handle file download info
        if content_type == "application/vnd.microsoft.teams.file.download.info":
            await handle_file_download(ctx, attachment)
            return
        # Handle inline images
        elif content_type.startswith("image/"):
            await process_inline_image(ctx, attachment)
            return
    
    # Default: echo message
    print(f"[DEBUG] Echoing message")
    await ctx.send(f"You said '{text}'")

async def handle_file_download(ctx: ActivityContext, attachment) -> None:
    """Handle file download from Teams."""
    try:
        file_content = attachment.content
        download_url = file_content.get("downloadUrl")
        file_name = attachment.name
        file_path = os.path.join(FILES_DIR, file_name)
        
        await download_file(download_url, file_path)
        await ctx.send(f"Completed downloading <b>{file_name}</b>")
    except Exception as e:
        await ctx.send(f"<b>File download failed.</b> Error: <pre>{str(e)}</pre>")

async def process_inline_image(ctx: ActivityContext, attachment) -> None:
    """Process an inline image by saving it and notifying the user."""
    try:
        content_url = attachment.content_url
        file_name = await generate_file_name()
        file_path = os.path.join(FILES_DIR, file_name)
        
        await download_file(content_url, file_path)
        file_size = os.path.getsize(file_path)
        
        # Read file and encode as base64
        async with aiofiles.open(file_path, 'rb') as f:
            image_data = await f.read()
        base64_image = base64.b64encode(image_data).decode('utf-8')
        
        from microsoft_teams.api import Attachment
        
        inline_attachment = Attachment(
            name=file_name,
            content_type="image/png",
            content_url=f"data:image/png;base64,{base64_image}"
        )
        
        message = MessageActivityInput(
            text=f"Image <b>{file_name}</b> of size <b>{file_size}</b> bytes received and saved.",
            attachments=[inline_attachment]
        )
        await ctx.send(message)
    except Exception as e:
        await ctx.send(f"<b>Error processing image.</b> Error: <pre>{str(e)}</pre>")

@app.on_file_consent
async def handle_file_consent(ctx: ActivityContext[FileConsentInvokeActivity]) -> None:
    """Handle file consent activities."""
    activity = ctx.activity
    file_consent_response = activity.value
    action = file_consent_response.action
    
    if action == "accept":
        try:
            file_name = file_consent_response.context["filename"]
            file_path = os.path.join(FILES_DIR, file_name)
            upload_url = file_consent_response.upload_info.upload_url
            
            await upload_file(upload_url, file_path)
            await file_upload_completed(ctx, file_consent_response)
        except Exception as e:
            await file_upload_failed(ctx, str(e))
    elif action == "decline":
        file_name = file_consent_response.context["filename"]
        await ctx.send(f"Declined. We won't upload file <b>{file_name}</b>.")


async def file_upload_completed(ctx: ActivityContext, file_consent_response) -> None:
    """Notify user when file upload is completed."""
    upload_info = file_consent_response.upload_info
    
    download_card = {
        "uniqueId": upload_info.unique_id,
        "fileType": upload_info.file_type
    }
    
    attachment = Attachment(
        content=download_card,
        content_type="application/vnd.microsoft.teams.card.file.info",
        name=upload_info.name,
        content_url=upload_info.content_url
    )
    
    message = MessageActivityInput(
        text=f"<b>File uploaded.</b> Your file <b>{upload_info.name}</b> is ready to download",
        attachments=[attachment]
    )
    await ctx.send(message)

async def file_upload_failed(ctx: ActivityContext, error: str) -> None:
    """Handle file upload failure."""
    await ctx.send(f"<b>File upload failed.</b> Error: <pre>{error}</pre>")

if __name__ == "__main__":
    asyncio.run(app.start())

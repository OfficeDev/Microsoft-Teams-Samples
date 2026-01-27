# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import asyncio
import os
import uuid

from dotenv import load_dotenv
from microsoft_teams.api import (
    Attachment,
    InstalledActivity,
    InvokeActivity,
    InvokeResponse,
    MessageActivity,
    MessageActivityInput,
)
from microsoft_teams.apps import ActivityContext, App
from microsoft_teams.common.http.client import Client, ClientOptions

load_dotenv()

# Configuration
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
FILES_DIR = os.path.join(BASE_DIR, "files")
DEFAULT_FILE_NAME = "teams-logo.png"

# In-memory cache for pending file uploads (file_id -> {content, size})
pending_uploads: dict[str, dict] = {}

app = App()


# Helper function to get values from dict or object
def get_value(obj, key: str, attr: str = None):
    """Get value from dict or object."""
    attr = attr or key
    return obj.get(key) if isinstance(obj, dict) else getattr(obj, attr, None)


async def download_content(url: str) -> bytes:
    """Download content from URL and return as bytes."""
    client = Client(ClientOptions())
    response = await client.get(url)
    if response.status_code == 200:
        return response.content
    raise Exception(f"Download failed with status {response.status_code}")


async def upload_content(url: str, content: bytes) -> None:
    """Upload content to OneDrive."""
    file_size = len(content)

    headers = {
        "Content-Type": "application/octet-stream",
        "Content-Length": str(file_size),
        "Content-Range": f"bytes 0-{file_size - 1}/{file_size}"
    }

    client = Client(ClientOptions(headers=headers))
    response = await client.put(url, content=content)

    if response.status_code not in [200, 201]:
        raise Exception(f"Upload failed with status {response.status_code}")


async def upload_file_from_disk(url: str, file_path: str) -> None:
    """Upload file from disk to OneDrive (for default file)."""
    with open(file_path, 'rb') as f:
        content = f.read()
    await upload_content(url, content)

async def send_file_card(ctx: ActivityContext, filename: str, file_id: str = None) -> None:
    """Send a FileConsentCard to request permission to upload a file."""
    # Check if it's a cached upload or a local file
    if file_id and file_id in pending_uploads:
        file_size = pending_uploads[file_id]["size"]
    else:
        # Local file (default file)
        file_path = os.path.join(FILES_DIR, filename)
        if not os.path.exists(file_path):
            await ctx.send(f"<b>Error:</b> File not found: {filename}")
            return
        file_size = os.path.getsize(file_path)
        file_id = None  # Mark as local file

    consent_context = {"filename": filename, "file_id": file_id}

    attachment = Attachment(
        content={
            "description": "This is the file I want to send you",
            "sizeInBytes": file_size,
            "acceptContext": consent_context,
            "declineContext": consent_context
        },
        content_type="application/vnd.microsoft.teams.card.file.consent",
        name=filename
    )

    await ctx.send(MessageActivityInput(attachments=[attachment]))


@app.on_install_add
async def handle_install(ctx: ActivityContext[InstalledActivity]) -> None:
    """Handle membersAdded event to send welcome message."""
    await ctx.send(
        'Welcome to the File Upload Bot! You can send me files and I can also send files to you. '
        'Try saying "send file" to get a file from me.'
    )

@app.on_invoke
async def handle_invoke(ctx: ActivityContext[InvokeActivity]) -> InvokeResponse:
    """Handle file consent invoke activities."""
    activity = ctx.activity

    if activity.name != "fileConsent/invoke":
        return InvokeResponse(status=200)

    response = activity.value
    action = get_value(response, "action")
    context = get_value(response, "context")
    filename = get_value(context, "filename")
    file_id = get_value(context, "file_id")

    if action == "accept":
        # Start upload in background task to avoid timeout
        asyncio.create_task(handle_file_upload(ctx, response, filename, file_id))

    elif action == "decline":
        # Clean up cached content if any
        if file_id and file_id in pending_uploads:
            del pending_uploads[file_id]
        await ctx.send(f"Declined. We won't upload file <b>{filename}</b>.")

    return InvokeResponse(status=200)


async def handle_file_upload(ctx: ActivityContext, response, filename: str, file_id: str = None) -> None:
    """Handle file upload to OneDrive in background."""
    try:
        upload_info = get_value(response, "uploadInfo", "upload_info")
        upload_url = get_value(upload_info, "uploadUrl", "upload_url")

        # Upload from cache or local file
        if file_id and file_id in pending_uploads:
            content = pending_uploads[file_id]["content"]
            await upload_content(upload_url, content)
            del pending_uploads[file_id]  # Clean up cache
        else:
            # Local file (default file)
            file_path = os.path.join(FILES_DIR, filename)
            await upload_file_from_disk(upload_url, file_path)

        # Send file info card
        attachment = Attachment(
            content={
                "uniqueId": get_value(upload_info, "uniqueId", "unique_id"),
                "fileType": get_value(upload_info, "fileType", "file_type")
            },
            content_type="application/vnd.microsoft.teams.card.file.info",
            name=get_value(upload_info, "name"),
            content_url=get_value(upload_info, "contentUrl", "content_url")
        )

        await ctx.send(MessageActivityInput(
            text=f"<b>File uploaded.</b> Your file <b>{get_value(upload_info, 'name')}</b> is ready to download",
            attachments=[attachment]
        ))
    except Exception as e:
        # Clean up cache on failure
        if file_id and file_id in pending_uploads:
            del pending_uploads[file_id]
        await ctx.send(f"<b>File upload failed.</b> Error: <pre>{e}</pre>")


@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]) -> None:
    """Handle incoming messages."""
    attachments = ctx.activity.attachments or []

    # Handle file attachments
    if attachments:
        attachment = attachments[0]
        content_type = attachment.content_type or ""

        if content_type == "application/vnd.microsoft.teams.file.download.info":
            await handle_file_download(ctx, attachment)
            return

        # Handle any inline attachment (images, gifs, videos, etc.)
        if attachment.content_url:
            await process_inline_attachment(ctx, attachment)
            return

    # Any message triggers file upload
    await send_file_card(ctx, DEFAULT_FILE_NAME)

async def handle_file_download(ctx: ActivityContext, attachment) -> None:
    """Handle file sent by user and request consent to upload to OneDrive."""
    try:
        download_url = attachment.content.get("downloadUrl")
        filename = attachment.name

        # Download content to memory
        content = await download_content(download_url)
        file_id = str(uuid.uuid4())
        pending_uploads[file_id] = {"content": content, "size": len(content)}

        await ctx.send(f"Received <b>{filename}</b>. Requesting permission to save to your OneDrive...")
        await send_file_card(ctx, filename, file_id)
    except Exception as e:
        await ctx.send(f"<b>File processing failed.</b> Error: <pre>{e}</pre>")


async def process_inline_attachment(ctx: ActivityContext, attachment) -> None:
    """Handle inline attachment and request consent to upload to OneDrive."""
    try:
        # Determine filename with proper extension
        extension = None
        
        # Try to get extension from attachment name first
        if attachment.name and "." in attachment.name:
            extension = attachment.name.split(".")[-1]
        
        # Try to get extension from content_url if name didn't work
        if not extension and attachment.content_url:
            url_path = attachment.content_url.split("?")[0]
            if "." in url_path.split("/")[-1]:
                extension = url_path.split(".")[-1]
        
        # Try to get extension from content_type
        if not extension:
            content_type = attachment.content_type or ""
            if "/" in content_type:
                main_type, sub_type = content_type.split("/", 1)
                type_defaults = {
                    "image": "png",
                    "video": "mp4",
                    "audio": "mp3",
                    "text": "txt",
                    "application": "bin"
                }
                invalid_chars = ['*', '?', '<', '>', '|', ':', '"', '\\', '/']
                if sub_type and not any(c in sub_type for c in invalid_chars):
                    extension = sub_type
                else:
                    extension = type_defaults.get(main_type, "bin")
        
        if not extension:
            extension = "bin"
        
        filename = f"Attachment_{uuid.uuid4().hex[:8]}.{extension}"

        # Download content to memory
        content = await download_content(attachment.content_url)
        file_id = str(uuid.uuid4())
        pending_uploads[file_id] = {"content": content, "size": len(content)}

        await ctx.send(f"Received <b>{filename}</b> ({len(content)} bytes). Requesting permission to save to your OneDrive...")
        await send_file_card(ctx, filename, file_id)
    except Exception as e:
        await ctx.send(f"<b>Error processing attachment.</b> Error: <pre>{e}</pre>")


if __name__ == "__main__":
    asyncio.run(app.start())


if __name__ == "__main__":
    asyncio.run(app.start())
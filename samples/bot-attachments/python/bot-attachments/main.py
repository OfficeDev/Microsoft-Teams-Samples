"""
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
"""

import asyncio
import re
import uuid

from dotenv import load_dotenv
from microsoft_teams.api import (
    Attachment,
    MessageActivity,
    MessageActivityInput,
    FileConsentInvokeActivity,
    FileConsentCard,
    FileInfoCard,
    FileUploadInfo
)
from microsoft_teams.apps import ActivityContext, App
from microsoft_teams.common.http.client import Client, ClientOptions

load_dotenv()

CONTENT_TYPE_FILE_DOWNLOAD = "application/vnd.microsoft.teams.file.download.info"
CONTENT_TYPE_FILE_CONSENT = "application/vnd.microsoft.teams.card.file.consent"
CONTENT_TYPE_FILE_INFO = "application/vnd.microsoft.teams.card.file.info"

app = App()

pending_uploads: dict[str, bytes] = {}

@app.on_file_consent
async def handle_file_consent(ctx: ActivityContext[FileConsentInvokeActivity]) -> None:
    value = ctx.activity.value
    context = value.context or {}
    filename = context.get("filename")
    file_id = context.get("file_id")

    if value.action == "accept":
        await ctx.send(f"Accepted. Uploading <b>{filename}</b>...")
        asyncio.create_task(_handle_file_upload(ctx, value.upload_info, file_id))
    elif value.action == "decline":
        pending_uploads.pop(file_id, None)
        await ctx.send(f"Declined. We won't upload file <b>{filename}</b>.")

@app.on_message
async def handle_message(ctx: ActivityContext[MessageActivity]) -> None:
    content = await _get_attachment_content(ctx)
    if content is None:
        await ctx.send("Welcome to the Bot Attachments sample! Please attach a file or image to save to your OneDrive!")
        return

    attachment = next(iter(ctx.activity.attachments or []))
    filename = attachment.name or f"image_{uuid.uuid4()}.png"
    file_id = str(uuid.uuid4())
    pending_uploads[file_id] = content
    await ctx.send(f"Received <b>{filename}</b>. Requesting permission to save to your OneDrive...")
    await _send_file_consent_card(ctx, filename, file_id)

async def _get_attachment_content(ctx: ActivityContext[MessageActivity]) -> bytes | None:
    attachment = next(iter(ctx.activity.attachments or []), None)
    if attachment is None:
        return None

    if attachment.content_type == CONTENT_TYPE_FILE_DOWNLOAD:
        try:
            return await _download_attachment(attachment.content.get("downloadUrl"))
        except Exception as e:
            print(f"Failed to download attachment: {e}")
            return None

    return None

async def _download_attachment(url: str) -> bytes:
    response = await Client(ClientOptions()).get(url)
    if response.status_code == 200:
        return response.content
    raise Exception(f"Download failed with status {response.status_code}")


async def _upload_to_onedrive(url: str, content: bytes) -> None:
    file_size = len(content)
    client = Client(ClientOptions(headers={
        "Content-Type": "application/octet-stream",
        "Content-Length": str(file_size),
        "Content-Range": f"bytes 0-{file_size - 1}/{file_size}",
    }))
    response = await client.put(url, content=content)
    if response.status_code not in [200, 201]:
        raise Exception(f"Upload failed with status {response.status_code}")


async def _send_file_consent_card(ctx: ActivityContext, filename: str, file_id: str) -> None:
    consent_context = {"filename": filename, "file_id": file_id}
    await ctx.send(MessageActivityInput(attachments=[Attachment(
        content=FileConsentCard(
            description="This is the file I want to send you",
            size_in_bytes=len(pending_uploads[file_id]),
            accept_context=consent_context,
            decline_context=consent_context,
        ),
        content_type=CONTENT_TYPE_FILE_CONSENT,
        name=filename,
    )]))


async def _handle_file_upload(ctx: ActivityContext, upload_info: FileUploadInfo, file_id: str) -> None:
    try:
        content = pending_uploads.pop(file_id)
        await _upload_to_onedrive(upload_info.upload_url, content)
        await ctx.send(MessageActivityInput(
            text=f"<b>{upload_info.name}</b> has been successfully uploaded.",
            attachments=[Attachment(
                content=FileInfoCard(
                    unique_id=upload_info.unique_id,
                    file_type=upload_info.file_type,
                ),
                content_type=CONTENT_TYPE_FILE_INFO,
                name=upload_info.name,
                content_url=upload_info.content_url,
            )],
        ))
    except Exception as e:
        pending_uploads.pop(file_id, None)
        print(f"File upload failed: {e}")

if __name__ == "__main__":
    asyncio.run(app.start())

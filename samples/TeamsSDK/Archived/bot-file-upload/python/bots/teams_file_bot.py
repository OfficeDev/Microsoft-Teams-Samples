# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
import os
import aiohttp
from botbuilder.core import TurnContext
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema import Attachment
from botbuilder.schema.teams import (
    FileDownloadInfo,
    FileConsentCard,
    FileConsentCardResponse,
    FileInfoCard,
)
from botbuilder.schema.teams.additional_properties import ContentType


class TeamsFileUploadBot(TeamsActivityHandler):

    FILE_DIRECTORY = "files/"
    DEFAULT_FILE_NAME = "teams-logo.png"

    async def on_message_activity(self, turn_context: TurnContext):
        if not turn_context.activity.attachments:
            return

        attachment = turn_context.activity.attachments[0]

        if attachment.content_type == ContentType.FILE_DOWNLOAD_INFO:
            await self._download_file(turn_context, attachment)
        else:
            await self._send_file_card(turn_context, self.DEFAULT_FILE_NAME)

    async def _download_file(self, turn_context: TurnContext, attachment: Attachment):
        """Handles file download from Teams."""
        file_download = FileDownloadInfo.deserialize(attachment.content)
        file_path = os.path.join(self.FILE_DIRECTORY, attachment.name)

        os.makedirs(self.FILE_DIRECTORY, exist_ok=True)

        async with aiohttp.ClientSession() as session:
            async with session.get(file_download.download_url) as response:
                if response.status == 200:
                    with open(file_path, "wb") as f:
                        f.write(await response.read())
                    await self._send_download_complete(turn_context, attachment.name)
                else:
                    await self._file_upload_failed(turn_context, "Download failed.")

    async def _send_download_complete(self, turn_context: TurnContext, filename: str):
        """Send a confirmation message after a successful file download."""
        reply = turn_context.activity.create_reply(
            f"Completed downloading <b>{filename}</b>"
        )
        reply.text_format = "xml"
        await turn_context.send_activity(reply)

    async def _send_file_card(self, turn_context: TurnContext, filename: str):
        """Send a FileConsentCard to get user consent to upload a file."""
        file_path = os.path.join(self.FILE_DIRECTORY, filename)
        if not os.path.exists(file_path):
            await self._file_upload_failed(turn_context, "File not found.")
            return

        file_size = os.path.getsize(file_path)
        consent_context = {"filename": filename}

        file_card = FileConsentCard(
            description="This is the file I want to send you",
            size_in_bytes=file_size,
            accept_context=consent_context,
            decline_context=consent_context,
        )

        attachment = Attachment(
            content=file_card.serialize(),
            content_type=ContentType.FILE_CONSENT_CARD,
            name=filename,
        )

        reply = turn_context.activity.create_reply()
        reply.attachments = [attachment]
        await turn_context.send_activity(reply)

    async def on_teams_file_consent_accept(
        self,
        turn_context: TurnContext,
        file_consent_card_response: FileConsentCardResponse,
    ):
        """Handles file upload when the user accepts the file consent."""
        await self._upload_file(turn_context, file_consent_card_response)

    async def _upload_file(
        self,
        turn_context: TurnContext,
        file_consent_card_response: FileConsentCardResponse,
    ):
        """Uploads the file to Teams."""
        file_path = os.path.join(
            self.FILE_DIRECTORY, file_consent_card_response.context["filename"]
        )
        file_size = os.path.getsize(file_path)

        headers = {
            "Content-Length": str(file_size),
            "Content-Range": f"bytes 0-{file_size - 1}/{file_size}",
        }

        with open(file_path, "rb") as f:
            file_data = f.read()

        async with aiohttp.ClientSession() as session:
            async with session.put(
                file_consent_card_response.upload_info.upload_url,
                data=file_data,
                headers=headers,
            ) as response:
                if response.status in (200, 201):
                    await self._file_upload_complete(
                        turn_context, file_consent_card_response
                    )
                else:
                    await self._file_upload_failed(
                        turn_context, "Unable to upload file."
                    )

    async def on_teams_file_consent_decline(
        self,
        turn_context: TurnContext,
        file_consent_card_response: FileConsentCardResponse,
    ):
        """Handles file upload when the user declines the file consent."""
        filename = file_consent_card_response.context["filename"]
        reply = turn_context.activity.create_reply(
            f"Declined. We won't upload file <b>{filename}</b>."
        )
        reply.text_format = "xml"
        await turn_context.send_activity(reply)

    async def _file_upload_complete(
        self,
        turn_context: TurnContext,
        file_consent_card_response: FileConsentCardResponse,
    ):
        """Handles successful file upload."""
        upload_info = file_consent_card_response.upload_info
        download_card = FileInfoCard(
            unique_id=upload_info.unique_id, file_type=upload_info.file_type
        )

        attachment = Attachment(
            content=download_card.serialize(),
            content_type=ContentType.FILE_INFO_CARD,
            name=upload_info.name,
            content_url=upload_info.content_url,
        )

        reply = turn_context.activity.create_reply(
            f"<b>File uploaded.</b> Your file <b>{upload_info.name}</b> is ready to download"
        )
        reply.text_format = "xml"
        reply.attachments = [attachment]
        await turn_context.send_activity(reply)

    async def _file_upload_failed(self, turn_context: TurnContext, error: str):
        """Handles file upload failure."""
        reply = turn_context.activity.create_reply(
            f"<b>File upload failed.</b> Error: <pre>{error}</pre>"
        )
        reply.text_format = "xml"
        await turn_context.send_activity(reply)

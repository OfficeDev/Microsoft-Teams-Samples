import json
import os
import time
from botbuilder.core import TurnContext, MessageFactory
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema import Activity, ActivityTypes, Attachment, Entity
from openai import AzureOpenAI
from models.streaming import StreamType, ChannelData

class TeamsConversationBot(TeamsActivityHandler):
    def __init__(self, config):
        # Initialize configuration settings
        self._app_id = config.APP_ID
        self._app_password = config.APP_PASSWORD
        self._app_tenant_id = config.APP_TENANT_ID
        self._endpoint = config.AZURE_OPENAI_ENDPOINT
        self._key = config.AZURE_OPENAI_KEY
        self._deployment = config.AZURE_OPENAI_DEPLOYMENT

        self._client = AzureOpenAI(
            api_key=self._key,
            api_version="2024-07-01-preview",
            azure_endpoint=self._endpoint
        )
        self.adaptive_card_template = "./Resources/CardTemplate.json"

    async def on_message_activity(self, turn_context: TurnContext):
        user_input = turn_context.activity.text.strip().lower()
        content_builder = []
        stream_sequence = 1
        rps = 1000  # Rate per second limit
        start_time = time.time()
        temperature=0.7
        frequency_penalty=0
        presence_penalty=0

        try:
            # Initial informative message
            channel_data = ChannelData(
                streamType=StreamType.INFORMATIVE.value,
                streamSequence=stream_sequence
            )
            stream_id = await self.build_and_send_streaming_activity(
                turn_context, "Getting the information...", channel_data
            )

            # Prepare messages for the chat completion request
            messages = [
                {"role": "system", "content": "You are an AI great at storytelling."},
                {"role": "user", "content": user_input},
            ]

            # Send request to chat client with streaming enabled
            chat_response = self._client.chat.completions.create(
                model=self._deployment,
                messages=messages,
                temperature=temperature,
                frequency_penalty=frequency_penalty,
                presence_penalty=presence_penalty,
                stream=True  # Set stream=True to get a streaming response
            )

            # Debug: Check response structure
            print(f"Chat response started: {chat_response}")

            # Use a synchronous for loop to read the chunks from the response stream
            for chunk in chat_response:
                stream_sequence = stream_sequence + 1
                # Check if the chunk has valid choices
                if len(chunk.choices) > 0:
                    choice_delta = chunk.choices[0].delta  # Assuming one choice
                    delta_content = choice_delta.content

                    # Handle the finish reason for the final chunk
                    if chunk.choices[0].finish_reason != None:
                        finish_reason = chunk.choices[0].finish_reason
                        if finish_reason:
                            channel_data = ChannelData(
                                streamType=StreamType.FINAL.value,
                                streamSequence=stream_sequence,
                                streamId=stream_id,
                            )
                            await self.build_and_send_streaming_activity(
                                turn_context, "".join(content_builder), channel_data
                            )
                            break

                    # Append and send content incrementally
                    if delta_content:
                        content_builder.append(delta_content)
                        if content_builder and (time.time() - start_time > 1 / rps):
                            channel_data = ChannelData(
                                streamType=StreamType.STREAMING.value,
                                streamSequence=stream_sequence,
                                streamId=stream_id,
                            )
                            await self.build_and_send_streaming_activity(
                                turn_context, "".join(content_builder), channel_data
                            )
                            start_time = time.time()  # Reset time
                else:
                    # Handle case where 'choices' is missing or empty
                    print(f"Warning: 'choices' is empty or missing in chunk: {chunk}")
                    # Add logic to either retry or stop if empty chunks persist

        except Exception as ex:
            await turn_context.send_activity(MessageFactory.text(str(ex)))


    async def build_and_send_streaming_activity(self, turn_context: TurnContext, text: str, channel_data):
        is_stream_final = channel_data.streamType == StreamType.FINAL.value
        channel_data_dict = {
            "streamId": channel_data.streamId,
            "streamType": channel_data.streamType,
            "streamSequence": channel_data.streamSequence
        }
        streaming_activity = Activity(
            type=ActivityTypes.message if is_stream_final else ActivityTypes.typing,
            id=channel_data.streamId,
            channel_data=channel_data_dict
        )

        streaming_activity.entities = [{
            "type": "streaminfo",
            "streamId": channel_data.streamId,
            "streamType": channel_data.streamType,
            "streamSequence": channel_data.streamSequence
        }]

        if text:
            streaming_activity.text = text

        # For the final stream, add an Adaptive Card attachment
        if is_stream_final:
            # Build the adaptive card
            with open(self.adaptive_card_template) as template_file:
                adaptive_card_template = json.load(template_file)
            adaptive_card = adaptive_card_template.copy()
            adaptive_card["body"][0]["text"] = text
            attachment = Attachment(
                content_type="application/vnd.microsoft.card.adaptive",
                content=adaptive_card
            )

            streaming_activity.attachments = [attachment]
            streaming_activity.text = "This is what I've got:"

        # Send the streaming activity
        return await self.send_streaming_activity_async(turn_context, streaming_activity)


    # Helper function to send streaming activity
    async def send_streaming_activity_async(self, turn_context: TurnContext, streaming_activity):
        try:
            activity_dict = streaming_activity.__dict__
            print(json.dumps(activity_dict, indent=4))
            streaming_response = await turn_context.send_activity(streaming_activity)
            return streaming_response.id
        except Exception as ex:
            error_message = f"Error while sending streaming activity: {str(ex)}"
            await turn_context.send_activity(MessageFactory.text(error_message))
            raise Exception(error_message)
        
    async def on_installation_update(self, turn_context: TurnContext):
        if turn_context.activity.conversation.conversation_type == "channel":
            await turn_context.send_activity(
                MessageFactory.text(
                    f"Welcome to the streaming bot. The streaming feature is not available for channels yet."
                )
            )
        else:
            await turn_context.send_activity(
                MessageFactory.text(
                    "Welcome! You can ask a question, and I'll stream the response."
                )
            )
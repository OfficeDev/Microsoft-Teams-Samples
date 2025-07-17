# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
import re
from botbuilder.core import TurnContext
from botbuilder.core.teams import TeamsActivityHandler
from openai import OpenAI
from botbuilder.schema import Attachment
from botbuilder.schema.teams import (
    TaskModuleResponse,
    TaskModuleMessageResponse,
    TaskModuleContinueResponse,
    TaskModuleTaskInfo,
    MessagingExtensionAction,
)

class SentimentAnalysis(TeamsActivityHandler):
    # Initializes the SentimentAnalysis bot class and sets the base URL from environment variables
    def __init__(self):
        super(SentimentAnalysis, self).__init__()
        self.base_url = os.getenv("BaseUrl")

    # Welcomes new users who join the conversation
    async def on_members_added_activity(self, members_added, turn_context: TurnContext):
        for member in members_added:
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity(
                    "Welcome to the Sentiment Analysis Sample. This example demonstrates how to analyze selected text and classify its sentiment as Positive, Neutral, or Negative."
                )

    # Handles fetching the task module for messaging extension and returns a sentiment analysis result card
    async def on_teams_messaging_extension_fetch_task(
        self, turn_context: TurnContext, action: MessagingExtensionAction
    ) -> TaskModuleResponse:
        # Extract and clean the selected message content
        message_body = action.message_payload.body.content
        text_to_analyze = re.sub(r"<[^>]+>", "", message_body)
        print("Text to analyze:", text_to_analyze)

        sentiment_response = "Not available"

        try:
            # Initialize OpenAI client with API key
            openai = OpenAI(api_key=os.getenv("SECRET_OPENAI_API_KEY"))

            # Call OpenAI chat completion API to analyze sentiment
            response = openai.chat.completions.create(
                model=os.getenv("CHAT_COMPLETION_MODEL_NAME", "gpt-3.5-turbo"),
                messages=[
                    {
                        "role": "system",
                        "content": "You will be provided with a message, and your task is to classify its sentiment as Positive, Neutral, or Negative. Only respond with one of these three words.",
                    },
                    {"role": "user", "content": text_to_analyze},
                ],
                temperature=0,
            )

            # Extract sentiment from OpenAI response
            sentiment_response = response.choices[0].message.content.strip()
            # sentiment_response = "Positive Negative"
            print("Sentiment result:", sentiment_response)

        except Exception as e:
            # Print error if OpenAI call fails
            print("OpenAI error:", str(e))

        # Define adaptive card content with title, message, and sentiment
        adaptive_card_content = {
            "type": "AdaptiveCard",
            "version": "1.3",
            "body": [
                {
                    "type": "Container",
                    "items": [
                        {
                            "type": "TextBlock",
                            "text": "Sentiment Analysis",
                            "weight": "Bolder",
                            "size": "Large",
                            "horizontalAlignment": "Center",
                            "spacing": "Medium",
                        },
                        {
                            "type": "TextBlock",
                            "text": "This analysis evaluates the sentiment of the selected message.",
                            "wrap": True,
                            "horizontalAlignment": "Center",
                            "spacing": "Small",
                            "isSubtle": True,
                        },
                    ],
                    "style": "emphasis",
                    "bleed": True,
                },
                {
                    "type": "Container",
                    "spacing": "Large",
                    "items": [
                        {
                            "type": "TextBlock",
                            "text": "Original Message:",
                            "weight": "Bolder",
                            "wrap": True,
                            "spacing": "Medium",
                        },
                        {
                            "type": "TextBlock",
                            "text": text_to_analyze,
                            "wrap": True,
                            "spacing": "Small",
                            "separator": True,
                        },
                        {
                            "type": "TextBlock",
                            "text": "Sentiment:",
                            "weight": "Bolder",
                            "wrap": True,
                            "spacing": "Medium",
                        },
                        {
                            "type": "TextBlock",
                            "text": sentiment_response,
                            "wrap": True,
                            "spacing": "Small",
                            "separator": True,
                            "color": (
                                "Good"
                                if "positive" in sentiment_response.lower()
                                else "Attention"
                            ),
                        },
                    ],
                },
            ],
            "actions": [],
        }

        # Create attachment with the adaptive card content
        card_attachment = Attachment(
            content_type="application/vnd.microsoft.card.adaptive",
            content=adaptive_card_content,
        )

        # Return adaptive card inside a task module popup
        return TaskModuleResponse(
            task=TaskModuleContinueResponse(
                value=TaskModuleTaskInfo(
                    card=card_attachment,
                    title="Sentiment Analysis",
                    width=500,
                    height=400,
                )
            )
        )

    # Ensures the base class run method is called for handling activities
    async def run(self, turn_context: TurnContext):
        await super().run(turn_context)

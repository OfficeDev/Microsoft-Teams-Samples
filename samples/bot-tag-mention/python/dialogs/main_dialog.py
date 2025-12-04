# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import MessageFactory
from botbuilder.dialogs import (
    WaterfallDialog,
    WaterfallStepContext,
    DialogTurnResult,
    PromptOptions,
)
from botbuilder.dialogs.prompts import OAuthPrompt, OAuthPromptSettings, ConfirmPrompt

from .logout_dialog import LogoutDialog
from botbuilder.core import CardFactory, TurnContext
from botbuilder.core.teams.teams_info import TeamsInfo
from simple_graph_client import SimpleGraphClient
import logging

logging.basicConfig(level=logging.INFO)

class MainDialog(LogoutDialog):
    def __init__(self, connection_name: str):
        # Initializes the MainDialog with OAuthPrompt, ConfirmPrompt, and WaterFallDialog.
        super(MainDialog, self).__init__(MainDialog.__name__, connection_name)

        self.add_dialog(
            OAuthPrompt(
                OAuthPrompt.__name__,
                OAuthPromptSettings(
                    connection_name=connection_name,
                    title="Please Sign In",
                    timeout=300000
                )
            )
        )

        self.add_dialog(ConfirmPrompt(ConfirmPrompt.__name__))

        self.add_dialog(
            WaterfallDialog(
                "WFDialog",
                [
                    self.prompt_step,
                    self.login_step,
                    self.mention_tag,
                ],
            )
        )

        self.initial_dialog_id = "WFDialog"

    # Prompts the user to sign in using the OAuthPrompt.
    async def prompt_step(self, step_context):
        logging.info("Reached OAuthPrompt step - prompting for login.")
        return await step_context.begin_dialog(OAuthPrompt.__name__)
    
    # Handles the login result and moves to tag mention step.
    async def login_step(self, step_context):
        if step_context.result and step_context.result.token:
            logging.info("Login Step Reached — Successfully Logged In")
            if step_context.context.activity.conversation.conversation_type == 'personal':
                await step_context.context.send_activity(
                    "You are logged in. Please install the app in the team scope to use the Tag mention functionality."
                )
            return await step_context.next(step_context.result)  # Pass token to next step

        # Handles invalid actions or cancelled login
        await step_context.context.send_activity(
            "Login was not successful or was cancelled. Let's try again."
        )
        return await step_context.replace_dialog(self.initial_dialog_id)

    # Sends tag mention adaptive card.
    async def tagMentionAdaptiveCard(self, step_context: WaterfallStepContext, tagName: str, tagId: str) -> DialogTurnResult:
        adaptiveCardAttachment = {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.5",
            "speak": f"This card mentions a tag: {tagName}",
            "body": [
                {
                    "type": "TextBlock",
                    "text": f"Mention a Tag: <at>{tagName}</at>"
                }
            ],
            "msteams": {
                "entities": [
                    {
                        "type": "mention",
                        "text": f"<at>{tagName}</at>",
                        "mentioned": {
                            "id": tagId,
                            "name": tagName,
                            "type": "tag"
                        }
                    }
                ]
            }
        }
        await step_context.context.send_activity(
            MessageFactory.attachment(CardFactory.adaptive_card(adaptiveCardAttachment))
        )
        return await step_context.end_dialog()
    

    # Method to invoke Tag mention functionality flow.
    async def mention_tag(self, step_context):
        try:
            tokenResponse = step_context.result
            if step_context.context.activity.conversation.conversation_type == 'personal' and tokenResponse and tokenResponse.token:
               await step_context.context.send_activity('You have successfully logged in. Please install the app in the team scope to use the Tag mention functionality.') 
            else:
                tagExists = False
                if tokenResponse and tokenResponse.token:
                    TurnContext.remove_recipient_mention(step_context.context.activity)
                    activityText = step_context.context.activity.text.strip()
                    if '<at>' in activityText:
                        tagName = activityText.replace('<at>', '').replace('</at>', '').strip()
                        if len(step_context.context.activity.entities) > 1:
                            tagID = step_context.context.activity.entities[1].additional_properties.get('mentioned', {}).get('id')
                            await self.tagMentionAdaptiveCard(step_context, tagName, tagID)
                        else:
                            await step_context.context.send_activity("Could not find tag information in the message.")
                    elif activityText != '':
                        try:
                            # Fetch data from Microsoft Graph.
                            client = SimpleGraphClient(tokenResponse.token)
                            teamDetails = await TeamsInfo.get_team_details(step_context.context)
                            result = await client.get_Tag(teamDetails.aad_group_id)
                            
                            for tagDetails in result.get('value', []):
                                if tagDetails.get('displayName') == activityText:
                                    tagExists = True
                                    await self.tagMentionAdaptiveCard(step_context, tagDetails.get('displayName'), tagDetails.get('id'))
                                    break
                                    
                            if not tagExists:
                                await step_context.context.send_activity(
                                    "Provided tag name is not available in this team. Please try with another tag name or create a new tag."
                                )
                        except Exception as e:
                            logging.error(f"Error fetching team details: {e}")
                            await step_context.context.send_activity("You don't have Graph API permissions to fetch tag's information. Please use this command to mention a tag: '`@<Bot-name>  @<your-tag>`' to experience tag mention using the bot.")
                    else:
                        await step_context.context.send_activity(
                            'Please provide a tag name while mentioning the bot as "`@<Bot-name> <your-tag-name>`" or mention a tag as "`@<Bot-name> @<your-tag>`"'
                        )
                else:
                    await step_context.context.send_activity(
                        'Response token is null or empty.'
                    )
        except Exception as e:
            logging.error(f'Error occurred while processing your request: {e}')
            await step_context.context.send_activity('An error occurred while processing your request.')

        return await step_context.end_dialog()


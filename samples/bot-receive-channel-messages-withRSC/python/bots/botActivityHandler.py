# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import MessageFactory, TurnContext
from botbuilder.schema.teams import TeamInfo, TeamsChannelAccount
from botbuilder.core.teams import TeamsActivityHandler

# This class represents a bot that handles message activities and reactions in Microsoft Teams.
class BotActivityHandler(TeamsActivityHandler):
    def __init__(self):
        super().__init__()  # Call the parent class constructor
        
        # Sample bot messages and descriptions
        self.sampleDescription = "With this sample your bot can receive user messages across standard channels in a team without being @mentioned"
        self.option = "Type 1 to know about the permissions required,  Type 2 for documentation link"
        self.permissionRequired = "This capability is enabled by specifying the ChannelMessage.Read.Group permission in the manifest of an RSC enabled Teams app"
        self.docLink = "https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/channel-messages-with-rsc"

    # Event handler for when new members are added to a team
    async def on_teams_members_added(
        self,
        teams_members_added: list[TeamsChannelAccount],  # List of new members added
        team_info: TeamInfo,  # Information about the team
        turn_context: TurnContext,  # Context for the current turn
    ):
        # Welcome message for new members
        welcome_text = "Hello and welcome! With this sample, your bot can receive user messages across standard channels in a team without being @mentioned."
        
        for member in teams_members_added:
            # Ensure the bot does not send a welcome message to itself
            if member.id != turn_context.activity.recipient.id:
                await turn_context.send_activity(MessageFactory.text(welcome_text))

    # Event handler for when a message is received
    async def on_message_activity(self, turn_context: TurnContext):
        # Get the text from the incoming message
        reply_text = turn_context.activity.text

        # Respond based on user input
        if reply_text == "1":
            # Provide information about required permissions
            await turn_context.send_activity(
                MessageFactory.text(self.permissionRequired)
            )
        elif reply_text == "2":
            # Provide the documentation link
            await turn_context.send_activity(MessageFactory.text(self.docLink))
        else:
            # Send the default sample description and options for further interaction
            await turn_context.send_activity(
                MessageFactory.text(self.sampleDescription)
            )
            await turn_context.send_activity(MessageFactory.text(self.option))

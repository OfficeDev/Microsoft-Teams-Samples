from botbuilder.core import TeamsActivityHandler, ConversationState, UserState, TurnContext
from botbuilder.dialogs import Dialog

class DialogBot(TeamsActivityHandler):
    """
    A bot that handles dialogs using the TeamsActivityHandler class.
    """

    def __init__(self, conversation_state: ConversationState, user_state: UserState, dialog: Dialog):
        super().__init__()
        if not conversation_state:
            raise ValueError("[DialogBot]: Missing parameter. conversation_state is required")
        if not user_state:
            raise ValueError("[DialogBot]: Missing parameter. user_state is required")
        if not dialog:
            raise ValueError("[DialogBot]: Missing parameter. dialog is required")

        self.conversation_state = conversation_state
        self.user_state = user_state
        self.dialog = dialog
        self.dialog_state = self.conversation_state.create_property("DialogState")

    async def on_message_activity(self, turn_context: TurnContext):
        """
        Handles the on_message activity event.
        """
        print("Running dialog with Message Activity.")

        # Run the Dialog with the new message Activity.
        await self.dialog.run(turn_context, self.dialog_state)

    async def run(self, turn_context: TurnContext):
        """
        Override the ActivityHandler.run() method to save state changes after the bot logic completes.
        """
        await super().run(turn_context)

        # Save any state changes. The load happened during the execution of the Dialog.
        await self.conversation_state.save_changes(turn_context, False)
        await self.user_state.save_changes(turn_context, False)

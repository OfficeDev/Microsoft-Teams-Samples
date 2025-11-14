from botbuilder.core import MessageFactory
from botbuilder.core.teams import TeamsActivityHandler
from botbuilder.schema import ChannelAccount, Activity, ActivityTypes
from botbuilder.schema.teams import (
    TaskModuleRequest,
    TaskModuleResponse,
    TaskModuleTaskInfo,
    TaskModuleContinueResponse
)
import os
import sys
from pathlib import Path
from dotenv import load_dotenv

# Load environment variables
project_root = Path(__file__).parent.parent.parent
load_dotenv(dotenv_path=project_root / 'env' / '.env.local')
load_dotenv(dotenv_path=project_root / 'env' / '.env.local.user')

sys.path.append(os.path.dirname(os.path.dirname(__file__)))
from services.store import store
from services.adaptive_card_service import create_adaptive_card
from botbuilder.core import CardFactory

class BotActivityHandler(TeamsActivityHandler):
    def __init__(self):
        super().__init__()

    async def on_conversation_update_activity(self, turn_context):
        try:
            store.set_item("conversationId", turn_context.activity.conversation.id)
            store.set_item("serviceUrl", turn_context.activity.service_url)
        except Exception as e:
            print(f"Error in on_conversation_update_activity: {str(e)}")
        await super().on_conversation_update_activity(turn_context)

    async def on_members_added_activity(self, members_added, turn_context):
        try:
            store.set_item("conversationId", turn_context.activity.conversation.id)
            store.set_item("serviceUrl", turn_context.activity.service_url)
        except Exception as e:
            print(f"Error in on_members_added_activity: {str(e)}")

    async def on_message_activity(self, turn_context):
        try:
            if not turn_context.activity.value:
                await turn_context.send_activity(
                    MessageFactory.text(f"Bot is working! You said: {turn_context.activity.text}")
                )
                return

            user_name = turn_context.activity.from_property.name
            data = turn_context.activity.value

            if data.get('Type') == 'task/fetch':
                return  # Handled by task/fetch handler

            answer = data.get('Feedback')
            choice_id = data.get('Choice')

            actual_answer, option_position = None, None
            if answer:
                if answer.startswith('option1_'):
                    actual_answer = answer.replace('option1_', '')
                    option_position = 'option1'
                elif answer.startswith('option2_'):
                    actual_answer = answer.replace('option2_', '')
                    option_position = 'option2'
                else:
                    actual_answer = answer

            if not actual_answer:
                await turn_context.send_activity(MessageFactory.text("Please select an option before submitting your vote."))
                return
            if not choice_id:
                await turn_context.send_activity(MessageFactory.text("Invalid poll response - missing poll ID."))
                return

            task_info_list = store.get_item("agendaList")
            if not task_info_list:
                await turn_context.send_activity(MessageFactory.text("No polls available."))
                return

            task_info = next((x for x in task_info_list if x.get('Id') == choice_id), None)
            if not task_info:
                await turn_context.send_activity(MessageFactory.text("Poll not found."))
                return

            if not option_position:
                if actual_answer == task_info.get('option1'):
                    option_position = 'option1'
                elif actual_answer == task_info.get('option2'):
                    option_position = 'option2'

            if not option_position:
                await turn_context.send_activity(MessageFactory.text("Could not determine selected option."))
                return

            person_answered = task_info.get('personAnswered') or {}
            if task_info['option1'] == task_info['option2']:
                vote_key = f"{actual_answer}_{option_position}"
            else:
                vote_key = actual_answer

            if vote_key in person_answered:
                if isinstance(person_answered[vote_key], list):
                    person_answered[vote_key].append(user_name)
                else:
                    person_answered[vote_key] = [person_answered[vote_key], user_name]
            else:
                person_answered[vote_key] = [user_name]

            task_info['personAnswered'] = person_answered
            store.set_item("agendaList", task_info_list)

            if task_info['option1'] == task_info['option2']:
                option1_key = f"{task_info['option1']}_option1"
                option2_key = f"{task_info['option2']}_option2"
                option1_answered = len(person_answered.get(option1_key, []))
                option2_answered = len(person_answered.get(option2_key, []))
                if option1_answered == 0 and option2_answered == 0:
                    legacy_votes = person_answered.get(task_info['option1'], [])
                    total_legacy = len(legacy_votes)
                    option1_answered = total_legacy // 2
                    option2_answered = total_legacy - option1_answered
            else:
                option1_answered = len(person_answered.get(task_info['option1'], []))
                option2_answered = len(person_answered.get(task_info['option2'], []))

            total = option1_answered + option2_answered
            percent_option1 = 0 if total == 0 else int((option1_answered * 100) / total)
            percent_option2 = 0 if total == 0 else 100 - percent_option1

            conversation_id = turn_context.activity.conversation.id
            service_url = turn_context.activity.service_url

            bot_id = os.getenv('AAD_APP_CLIENT_ID')
            bot_password = (
                os.getenv('MicrosoftAppPassword') or
                os.getenv('BotPassword') or
                os.getenv('SECRET_AAD_APP_CLIENT_SECRET')
            )

            card = create_adaptive_card("Result.json", task_info, percent_option1, percent_option2)

            if bot_id and bot_password:
                from botframework.connector import ConnectorClient
                from botframework.connector.auth import MicrosoftAppCredentials

                credentials = MicrosoftAppCredentials(bot_id, bot_password)
                client = ConnectorClient(credentials, base_url=service_url)
                MicrosoftAppCredentials.trust_service_url(service_url)

                result_activity = Activity(
                    type=ActivityTypes.message,
                    from_property=ChannelAccount(id=bot_id),
                    attachments=[card]
                )
                client.conversations.send_to_conversation(conversation_id, result_activity)
            else:
                await turn_context.send_activity(MessageFactory.attachment(card))

        except Exception as e:
            print(f"Error in on_message_activity: {str(e)}")
            import traceback
            traceback.print_exc()
            try:
                await turn_context.send_activity(MessageFactory.text("An error occurred processing your message."))
            except:
                pass

    async def on_teams_task_module_fetch(self, turn_context, task_module_request: TaskModuleRequest) -> TaskModuleResponse:
        try:
            request_data = task_module_request.data if task_module_request else {}
            poll_id = request_data.get('Id')
            if not poll_id:
                return None

            # Get the poll data from store
            task_info_list = store.get_item("agendaList") or []
            poll_data = None
            
            for item in task_info_list:
                if item.get('Id') == poll_id:
                    poll_data = item
                    break
            
            if not poll_data:
                # If not found in main list, check conversations
                conversations = store.get_item("conversations") or {}
                for conv_data in conversations.values():
                    agenda_list = conv_data.get("agendaList", [])
                    for item in agenda_list:
                        if item.get('Id') == poll_id:
                            poll_data = item
                            break
                    if poll_data:
                        break
            
            if not poll_data:
                return None

            # Calculate percentages for the modal
            person_answered = poll_data.get('personAnswered', {})
            option1 = poll_data.get('option1', 'Option 1')
            option2 = poll_data.get('option2', 'Option 2')
            
            # Count votes for each option
            option1_votes = 0
            option2_votes = 0
            
            for key, voters in person_answered.items():
                if isinstance(voters, list):
                    vote_count = len(voters)
                    if key == option1 or key.endswith('_option1'):
                        option1_votes += vote_count
                    elif key == option2 or key.endswith('_option2'):
                        option2_votes += vote_count

            total_votes = option1_votes + option2_votes
            percent_option1 = 0 if total_votes == 0 else int((option1_votes * 100) / total_votes)
            percent_option2 = 0 if total_votes == 0 else 100 - percent_option1

            # Create adaptive card for the modal (similar to the example code pattern)
            results_card = self.__create_results_card_attachment(poll_data, option1_votes, option2_votes, percent_option1, percent_option2)
            
            # Set up task info with the card (not URL) - following the example pattern
            task_info = TaskModuleTaskInfo()
            task_info.card = results_card
            task_info.height = 400
            task_info.width = 600
            task_info.title = "Poll Results"

            # Return TaskModuleResponse with the card
            continue_response = TaskModuleContinueResponse(value=task_info)
            return TaskModuleResponse(task=continue_response)

        except Exception as e:
            print(f"Error in on_teams_task_module_fetch: {str(e)}")
            import traceback
            traceback.print_exc()
            return None

    def __create_results_card_attachment(self, poll_data, option1_votes, option2_votes, percent_option1, percent_option2):
        """
        Creates an AdaptiveCard attachment for displaying poll results in the modal.
        Similar to the example code's __create_adaptive_card_attachment method.
        
        :param poll_data: The poll data dictionary
        :param option1_votes: Number of votes for option 1
        :param option2_votes: Number of votes for option 2
        :param percent_option1: Percentage for option 1
        :param percent_option2: Percentage for option 2
        :return: An Attachment object containing the AdaptiveCard.
        """
        total_votes = option1_votes + option2_votes
        
        adaptive_card = {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.2",
            "type": "AdaptiveCard",
            "body": [
                {
                    "type": "TextBlock",
                    "text": " Poll Results",
                    "size": "Medium",
                    "weight": "Bolder",
                    "color": "Accent",
                    "horizontalAlignment": "Center"
                },
                {
                    "type": "TextBlock",
                    "text": poll_data.get('title', 'Poll'),
                    "size": "Default",
                    "weight": "Bolder",
                    "wrap": True,
                    "horizontalAlignment": "Center",
                    "spacing": "Medium"
                },
                {
                    "type": "Container",
                    "spacing": "Medium",
                    "items": [
                        {
                            "type": "TextBlock",
                            "text": f" **{poll_data.get('option1', 'Option 1')}**",
                            "weight": "Bolder",
                            "color": "Good"
                        },
                        {
                            "type": "TextBlock",
                            "text": f"{percent_option1}% ({option1_votes} votes)",
                            "color": "Good",
                            "spacing": "Small"
                        },
                        {
                            "type": "TextBlock",
                            "text": f" **{poll_data.get('option2', 'Option 2')}**",
                            "weight": "Bolder",
                            "color": "Attention",
                            "spacing": "Medium"
                        },
                        {
                            "type": "TextBlock",
                            "text": f"{percent_option2}% ({option2_votes} votes)",
                            "color": "Attention",
                            "spacing": "Small"
                        }
                    ]
                },
                {
                    "type": "TextBlock",
                    "text": f"Total Votes: {total_votes}",
                    "size": "Small",
                    "color": "Default",
                    "horizontalAlignment": "Center",
                    "spacing": "Medium"
                }
            ]
        }

        return CardFactory.adaptive_card(adaptive_card)

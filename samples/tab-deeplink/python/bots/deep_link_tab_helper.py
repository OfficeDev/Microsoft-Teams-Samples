import os
import urllib.parse

class DeepLinkTabHelper:
    @staticmethod
    def get_deep_link_tab_channel(sub_entity_id, ID, desc, channel_id, app_id, entity_id):
        task_context = urllib.parse.quote(f'{{"subEntityId": "{sub_entity_id}","channelId":"{channel_id}"}}')
        encoded_web_url = urllib.parse.quote(f'{os.getenv("Base_URL")}/ChannelDeepLink.html&label=DeepLink')

        return {
            "linkUrl": f"https://teams.microsoft.com/l/entity/{app_id}/{entity_id}?webUrl={encoded_web_url}&context={task_context}",
            "ID": ID,
            "TaskText": desc
        }

    @staticmethod
    def get_deep_link_tab_static(sub_entity_id, ID, desc, app_id):
        task_context = urllib.parse.quote(f'{{"subEntityId": "{sub_entity_id}"}}')

        return {
            "linkUrl": f"https://teams.microsoft.com/l/entity/{app_id}/{os.getenv('Tab_Entity_Id')}?context={task_context}",
            "ID": ID,
            "TaskText": desc
        }

    @staticmethod
    def get_deep_link_to_meeting_side_panel(ID, desc, app_id, base_url, chat_id, context_type):
        json_context = f'{{"chatId": "{chat_id}", "contextType": "{context_type}"}}'
        task_context = urllib.parse.quote(json_context)
        encoded_url = urllib.parse.quote(f"{base_url}/ChannelDeepLink.html")

        return {
            "linkUrl": f"https://teams.microsoft.com/l/entity/{app_id}/{os.getenv('Channel_Entity_Id')}?webUrl={encoded_url}&context={task_context}",
            "ID": ID,
            "TaskText": desc
        }
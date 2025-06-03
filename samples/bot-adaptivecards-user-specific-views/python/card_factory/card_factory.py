import os
import json
from jinja2 import Template
from botbuilder.schema import Attachment
from typing import Any

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

class CardFactory:
    SELECT_CARD_TYPE_TEMPLATE = os.path.join(BASE_DIR, "..", "templates", "select-card-type.json")
    REFRESH_SPECIFIC_USER_TEMPLATE = os.path.join(BASE_DIR, "..", "templates", "refresh-specific-user.json")
    REFRESH_ALL_USERS_TEMPLATE = os.path.join(BASE_DIR, "..", "templates", "refresh-all-users.json")
    UPDATED_BASE_CARD_TEMPLATE = os.path.join(BASE_DIR, "..", "templates", "updated-base-card.json")

    BASE_CARD_STATUS = "Base"
    UPDATED_CARD_STATUS = "Updated"
    FINAL_CARD_STATUS = "Final"

    PERSONAL_VIEW = "Personal"
    SHARED_VIEW = "Shared"

    def get_select_card_type_card(self) -> Attachment:
        data = {}
        template = self._get_card_template(self.SELECT_CARD_TYPE_TEMPLATE)
        card_json = template.render(**data)
        return self._create_attachment(card_json)

    def get_auto_refresh_for_all_users_base_card(self, card_type: str) -> Attachment:
        data = {
            "count": 0,
            "cardType": card_type,
            "cardStatus": self.BASE_CARD_STATUS,
            "trigger": "NA",
            "view": self.SHARED_VIEW,
            "message": "Original Message"
        }
        template = self._get_card_template(self.REFRESH_ALL_USERS_TEMPLATE)
        card_json = template.render(**data)
        return self._create_attachment(card_json)

    def get_auto_refresh_for_specific_user_base_card(self, user_mri: str, card_type: str) -> Attachment:
        data = {
            "count": 0,
            "cardType": card_type,
            "cardStatus": self.BASE_CARD_STATUS,
            "trigger": "NA",
            "view": self.SHARED_VIEW,
            "userMri": user_mri,
            "message": "Original Message"
        }
        template = self._get_card_template(self.REFRESH_SPECIFIC_USER_TEMPLATE)
        card_json = template.render(**data)
        return self._create_attachment(card_json)

    def get_updated_card_for_user(self, user_mri: str, action_data: Any) -> Attachment:
        data = {
            "count": action_data["action"]["data"]["refreshCount"],
            "cardType": action_data["action"]["data"]["cardType"],
            "cardStatus": self.UPDATED_CARD_STATUS,
            "trigger": action_data["trigger"],
            "view": self.PERSONAL_VIEW,
            "userMri": user_mri,
            "message": "Updated Message!"
        }
        template = self._get_card_template(self.REFRESH_SPECIFIC_USER_TEMPLATE)
        card_json = template.render(**data)
        return self._create_attachment(card_json)

    def get_final_base_card(self, action_data: Any) -> Attachment:
        data = {
            "count": action_data["action"]["data"]["refreshCount"],
            "cardType": action_data["action"]["data"]["cardType"],
            "cardStatus": self.FINAL_CARD_STATUS,
            "trigger": action_data["trigger"],
            "view": self.SHARED_VIEW,
            "message": "Final Message!"
        }
        template = self._get_card_template(self.UPDATED_BASE_CARD_TEMPLATE)
        card_json = template.render(**data)
        return self._create_attachment(card_json)

    def _get_card_template(self, path: str) -> Template:
        with open(path, encoding="utf-8") as f:
            content = f.read()
            return Template(content)

    def _create_attachment(self, card_json: str) -> Attachment:
        content = json.loads(card_json)
        return Attachment(content_type="application/vnd.microsoft.card.adaptive", content=content)
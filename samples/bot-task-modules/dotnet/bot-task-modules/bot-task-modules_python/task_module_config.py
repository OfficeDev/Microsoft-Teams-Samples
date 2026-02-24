# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.


class UISettings:
    """UI settings for task modules."""
    def __init__(self, width: int, height: int, title: str, id: str, button_title: str):
        self.width = width
        self.height = height
        self.title = title
        self.id = id
        self.button_title = button_title


class TaskModuleIds:
    """Task module identifiers and settings."""
    CUSTOM_FORM = UISettings(510, 450, "Custom Form", "CustomForm", "Custom Form")
    ADAPTIVE_CARD = UISettings(400, 200, "Adaptive Card: Inputs", "AdaptiveCard", "Adaptive Card")

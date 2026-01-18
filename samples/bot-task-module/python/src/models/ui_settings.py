# Copyright (c) Microsoft Corp. All rights reserved.
# Licensed under the MIT License.


class UISettings:
    """
    A class to hold UI settings for Task Module configuration.
    """

    def __init__(self, width: int, height: int, title: str, id: str, button_title: str):
        """
        Initialize UI settings.
        
        :param width: Width of the task module dialog.
        :param height: Height of the task module dialog.
        :param title: Title of the task module dialog.
        :param id: Identifier for the task module.
        :param button_title: Title for the button that invokes this task module.
        """
        self.width = width
        self.height = height
        self.title = title
        self.id = id
        self.button_title = button_title

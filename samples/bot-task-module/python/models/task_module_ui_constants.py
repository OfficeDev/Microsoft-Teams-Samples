# Copyright (c) Microsoft Corp. All rights reserved.
# Licensed under the MIT License.

from .ui_settings import UISettings
from .task_module_ids import TaskModuleIds


class TaskModuleUIConstants:
    """
    A class to hold constants for Task Module UI settings.
    """
    
    # UI settings for YouTube video task module
    YOUTUBE = UISettings(1000, 700, "YouTube Video", TaskModuleIds.YOUTUBE, "YouTube")
    
    # UI settings for Custom Form task module
    CUSTOM_FORM = UISettings(
        510, 450, "Custom Form", TaskModuleIds.CUSTOM_FORM, "Custom Form"
    )
    
    # UI settings for Adaptive Card task module
    ADAPTIVE_CARD = UISettings(
        400, 200, "Adaptive Card: Inputs", TaskModuleIds.ADAPTIVE_CARD, "Adaptive Card",
    )

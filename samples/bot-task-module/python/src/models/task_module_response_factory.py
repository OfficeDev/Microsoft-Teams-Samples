# Copyright (c) Microsoft Corp. All rights reserved.
# Licensed under the MIT License.

from typing import Union, Any


class TaskModuleTaskInfo:
    """
    Task Module task info for configuring the task module dialog.
    """

    def __init__(self):
        self.title: str = None
        self.height: int = None
        self.width: int = None
        self.url: str = None
        self.fallback_url: str = None
        self.card: Any = None

    def to_dict(self) -> dict:
        """Convert to dictionary for API response."""
        result = {}
        if self.title:
            result["title"] = self.title
        if self.height:
            result["height"] = self.height
        if self.width:
            result["width"] = self.width
        if self.url:
            result["url"] = self.url
        if self.fallback_url:
            result["fallbackUrl"] = self.fallback_url
        if self.card:
            result["card"] = self.card
        return result


class TaskModuleResponseFactory:
    """
    A factory class to create TaskModuleResponse objects.
    """

    @staticmethod
    def create_response(value: Union[str, TaskModuleTaskInfo]) -> dict:
        """
        Creates a TaskModuleResponse based on the provided value.

        :param value: A string or TaskModuleTaskInfo object.
        :return: A TaskModuleResponse dict.
        """
        if isinstance(value, TaskModuleTaskInfo):
            return {
                "task": {
                    "type": "continue",
                    "value": value.to_dict()
                }
            }
        return {
            "task": {
                "type": "message",
                "value": value
            }
        }

    @staticmethod
    def to_task_module_response(task_info: TaskModuleTaskInfo) -> dict:
        """
        Converts TaskModuleTaskInfo to a TaskModuleResponse.

        :param task_info: The TaskModuleTaskInfo object.
        :return: A TaskModuleResponse dict.
        """
        return TaskModuleResponseFactory.create_response(task_info)

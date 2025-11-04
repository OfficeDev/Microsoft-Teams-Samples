# Copyright (c) Microsoft Corp. All rights reserved.
# Licensed under the MIT License.

from typing import Union

from botbuilder.schema.teams import (
    TaskModuleResponse,
    TaskModuleMessageResponse,
    TaskModuleTaskInfo,
    TaskModuleContinueResponse,
)


class TaskModuleResponseFactory:
    """
    A factory class to create TaskModuleResponse objects.
    """

    @staticmethod
    def create_response(value: Union[str, TaskModuleTaskInfo]) -> TaskModuleResponse:
        """
        Creates a TaskModuleResponse based on the provided value.
        
        :param value: A string or TaskModuleTaskInfo object.
        :return: A TaskModuleResponse object.
        """
        if isinstance(value, TaskModuleTaskInfo):
            return TaskModuleResponse(task=TaskModuleContinueResponse(value=value))
        return TaskModuleResponse(task=TaskModuleMessageResponse(value=value))

    @staticmethod
    def to_task_module_response(task_info: TaskModuleTaskInfo) -> TaskModuleResponse:
        """
        Converts TaskModuleTaskInfo to a TaskModuleResponse.
        
        :param task_info: The TaskModuleTaskInfo object.
        :return: A TaskModuleResponse object.
        """
        return TaskModuleResponseFactory.create_response(task_info)
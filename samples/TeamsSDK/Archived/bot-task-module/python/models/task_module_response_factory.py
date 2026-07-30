# Copyright (c) Microsoft Corp. All rights reserved.
# Licensed under the MIT License.

from botbuilder.schema.teams import (
    TaskModuleResponse,
    TaskModuleTaskInfo,
    TaskModuleContinueResponse,
)


class TaskModuleResponseFactory:
    @staticmethod
    def to_task_module_response(task_info: TaskModuleTaskInfo) -> TaskModuleResponse:
        return TaskModuleResponse(task=TaskModuleContinueResponse(value=task_info))

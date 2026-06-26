# Copyright (c) Microsoft Corp. All rights reserved.
# Licensed under the MIT License.

from botbuilder.schema.teams import (
    TaskModuleResponse,
    TaskModuleMessageResponse,
    TaskModuleTaskInfo,
    TaskModuleContinueResponse,
)


class TaskModuleResponseFactory:
    @staticmethod
    def create_response(value: str | TaskModuleTaskInfo) -> TaskModuleResponse:
        if isinstance(value, TaskModuleTaskInfo):
            return TaskModuleResponse(task=TaskModuleContinueResponse(value=value))
        return TaskModuleResponse(task=TaskModuleMessageResponse(value=value))
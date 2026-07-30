// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

/**
 * TaskModuleResponseFactory creates responses for task modules in Microsoft Teams.
 */
class TaskModuleResponseFactory {
    /**
     * Creates a task module response.
     * @param {Object|string} taskModuleInfoOrString - The task module info object or a string message.
     * @returns {Object} - The task module response.
     */
    static createResponse(taskModuleInfoOrString) {
        return {
            task: {
                type: typeof taskModuleInfoOrString === 'string' ? 'message' : 'continue',
                value: taskModuleInfoOrString
            }
        };
    }

    /**
     * Converts task info to a task module response.
     * @param {Object} taskInfo - The task info object.
     * @returns {Object} - The task module response.
     */
    static toTaskModuleResponse(taskInfo) {
        return TaskModuleResponseFactory.createResponse(taskInfo);
    }
}

module.exports.TaskModuleResponseFactory = TaskModuleResponseFactory;

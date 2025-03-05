// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

/**
 * TaskModuleResponseFactory is a utility class for creating task module responses.
 * It provides methods for generating task responses, either as a message or a continuation.
 */
class TaskModuleResponseFactory {
    /**
     * Creates a task module response based on the input type.
     * If the input is a string, a message type response is created.
     * If the input is an object, a continue type response is created.
     * 
     * @param {string|Object} taskModuleInfoOrString - The task module info or string value to generate the response.
     * @returns {Object} The generated task module response.
     */
    static createResponse(taskModuleInfoOrString) {
        if (typeof taskModuleInfoOrString === 'string') {
            return {
                task: {
                    type: 'message',
                    value: taskModuleInfoOrString
                }
            };
        }

        return {
            task: {
                type: 'continue',
                value: taskModuleInfoOrString
            }
        };
    }

    /**
     * A wrapper for createResponse. It returns a task module response based on the provided taskInfo.
     * 
     * @param {Object} taskInfo - The task info to generate the response.
     * @returns {Object} The task module response.
     */
    static toTaskModuleResponse(taskInfo) {
        return TaskModuleResponseFactory.createResponse(taskInfo);
    }
}

module.exports.TaskModuleResponseFactory = TaskModuleResponseFactory;
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

/**
 * ActivityLog handles the storage and retrieval of activities.
 * This is used to track messages sent by the bot so we can respond to reactions.
 * @class
 */
class ActivityLog {
    /**
     * Creates an instance of ActivityLog.
     * @param {Object} storage - The storage mechanism for activities.
     */
    constructor(storage) {
        this._storage = storage;
    }

    /**
     * Appends an activity to the log with its activity ID.
     * @param {string} activityId - The ID of the activity.
     * @param {Object} activity - The activity to append.
     * @throws {TypeError} If activityId or activity is null.
     */
    async append(activityId, activity) {
        if (activityId == null) {
            throw new TypeError('activityId is required for ActivityLog.append');
        }
        
        if (activity == null) {
            throw new TypeError('activity is required for ActivityLog.append');
        }

        const conversationId = activity.conversation?.id || 'unknown';
        const key = `activities_${conversationId}`;
        
        // Get existing activities for this conversation
        let activities = this._storage.get(key) || {};
        
        // Store the activity with its ID
        activities[activityId] = { 
            activity: {
                text: activity.text,
                type: activity.type,
                timestamp: new Date().toISOString()
            }
        };
        
        // Save back to storage
        this._storage.set(key, activities);
    }

    /**
     * Finds an activity in the log by its ID.
     * @param {string} activityId - The ID of the activity to find.
     * @param {string} conversationId - The conversation ID to search in.
     * @returns {Object|null} The activity if found, otherwise null.
     * @throws {TypeError} If activityId is null.
     */
    async find(activityId, conversationId = 'unknown') {
        if (activityId == null) {
            throw new TypeError('activityId is required for ActivityLog.find');
        }

        const key = `activities_${conversationId}`;
        const activities = this._storage.get(key) || {};
        
        return activities[activityId] ? activities[activityId].activity : null;
    }

    /**
     * Clears all activities for a conversation.
     * @param {string} conversationId - The conversation ID to clear.
     */
    async clear(conversationId) {
        const key = `activities_${conversationId}`;
        this._storage.delete(key);
    }
}

module.exports = { ActivityLog };
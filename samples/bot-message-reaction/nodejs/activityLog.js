// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

/**
 * ActivityLog handles the storage and retrieval of activities.
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
     * Appends an activity to the log.
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

        const obj = {};
        obj[activityId] = { activity };
        await this._storage.write(obj);
    }

    /**
     * Finds an activity in the log by its ID.
     * @param {string} activityId - The ID of the activity to find.
     * @returns {Object|null} The activity if found, otherwise null.
     * @throws {TypeError} If activityId is null.
     */
    async find(activityId) {
        if (activityId == null) {
            throw new TypeError('activityId is required for ActivityLog.find');
        }

        const items = await this._storage.read([activityId]);
        return (items && items[activityId]) ? items[activityId].activity : null;
    }
}

exports.ActivityLog = ActivityLog;
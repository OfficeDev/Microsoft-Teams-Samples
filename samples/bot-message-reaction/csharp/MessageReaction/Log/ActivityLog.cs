// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace MessageReaction.Log
{
    /// <summary>
    /// Manages the logging of activities.
    /// </summary>
    public class ActivityLog
    {
        private readonly IStorage _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityLog"/> class.
        /// </summary>
        /// <param name="storage">The storage to use for logging activities.</param>
        public ActivityLog(IStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        /// <summary>
        /// Appends an activity to the log.
        /// </summary>
        /// <param name="activityId">The ID of the activity.</param>
        /// <param name="activity">The activity to log.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when activityId or activity is null.</exception>
        public async Task AppendAsync(string activityId, Activity activity)
        {
            if (string.IsNullOrEmpty(activityId))
            {
                throw new ArgumentNullException(nameof(activityId));
            }

            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            var data = new Dictionary<string, object> { { activityId, activity } };
            await _storage.WriteAsync(data);
        }

        /// <summary>
        /// Finds an activity in the log by its ID.
        /// </summary>
        /// <param name="activityId">The ID of the activity to find.</param>
        /// <returns>The activity if found; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when activityId is null.</exception>
        public async Task<Activity> FindAsync(string activityId)
        {
            if (string.IsNullOrEmpty(activityId))
            {
                throw new ArgumentNullException(nameof(activityId));
            }

            var activities = await _storage.ReadAsync(new[] { activityId });
            return activities.TryGetValue(activityId, out var activity) ? (Activity)activity : null;
        }
    }
}
// <copyright file="TaskDetails.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Newtonsoft.Json;

namespace BotDailyTaskReminder.Models
{
    /// <summary>
    /// Task details model class.
    /// </summary>
    public class TaskDetails<T>
    {
        /// <summary>
        /// Gets or sets title value of task.
        /// </summary>
        [JsonProperty("title")]
        public object Title { get; set; }

        /// <summary>
        /// Gets or sets description value of task.
        /// </summary>
        [JsonProperty("description")]
        public object Description { get; set; }

        /// <summary>
        /// Gets or sets date-time value of task.
        /// </summary>
        [JsonProperty("dateTime")]
        public object DateTime { get; set; }

        /// <summary>
        /// Gets or sets slected days value of task.
        /// </summary>
        [JsonProperty("selectedDays")]
        public object SelectedDays { get; set; }
    }
}
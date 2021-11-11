// <copyright file="SaveTaskDetail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;

namespace BotTaskReminder.Models
{
    /// <summary>
    /// Save task detail model class.
    /// </summary>
    public class SaveTaskDetail
    {
        /// <summary>
        /// Gets or sets title value of task.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets description value of task.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets date-time value of task.
        /// </summary>
        public DateTimeOffset DateTime { get; set; }
    }
}
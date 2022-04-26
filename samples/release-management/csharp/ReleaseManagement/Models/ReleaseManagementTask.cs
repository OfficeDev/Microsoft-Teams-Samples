// <copyright file="ReleaseManagementTask.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace ReleaseManagement.Models
{
    public class ReleaseManagementTask
    {
        /// <summary>
        /// Gets or sets Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets task title.
        /// </summary>
        public string TaskTitle { get; set; }

        /// <summary>
        /// Gets or sets assigned member name.
        /// </summary>
        public string AssignedToName { get; set; }

        /// <summary>
        /// Gets or sets stakeholder team members mail.
        /// </summary>
        public IEnumerable<string> StakeholderTeam { get; set; }

        /// <summary>
        /// Gets or sets state of task.
        /// </summary>
        public string State { get; set; }
    }
}

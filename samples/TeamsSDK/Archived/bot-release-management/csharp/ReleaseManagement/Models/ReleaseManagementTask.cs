// <copyright file="ReleaseManagementTask.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ReleaseManagement.Models
{
    using System.Collections.Generic;

    public class ReleaseManagementTask
    {
        /// <summary>
        /// Gets or sets notification Id.
        /// </summary>
        public string NotificationId { get; set; }

        /// <summary>
        /// Gets or sets task title.
        /// </summary>
        public string TaskTitle { get; set; }

        /// <summary>
        /// Gets or sets assigned member name.
        /// </summary>
        public string AssignedToName { get; set; }

        /// <summary>
        /// Gets or sets assigned to user profile image.
        /// </summary>
        public string AssignedToProfileImage { get; set; }

        /// <summary>
        /// Gets or sets comma seperated stakeholder team members mail.
        /// </summary>
        public string StakeholderTeam { get; set; }

        /// <summary>
        /// Gets or sets state of task.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets name of workitem creator.
        /// </summary>
        public string CreatedByName { get; set; }

        /// <summary>
        /// Gets or sets profile image of workitem creator.
        /// </summary>
        public string CreatedByProfileImage { get; set; }

        /// <summary>
        /// Gets or sets list of group member.
        /// </summary>
        public IEnumerable<string> GroupChatMembers { get; set; }

        /// <summary>
        /// Gets or sets state of workitem url.
        /// </summary>
        public string WorkitemUrl { get; set; }
    }
}

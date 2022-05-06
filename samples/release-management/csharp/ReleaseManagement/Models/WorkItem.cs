// <copyright file="WorkItem.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ReleaseManagement.Models
{
    public class WorkItem
    {
        /// <summary>
        /// Gets or sets notification Id.
        /// </summary>
        public string NotificationId { get; set; }

        /// <summary>
        /// Gets or sets resource property of incoming workitem payload.
        /// </summary>
        public ResourceModel Resource { get; set; }
    }
}

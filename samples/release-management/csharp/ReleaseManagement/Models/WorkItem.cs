// <copyright file="WorkItem.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ReleaseManagement.Models
{
    public class WorkItem
    {
        /// <summary>
        /// Gets or sets Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets resource.
        /// </summary>
        public ResourceModel Resource { get; set; }
    }
}

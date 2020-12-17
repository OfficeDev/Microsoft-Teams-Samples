// <copyright file="PostNotificationServiceResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Service
{
    /// <summary>
    /// Post token status change model.
    /// </summary>
    public class PostNotificationServiceResponse
    {
        /// <summary>
        /// Gets or sets the activity ID of the posted notification.
        /// </summary>
        public string ActivityId { get; set; }
    }
}
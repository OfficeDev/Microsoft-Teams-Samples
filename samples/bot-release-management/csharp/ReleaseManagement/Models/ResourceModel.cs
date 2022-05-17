// <copyright file="Resource.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ReleaseManagement.Models
{
    using System.Collections.Generic;

    public class ResourceModel
    {
        /// <summary>
        /// Gets or sets project Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets fields property of incoming workitem payload.
        /// </summary>
        public Dictionary<string, string> Fields { get; set; }

        /// <summary>
        /// Gets or sets links property of incoming workitem payload.
        /// </summary>
        public Dictionary<string, Dictionary<string, dynamic>> _links { get; set; }
    }
}

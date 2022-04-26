// <copyright file="Resource.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace ReleaseManagement.Models
{
    public class Resource
    {
        /// <summary>
        /// Gets or sets project Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets fields.
        /// </summary>
        public Dictionary<string, string> Fields { get; set; }
    }
}

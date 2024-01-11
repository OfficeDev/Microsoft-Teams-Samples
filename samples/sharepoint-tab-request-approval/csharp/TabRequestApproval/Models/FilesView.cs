// <copyright file="FilesView.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Models
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Graph;

    /// <summary>
    /// Files view model.
    /// </summary>
    [Serializable]
    public class FilesView
    {
        /// <summary>
        /// Gets or sets the drive id.
        /// </summary>
        public string DriveId { get; set; }

        /// <summary>
        /// Gets or sets the drive item path.
        /// </summary>
        public List<DriveItem> Path { get; set; }
    }
}

// <copyright file="Resource.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities
{
    /// <summary>
    /// Resource entity definition.
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// Gets or sets resource id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets resource location url.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets resource name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets resource preview image url.
        /// </summary>
        public string PreviewImageUrl { get; set; }
    }
}

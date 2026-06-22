// <copyright file="ContainerPermission.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Models
{
    using System.Collections.Generic;
    using System.Security;

    /// <summary>
    /// Container permission model.
    /// </summary>
    public class ContainerPermission
    {
        /// <summary>
        /// Represents the roles on the model.
        /// </summary>
        public IEnumerable<string> roles;

        /// <summary>
        /// Represents the permission user.
        /// </summary>
#pragma warning disable CA1051 // Do not declare visible instance fields
        public PermissionUser grantedToV2;
#pragma warning restore CA1051 // Do not declare visible instance fields

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string id { get; set; }
    }
}

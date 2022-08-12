// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Apps.QBot.Domain.Models
{
    /// <summary>
    /// The User Configuration model class.
    /// This class contains the user-specific settings and values.
    /// </summary>
    public sealed class UserConfiguration
    {
        /// <summary>
        /// Gets or sets the user's Bot Framework Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is a QBot administrator.
        /// </summary>
        /// <value>True if the user is a QBot administrator, false otherwise.</value>
        public bool IsAdministrator { get; set; }
    }
}

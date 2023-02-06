// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Apps.QBot.Domain.Models
{
    /// <summary>
    /// The User model class.
    /// </summary>
    public sealed class User
    {
        /// <summary>
        /// Gets or sets the user's Azure Active Directory Id.
        /// </summary>
        public string AadId { get; set; }

        /// <summary>
        /// Gets or sets the user's Bot Framework Id.
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets the user's principal name.
        /// </summary>
        /// <remarks>
        /// This is almost always the user's organizational email address.
        /// </remarks>
        public string Upn { get; set; }

        /// <summary>
        /// Gets or sets the user's human readable display name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user's profile picture url. Base64 encoded image.
        /// </summary>
        public string ProfilePicUrl { get; set; }
    }
}

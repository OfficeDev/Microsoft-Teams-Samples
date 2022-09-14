// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Models
{
    /// <summary>
    /// User contains the attributes related to a user like UserId
    /// Additional information can be added in the future.
    /// IEquatable interface is implemented to handle duplicates
    /// </summary>
    /// <remarks>
    /// For in-tenant, federated and guest users we use the AAD ID as the user's
    /// identifier. For anonymous users, we use their email address as their identifier.
    /// </remarks>
    public record User : IEquatable<User>
    {
        /// <summary>
        /// Gets or sets UserId / AAD ID
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets User's Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets User's Email
        /// </summary>
        public string? Email { get; set; }

        public User DeepCopy()
        {
            User other = (User)this.MemberwiseClone();
            return other;
        }
    }
}

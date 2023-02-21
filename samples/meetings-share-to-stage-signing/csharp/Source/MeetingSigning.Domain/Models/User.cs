// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Models
{
    /// <summary>
    /// User contains the attributes related to a user like UserId
    /// Additional information can be added in the future.
    /// IEquatable interface is implemented to handle duplicates
    /// </summary>
    /// <remarks>
    /// For in-tenant, federated and guest users we use the AzureAD Object ID as the user's
    /// identifier. For anonymous users, we use their email address as their identifier.
    /// </remarks>
    public record User
    {
        /// <summary>
        /// Gets or sets UserId / AzureAD Object ID
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

        /// <summary>
        /// Unique ID for this user. The ID is decided based on the UserId or Email.
        /// This ID is subpar, especially if you plan to support multiple identity providers for the same user.
        /// It's not likely with this implementation but a user might create an User account with an email, and later
        /// get assigned an AzureAD Object ID and create a new User. In that case the same email would be in the user repository twice.
        ///
        /// A better solution would be to create an ID that is separate from the information the user provided.
        /// </summary>
        public string Id
        {
            get
            {
                return UserId != null ? $"azureAd://{UserId}" : $"email://{Email}";
            }
        }

        public User DeepCopy()
        {
            User other = (User)this.MemberwiseClone();
            return other;
        }
    }
}

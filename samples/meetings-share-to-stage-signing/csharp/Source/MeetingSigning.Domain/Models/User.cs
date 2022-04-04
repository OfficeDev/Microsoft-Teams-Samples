// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Models
{
    /// <summary>
    /// User contains the attributes related to a user like UserId
    /// Additional information can be added in the future.
    /// IEquatable interface is implemented to handle duplicates
    /// </summary>
    public class User : IEquatable<User>
    {
        /// <summary>
        /// Gets or sets UserId / AAD ID
        /// </summary>
        ///
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets User's Name
        /// </summary>
        public string Name { get; set; }

        public override bool Equals(object? obj) => base.Equals(obj as User);

        public bool Equals(User other)
        {
            return other != null &&
                   UserId == other.UserId;
        }

        public override int GetHashCode()
        {
            // Considering UserId as unique 
            return UserId.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }

        public User DeepCopy()
        {
            User other = (User)this.MemberwiseClone();
            return other;
        }
    }
}

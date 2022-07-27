// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.Teams.Apps.QBot.Domain.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Member model class.
    /// </summary>
    public sealed class Member
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Member"/> class.
        /// </summary>
        public Member()
        {
            this.TutorialGroupMembership = new HashSet<string>();
        }

        /// <summary>
        /// Gets or sets member's Team Id.
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets member's AAD Id.
        /// </summary>
        public string AadId { get; set; }

        /// <summary>
        /// Gets or sets member's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets member's role.
        /// </summary>
        public MemberRole Role { get; set; }

        /// <summary>
        /// Gets or sets the user's principal name.
        /// </summary>
        /// <remarks>
        /// This will almost always be set to the user's organizational email address.
        /// </remarks>
        public string Upn { get; set; }

        /// <summary>
        /// Gets or sets tutorial group membership. List of tutorial group ids.
        /// </summary>
        public IEnumerable<string> TutorialGroupMembership { get; set; }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingBotSample.Model
{
    public class ParticipantDetails
    {
        /// <summary>
        /// Gets or sets the name of participant.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the user id of participant.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the email of participant.
        /// </summary>
        public string? EmailId { get; set; }

        /// <summary>
        /// Gets or sets the tenant id of participant.
        /// </summary>
        public string? TenantId { get; set; }
    }
}

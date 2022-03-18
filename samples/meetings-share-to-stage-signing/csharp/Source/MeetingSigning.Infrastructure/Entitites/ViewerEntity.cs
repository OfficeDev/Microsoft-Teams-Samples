// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entitites
{
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entities;

    public class ViewerEntity
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Gets Observer object that contains information like AAD User UserId of the Signer
        /// </summary>
        public UserEntity Observer { get; set; }
    }
}

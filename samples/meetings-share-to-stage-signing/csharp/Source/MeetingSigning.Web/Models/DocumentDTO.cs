// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Web.Models
{
    using System;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    public class DocumentDTO
    {
        /// <summary>
        /// The Document returned
        /// </summary>
        public Document Document { get; set; }

        /// <summary>
        /// The details of the calling user.
        /// </summary>
        public User CallerUser { get; set; }
    }
}

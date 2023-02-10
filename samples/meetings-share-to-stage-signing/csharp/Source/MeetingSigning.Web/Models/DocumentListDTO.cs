// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Web.Models
{
    using System.Collections.Generic;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    public class DocumentListDTO
    {
        /// <summary>
        /// The Documents returned
        /// </summary>
        public IList<Document> Documents { get; set; }

        /// <summary>
        /// The details of the calling user.
        /// </summary>
        public User CallerUser { get; set; }
    }
}

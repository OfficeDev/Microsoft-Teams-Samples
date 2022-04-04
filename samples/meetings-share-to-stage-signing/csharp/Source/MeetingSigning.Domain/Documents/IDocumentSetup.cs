// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Documents
{
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    public interface IDocumentSetup
    {
        /// <summary>
        /// AddDocumentAsync takes POST document API parameters and calls the data repositories required.
        /// </summary>
        /// <param name="documentDetails"></param>
        /// <param name="ownerId">AADId of the owner</param>
        /// <returns>Document created</returns>
        public Task<Document> AddDocumentAsync(DocumentInput documentDetails, string ownerId);
    }
}

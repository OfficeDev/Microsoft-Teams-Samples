// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Documents
{
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    /// <summary>
    /// Service to manage requests related to Documents
    /// </summary>
    public interface IDocumentService
    {
        /// <summary>
        /// CreateDocumentAsync Creates a document that will be signed in the future
        /// </summary>
        /// <param name="documentDetails">Document details from the input</param>
        /// <param name="ownerId">AADId of the owner</param>
        /// <returns>Document</returns>
        Task<Document> CreateDocumentAsync(DocumentInput documentDetails, string ownerId);

        /// <summary>
        /// GetDocumentAsync finds a document matching the documentId
        /// </summary>
        /// <param name="documentId">The unique id of a Document</param>
        /// <returns>Document</returns>
        Task<Document> GetDocumentAsync(Guid documentId);

        /// <summary>
        /// GetDocumentsAsync finds list of documents belonging to a user
        /// </summary>
        /// <param name="ownerId">AADId of the owner</param>
        /// <returns>List of Documents</returns>
        Task<IList<Document>> GetDocumentsAsync(string ownerId);
        /// SignDocumentAsync adds a user's Signature attempt to a document.
        /// </summary>
        /// <param name="id">The unique id of a Document</param>
        /// <param name="signatureAttempt">The Signature attempt to update in the database</param>
        /// <returns>The signed signature</returns>
        Task<Signature> SignDocumentAsync(Guid id, Signature signatureAttempt);
    }
}

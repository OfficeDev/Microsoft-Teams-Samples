// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    public interface IDocumentRepository
    {
        /// <summary>
        /// CreateDocument maps a document to document entity.
        /// Inserts a document entity into the database.
        /// </summary>
        /// <param name="document"></param>
        /// <returns>Document created</returns>
        /// <exception cref="DbUpdateException">When a failure occurs while trying to insert a document</exception>
        /// <exception cref="Exception">Generic exception</exception>
        Task<Document> CreateDocument(Document document);

        /// <summary>
        /// GetDocument finds the first or default document that matches the given documentId
        /// </summary>
        /// <param name="documentId">Unique id of a document</param>
        /// <returns>Document found or null if not found</returns>
        Task<Document> GetDocument(Guid documentId);

        /// <summary>
        /// GetDocuments finds the all the documents that belong to a user(Owner)
        /// </summary>
        /// <param name="ownerId">AzureAD Object Id of the owner</param>
        /// <returns>List of Documents found. An empty list if no matching documents are found.</returns>
        Task<IList<Document>> GetDocuments(string ownerId);
    }
}

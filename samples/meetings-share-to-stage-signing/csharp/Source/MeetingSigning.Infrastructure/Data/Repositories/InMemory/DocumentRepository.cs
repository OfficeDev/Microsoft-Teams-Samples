// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Data.Repositories.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    /// <summary>
    /// DocumentRepository is an inmemory implementation of IDocumentRepository
    /// </summary>
    public class DocumentRepository : IDocumentRepository
    {
        private readonly IDictionary<Guid, Document> documentDictionary;

        public DocumentRepository()
        {
            this.documentDictionary = new Dictionary<Guid, Document>();
        }

        /// <summary>
        /// CreateDocument adds a new document to the inmemory dictionary.
        /// </summary>
        /// <param name="document"></param>
        /// <returns>Document created</returns>
        public async Task<Document> CreateDocument(Document document)
        {
            document.Id = Guid.NewGuid();
            this.documentDictionary.Add(document.Id, document);
            return document;
        }

        /// <summary>
        /// GetDocument tries to find a document given a documentId
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns>Document found or null otherwise</returns>
        public async Task<Document> GetDocument(Guid documentId)
        {
            if (this.documentDictionary.TryGetValue(documentId, out Document document))
            {
                return document.DeepCopy();
            }
            throw new ApiException(HttpStatusCode.NotFound, ErrorCode.DocumentNotFound, $"Requested document with id {documentId} was not found.");
        }

        /// <summary>
        /// GetDocuments tries to find all documents belonging to a user
        /// </summary>
        /// <param name="ownerId">AzureAD Object Id of the user</param>
        /// <returns>Document found or an empty list otherwise</returns>
        public async Task<IList<Document>> GetDocuments(string ownerId)
        {
            IList<Document> documents = this.documentDictionary
                .Where(document => document.Value.OwnerId.Equals(ownerId, StringComparison.InvariantCultureIgnoreCase))
                .Select(d => d.Value).ToList();
            return documents;
        }
    }
}

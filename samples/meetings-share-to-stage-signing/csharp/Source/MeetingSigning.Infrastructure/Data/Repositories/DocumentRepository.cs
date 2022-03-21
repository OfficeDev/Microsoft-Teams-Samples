// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Data.Repositories
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entities;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entitites;

    /// <summary>
    /// DocumentRepository implements IDocumentRepository as an Entity Framework solution
    /// </summary>
    public class DocumentRepository : IDocumentRepository
    {
        private readonly MeetingSigningDbContext dbContext;
        private readonly IMapper mapper;

        public DocumentRepository(MeetingSigningDbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc/>
        public async Task<Document> CreateDocument(Document document)
        {
            try
            {
                var documentEntity = await this.ConvertDocumentToDocumentEntity(document);
                await this.dbContext.Documents.AddAsync(documentEntity);
                await this.dbContext.SaveChangesAsync();
                return ConvertDocumentEntityToDocument(documentEntity);
            }
            catch (DbUpdateException exception)
            {
                string message = $"Failed to add document";
                throw new ApiException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public async Task<Document> GetDocument(Guid documentId)
        {
            var documentEntity = await dbContext.Documents
                .Include(doc => doc.SignatureEntities).ThenInclude(u => u.Signer)
                .Include(doc => doc.ViewerEntities).ThenInclude(v => v.Observer)
                .FirstOrDefaultAsync(c => c.Id.Equals(documentId));

            if (documentEntity == null)
            {
                throw new ApiException(HttpStatusCode.NotFound, ErrorCode.DocumentNotFound, $"Document id '{ documentId }' was not found");
            }

            return this.ConvertDocumentEntityToDocument(documentEntity);
        }

        /// <inheritdoc/>
        public async Task<IList<Document>> GetDocuments(string ownerId)
        {
            var documentEntities = await dbContext.Documents.Where(doc => doc.OwnerId.Equals(ownerId)).
                Include(doc => doc.SignatureEntities).ThenInclude(u => u.Signer).
                Include(doc => doc.ViewerEntities).ThenInclude(v => v.Observer).
                ToListAsync();

            if (documentEntities.IsNullOrEmpty())
            {
                return new List<Document>();
            }
            var documents = new List<Document>();
            foreach(var docEntity in documentEntities)
            {
                documents.Add(this.ConvertDocumentEntityToDocument(docEntity));
            }
            return documents;
        }

        /// <inheritdoc/>
        private Document ConvertDocumentEntityToDocument(DocumentEntity documentEntity)
        {
            var signatures = this.mapper.Map<List<Signature>>(documentEntity.SignatureEntities);
            var viewers = this.mapper.Map<List<Viewer>>(documentEntity.ViewerEntities);
            var document = new Document
            {
                DocumentType = documentEntity.DocumentType,
                Id = documentEntity.Id,
                OwnerId = documentEntity.OwnerId,
                Viewers = viewers,
                Signatures = signatures
            };

            return this.mapper.Map<Document>(document);
        }

        /// <inheritdoc/>
        private async Task<DocumentEntity> ConvertDocumentToDocumentEntity(Document document)
        {
            var docEntity = this.mapper.Map<DocumentEntity>(document);

            var signatureEntities = new List<SignatureEntity>();
            foreach (var signature in document.Signatures)
            {
                signatureEntities.Add(await dbContext.Signatures.FirstOrDefaultAsync(s => s.Id.Equals(signature.Id)).ConfigureAwait(false));
            }

            var viewerEntities = new List<ViewerEntity>();
            foreach (var viewer in document.Viewers)
            {
                viewerEntities.Add(await dbContext.Viewers.FirstOrDefaultAsync(s => s.Id.Equals(viewer.Id)).ConfigureAwait(false));
            }
            docEntity.SignatureEntities = signatureEntities;
            docEntity.ViewerEntities = viewerEntities;
            return docEntity;
        }
    }
}

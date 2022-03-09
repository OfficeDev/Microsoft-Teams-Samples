// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Documents
{
    using System.Net;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    /// <inheritdoc/>
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ISignatureRepository _signatureRepository;
        private readonly IDocumentSetup _documentSetup;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(IDocumentRepository documentRepository,
            ISignatureRepository signatureRepository,
            IDocumentSetup documentSetup,
            ILogger<DocumentService> logger)
        {
            _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
            _signatureRepository = signatureRepository ?? throw new ArgumentNullException(nameof(signatureRepository));
            _documentSetup = documentSetup ?? throw new ArgumentNullException(nameof(documentSetup));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<Document> CreateDocumentAsync(DocumentInput documentDetails, string ownerId)
        {
            _logger.LogInformation("About to create a document");
            Document document = await this._documentSetup.AddDocumentAsync(documentDetails, ownerId);
            _logger.LogInformation("Completed document creation");
            return document;
        }

        /// <inheritdoc/>
        public async Task<Document> GetDocumentAsync(Guid documentId)
        {
            Document document = await _documentRepository.GetDocument(documentId).ConfigureAwait(false);
            return document;
        }

        /// <inheritdoc/>
        public async Task<IList<Document>> GetDocumentsAsync(string ownerId)
        {
            IList<Document> documents = await _documentRepository.GetDocuments(ownerId).ConfigureAwait(false);
            return documents;
        }

        /// <inheritdoc/>
        public async Task<Signature> SignDocumentAsync(Guid id, Signature signatureAttempt)
        {
            signatureAttempt.SignedDateTime = DateTime.UtcNow;
            signatureAttempt.IsSigned = signatureAttempt.Text != null;

            Document document = await _documentRepository.GetDocument(id);

            if (document.Signatures.Any(s => s.Id == signatureAttempt.Id && s.Signer.UserId == signatureAttempt.Signer.UserId))
            {
                return await _signatureRepository.UpdateSignature(signatureAttempt);
            }
            else
            {
                throw new ApiException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, "Signature is not valid for this Document.");
            }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Web.Controllers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Documents;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;
    using Microsoft.Teams.Samples.MeetingSigning.Web.Authorization;
    using Microsoft.Teams.Samples.MeetingSigning.Web.Exceptions;
    using Microsoft.Teams.Samples.MeetingSigning.Web.Models;

    [ApiController]
    [Authorize(AuthenticationSchemes = AuthenticationScheme.AadAndMsa)]
    [Route("[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService documentService;
        private readonly IAuthorizationService authorizationService;

        public DocumentController(IDocumentService documentService, IAuthorizationService authorizationService)
        {
            this.documentService = documentService;
            this.authorizationService = authorizationService;
        }

        /// <summary>
        /// GetDocument returns a specific document matching with the document id.
        /// </summary>
        /// <param name="documentId"></param>
        /// <exception cref="ApiException">If no matching document for the specific document Id is found
        /// A Http Not found 404 exception with Document Not found error type is thrown</exception>
        [HttpGet("{documentId}", Name = "GetDocument")]
        public async Task<ActionResult<DocumentListDTO>> GetDocumentAsync([FromRoute] Guid documentId)
        {
            ApiArgumentNullException.ThrowIfNull(documentId);
            if (documentId == default)
            {
                throw new ApiArgumentException($"'{nameof(documentId)}' must be a non default Guid.", nameof(documentId));
            }

            var document = await this.documentService.GetDocumentAsync(documentId);
            await authorizationService.AuthorizeAsync(User, document, AuthZPolicy.GetDocumentPolicy);

            DocumentListDTO response = new DocumentListDTO
            {
                Documents = new[] { document },
                CallerUser = new User { UserId = GetAuthenticatedUserId(), Name = string.Empty, Email = User.GetUserEmail() ?? string.Empty }
            };

            return new OkObjectResult(response);
        }

        /// <summary>
        /// GetDocuments returns all the available documents belonging to the caller(owner).
        /// </summary>
        /// <returns>An Ok Result on success along with a json response of documents
        /// An empty list when documents were not found</returns>
        [HttpGet(Name = "GetDocuments")]
        public async Task<ActionResult<DocumentListDTO>> GetDocumentsAsync()
        {
            string? authenticatedUserId = GetAuthenticatedUserId();

            var documents = await this.documentService.GetDocumentsAsync(authenticatedUserId);

            DocumentListDTO response = new DocumentListDTO
            {
                Documents = documents,
                CallerUser = new User { UserId = authenticatedUserId, Name = string.Empty, Email = User.GetUserEmail() ?? string.Empty }
            };

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Signs a document
        /// </summary>
        /// <param name="documentDetails">Input details required to create a document</param>
        /// <returns>An OK Result on success along with a json response of the document thus created</returns>
        /// <exception cref="ApiArgumentException">If the parameters DocumentType, Signers from the input are null</exception>
        [HttpPost(Name = "CreateDocument")]
        public async Task<ActionResult<Document>> CreateDocumentAsync([FromBody] DocumentInput documentDetails)
        {
            ApiArgumentNullException.ThrowIfNull(documentDetails?.DocumentType);
            ApiArgumentNullException.ThrowIfNull(documentDetails?.Signers);

            Document document = await documentService.CreateDocumentAsync(documentDetails, GetAuthenticatedUserId()).ConfigureAwait(false);

            return new OkObjectResult(document);
        }

        /// <summary>
        /// Signs a document
        /// </summary>
        /// <param name="documentId">Id of the document you wish to sign</param>
        /// <param name="signature">Signature details of the signature attempt</param>
        /// <returns>An OK Result on success, with the signed Signature object</returns>
        /// <exception cref="ApiArgumentException">If the parameters are null, or default</exception>
        [HttpPost("{documentId}/sign", Name = "SignDocument")]
        public async Task<ActionResult<Signature>> SignDocumentAsync([FromRoute] string documentId, [FromBody] Signature signature)
        {
            ApiArgumentNullException.ThrowIfNull(documentId);
            ApiArgumentNullException.ThrowIfNull(signature);

            Guid parsedDocumentId;
            if (!Guid.TryParse(documentId, out parsedDocumentId) ||
                parsedDocumentId == default)
            {
                throw new ApiArgumentException($"'{nameof(documentId)}' must be a non default Guid.", nameof(documentId));
            }

            await authorizationService.AuthorizeAsync(User, signature, AuthZPolicy.SignDocumentPolicy);
            Signature signatureResult = await documentService.SignDocumentAsync(parsedDocumentId, signature).ConfigureAwait(false);

            return new OkObjectResult(signatureResult);
        }

        private string GetAuthenticatedUserId()
        {
            string? authenticatedUserId = User.GetUserId();
            if (authenticatedUserId == null)
            {
                throw new ApiException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, "You are not authorized to view this document.");
            }
            return authenticatedUserId;
        }
    }
}

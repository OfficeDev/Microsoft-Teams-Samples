// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Web.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Identity.Web;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Documents;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;
    using Microsoft.Teams.Samples.MeetingSigning.Web.Controllers;
    using Microsoft.Teams.Samples.MeetingSigning.Web.Exceptions;
    using Microsoft.Teams.Samples.MeetingSigning.Web.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DocumentControllerTests
    {
        private readonly Mock<IAuthorizationService> authorizationService;
        private readonly Mock<IDocumentService> documentService;

        public DocumentControllerTests()
        {
            this.authorizationService = new Mock<IAuthorizationService>();
            this.documentService = new Mock<IDocumentService>();
        }

        [TestMethod]
        [ExpectedException(typeof(ApiArgumentException))]
        public async Task DocumentController_GetDocumentAsync_ThrowsApiArgumentException()
        {
            var documentController = new DocumentController(documentService.Object, authorizationService.Object);

            await documentController.GetDocumentAsync(Guid.Empty);
        }

        [TestMethod]
        public async Task DocumentController_GetDocumentAsync_ReturnsExpectedDocument()
        {
            string userId = "001";
            var docId = Guid.NewGuid();
            var document = new Document { DocumentType = "Leasing", Id = docId, OwnerId = $"azureAd://{userId}" };

            var mockedUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimConstants.Oid, userId),
                }, "Test"));

            documentService.Setup(ds => ds.GetDocumentAsync(docId)).ReturnsAsync(document);
            var documentController = new DocumentController(documentService.Object, authorizationService.Object);
            documentController.ControllerContext.HttpContext = new DefaultHttpContext() { User = mockedUser };

            ActionResult<DocumentListDTO>? actionResult = await documentController.GetDocumentAsync(docId).ConfigureAwait(false);
            var result = (actionResult.Result as OkObjectResult);
            Assert.IsNotNull(result);
            var returnedDocument = result.Value as DocumentListDTO;
            Assert.AreEqual(returnedDocument.Documents[0], document);
        }

        [TestMethod]
        public async Task DocumentController_GetDocumentsAsync_ReturnsExpectedDocuments()
        {
            string userId = "001";

            var document = new Document { DocumentType = "Leasing", Id = Guid.Empty, OwnerId = $"azureAd://{userId}" };
            IList<Document> documents = new List<Document> { document };
            documentService.Setup(ds => ds.GetDocumentsAsync(It.IsAny<string>()))
            .ReturnsAsync(documents);

            var mockedUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimConstants.Oid, userId),
                }, "Test"));

            var documentController = new DocumentController(documentService.Object, authorizationService.Object);
            documentController.ControllerContext.HttpContext = new DefaultHttpContext() { User = mockedUser };

            ActionResult<DocumentListDTO>? actionResult = await documentController.GetDocumentsAsync();
            var result = (actionResult.Result as OkObjectResult);
            Assert.IsNotNull(result);
            var returnedDocuments = result.Value as DocumentListDTO;
            Assert.IsNotNull(returnedDocuments);
            Assert.IsTrue(returnedDocuments.Documents.SequenceEqual(documents));
        }

        [TestMethod]
        [ExpectedException(typeof(ApiArgumentNullException))]
        public async Task DocumentController_CreateDocumentsAsync_EmptyDocumentType_ThrowsApiArgumentException()
        {
            var documentInput = new DocumentInput { DocumentType = null, Signers = new List<User>(), Viewers = new List<User>() };
            var documentController = new DocumentController(documentService.Object, authorizationService.Object);
            await documentController.CreateDocumentAsync(documentInput);
        }

        [TestMethod]
        [ExpectedException(typeof(ApiArgumentNullException))]
        public async Task DocumentController_CreateDocumentsAsync_EmptySigners_ThrowsApiArgumentException()
        {
            var documentInput = new DocumentInput { DocumentType = "Test", Signers = default, Viewers = new List<User>() };
            var documentController = new DocumentController(documentService.Object, authorizationService.Object);
            await documentController.CreateDocumentAsync(documentInput);
        }

        [TestMethod]
        public async Task DocumentController_CreateDocumentsAsync_ReturnsExpectedDocuments()
        {
            string userId = "001";
            var document = new Document { DocumentType = "Test", Id = Guid.Empty, OwnerId = $"azureAd://{userId}" };
            var documentInput = new DocumentInput { DocumentType = "Test", Signers = new List<User>(), Viewers = new List<User>() };

            documentService.Setup(ds => ds.CreateDocumentAsync(It.IsAny<DocumentInput>(), It.IsAny<string>()))
            .ReturnsAsync(document);

            var mockedUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimConstants.Oid, userId),
                }, "Test"));

            var documentController = new DocumentController(documentService.Object, authorizationService.Object);
            documentController.ControllerContext.HttpContext = new DefaultHttpContext() { User = mockedUser };

            ActionResult<Document>? actionResult = await documentController.CreateDocumentAsync(documentInput);
        }
    }
}

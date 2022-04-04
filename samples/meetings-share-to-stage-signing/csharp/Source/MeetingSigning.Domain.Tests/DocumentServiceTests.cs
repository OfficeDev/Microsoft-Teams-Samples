// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Documents;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IServices;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DocumentServiceTests
    {
        private readonly Mock<ISignatureRepository> signatureRepository;
        private readonly Mock<IDocumentRepository> documentRepository;
        private readonly Mock<IUserRepository> userRepository;
        private readonly Mock<IViewerRepository> viewerRepository;
        private readonly Mock<IUserService> userService;
        private readonly Mock<ILogger<DocumentService>> logger;

        public DocumentServiceTests()
        {
            this.signatureRepository = new Mock<ISignatureRepository>(MockBehavior.Loose);
            this.documentRepository = new Mock<IDocumentRepository>(MockBehavior.Loose);
            this.userRepository = new Mock<IUserRepository>(MockBehavior.Loose);
            this.viewerRepository = new Mock<IViewerRepository>(MockBehavior.Loose);
            this.userService = new Mock<IUserService>(MockBehavior.Loose);
            this.logger = new Mock<ILogger<DocumentService>>(MockBehavior.Loose);
        }

        [TestMethod]
        public async Task TestCreateDocumentReturnsDefault()
        {
            var documentInput = new DocumentInput
            {
                DocumentType = "Leasing",
                Signers = new List<User> { },
                Viewers = new List<User> { }
            };
            
            var documentService = new Mock<IDocumentService>(MockBehavior.Loose);
            var createdDocument = await documentService.Object.CreateDocumentAsync(documentInput, "001").ConfigureAwait(false);
            
            Assert.AreEqual(createdDocument, default);
        }

        [TestMethod]
        public async Task TestGetDocumentAsyncExpectedDocId()
        {
            var documentId = Guid.NewGuid();
            var document = new Document { Id = documentId };

            this.documentRepository.Setup(dr => dr.GetDocument(It.IsAny<Guid>())).ReturnsAsync(document);
            var documentService = new DocumentService(this.documentRepository.Object, this.signatureRepository.Object, this.userRepository.Object,
            this.viewerRepository.Object,this.userService.Object,  logger.Object);
            var returnedDocument = await documentService.GetDocumentAsync(default);
            Assert.AreEqual(documentId, returnedDocument.Id);
        }
    }
}

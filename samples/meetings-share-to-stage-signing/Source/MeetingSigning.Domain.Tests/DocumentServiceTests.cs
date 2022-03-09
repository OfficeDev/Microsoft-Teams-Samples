// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Documents;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DocumentServiceTests
    {
        private readonly Mock<IDocumentRepository> documentRepository;
        private readonly Mock<ISignatureRepository> signatoryRepository;
        private readonly Mock<IDocumentSetup> documentSetup;
        private readonly Mock<ILogger<DocumentService>> logger;

        public DocumentServiceTests()
        {
            this.documentRepository = new Mock<IDocumentRepository>(MockBehavior.Loose);
            this.signatoryRepository = new Mock<ISignatureRepository>(MockBehavior.Loose);
            this.documentSetup = new Mock<IDocumentSetup>(MockBehavior.Loose);
            this.logger = new Mock<ILogger<DocumentService>>(MockBehavior.Loose);
        }

        [TestMethod]
        public async Task TestCreateDocumentMatchesExpectedDocId()
        {
            var mockDocument = new Mock<Document>().Object;
            var docId = Guid.NewGuid();
            var documentInput = new DocumentInput
            {
                DocumentType = "Leasing",
                Signers = new List<User> { },
                Viewers = new List<User> { }
            };
            var document = new Document
            {
                DocumentType = "Leasing",
                Id = docId,
                OwnerId = "001"
            };

            var documentService = new DocumentService(this.documentRepository.Object,
                this.signatoryRepository.Object, this.documentSetup.Object, logger.Object);

            this.documentSetup.Setup(m => m.AddDocumentAsync(documentInput, "001")).ReturnsAsync(document);            
            var createdDocument = await documentService.CreateDocumentAsync(documentInput, "001").ConfigureAwait(false);
            Assert.AreEqual(docId, createdDocument.Id);
        }
    }
}

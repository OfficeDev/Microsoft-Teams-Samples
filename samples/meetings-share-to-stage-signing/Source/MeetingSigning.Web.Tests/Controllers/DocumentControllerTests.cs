// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.Samples.MeetingSigning.Domain.Documents;
using Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions;
using Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories;
using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;
using Microsoft.Teams.Samples.MeetingSigning.Web.Controllers;
using Microsoft.Teams.Samples.MeetingSigning.Web.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Teams.Samples.MeetingSigning.Web.Tests
{
    [TestClass]
    public class DocumentControllerTests
    {
        private readonly Mock<IDocumentSetup> documentSetup;
        private readonly Mock<ILogger<DocumentService>> logger;
        private readonly Mock<ISignatureRepository> signatureRepository;
        private readonly Mock<IDocumentRepository> documentRepository;
        private readonly Mock<IAuthorizationService> authorizationService;

        public DocumentControllerTests()
        {
            this.documentSetup = new Mock<IDocumentSetup>(MockBehavior.Loose);
            this.logger = new Mock<ILogger<DocumentService>>(MockBehavior.Loose);
            this.signatureRepository = new Mock<ISignatureRepository>(MockBehavior.Loose);
            this.documentRepository = new Mock<IDocumentRepository>(MockBehavior.Loose);
            this.authorizationService = new Mock<IAuthorizationService>(MockBehavior.Loose);
        }

        [TestMethod]
        [ExpectedException(typeof(ApiArgumentException))]
        public async Task DocumentController_GetDocument_ThrowsDocumentNotFoundException()
        {
            var documentService = new Mock<DocumentService>(this.documentRepository.Object,
                 this.signatureRepository.Object, this.documentSetup.Object, logger.Object);

            DocumentController controller = new DocumentController(
                documentService.Object, authorizationService.Object);
            ActionResult<Document> result = await controller.GetDocumentAsync(Guid.Empty);
        }
    }
}

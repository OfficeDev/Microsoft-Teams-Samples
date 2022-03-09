// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.Samples.MeetingSigning.Domain.Documents;
using Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories;
using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;
using Microsoft.Teams.Samples.MeetingSigning.Web.Controllers;
using Microsoft.Teams.Samples.MeetingSigning.Web.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Teams.Samples.MeetingSigning.Web.Tests
{
    [TestClass]
    public class SignDocumentTests
    {
        private readonly Mock<Signature> signature;
        private readonly Mock<IAuthorizationService> authorizationService;
        private readonly Mock<IDocumentSetup> documentSetup;

        public SignDocumentTests()
        {
            signature = new Mock<Signature>();
            authorizationService = new Mock<IAuthorizationService>();
            documentSetup = new Mock<IDocumentSetup>();
        }

        [TestMethod]
        [ExpectedException(typeof(ApiArgumentNullException))]
        public async Task SignDocument_DocumentIdIsNull_ThrowsApiArgumentNullException()
        {
            DocumentController controller = new DocumentController(new DocumentService(new Mock<IDocumentRepository>().Object, new Mock<ISignatureRepository>().Object, documentSetup.Object, new Mock<ILogger<DocumentService>>().Object), authorizationService.Object);
            Signature signature = new Mock<Signature>().Object;

            ActionResult<Signature> result = await controller.SignDocumentAsync(null!, signature);
        }

        [TestMethod]
        [ExpectedException(typeof(ApiArgumentNullException))]
        public async Task SignDocument_SignatureIsNull_ThrowsApiArgumentNullException()
        {
            DocumentController controller = new DocumentController(new DocumentService(new Mock<IDocumentRepository>().Object, new Mock<ISignatureRepository>().Object, documentSetup.Object, new Mock<ILogger<DocumentService>>().Object), authorizationService.Object);

            ActionResult<Signature> result = await controller.SignDocumentAsync("docId", null!);
        }

        [TestMethod]
        [ExpectedException(typeof(ApiArgumentException))]
        public async Task SignDocument_DocumentIdIsNotGuid_ThrowsApiArgumentException()
        {
            DocumentController controller = new DocumentController(new DocumentService(new Mock<IDocumentRepository>().Object, new Mock<ISignatureRepository>().Object, documentSetup.Object, new Mock<ILogger<DocumentService>>().Object), authorizationService.Object);

            ActionResult<Signature> result = await controller.SignDocumentAsync("not-a-guid", signature.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ApiArgumentException))]
        public async Task SignDocument_DocumentIdIsEmpty_ThrowsApiArgumentException()
        {
            DocumentController controller = new DocumentController(new DocumentService(new Mock<IDocumentRepository>().Object, new Mock<ISignatureRepository>().Object, documentSetup.Object, new Mock<ILogger<DocumentService>>().Object), authorizationService.Object);

            ActionResult<Signature> result = await controller.SignDocumentAsync(string.Empty, signature.Object);
        }
    }
}

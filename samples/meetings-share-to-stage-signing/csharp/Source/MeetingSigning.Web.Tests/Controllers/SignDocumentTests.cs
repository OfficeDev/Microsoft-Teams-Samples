// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Teams.Samples.MeetingSigning.Domain.Documents;
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
        private readonly Mock<IDocumentService> documentService;

        public SignDocumentTests()
        {
            signature = new Mock<Signature>();
            authorizationService = new Mock<IAuthorizationService>();
            this.documentService = new Mock<IDocumentService>();
        }

        [TestMethod]
        [ExpectedException(typeof(ApiArgumentNullException))]
        public async Task SignDocument_DocumentIdIsNull_ThrowsApiArgumentNullException()
        {
            DocumentController controller = new DocumentController(documentService.Object, authorizationService.Object);
            Signature signature = new Mock<Signature>().Object;

            ActionResult<Signature> result = await controller.SignDocumentAsync(null!, signature);
        }

        [TestMethod]
        [ExpectedException(typeof(ApiArgumentNullException))]
        public async Task SignDocument_SignatureIsNull_ThrowsApiArgumentNullException()
        {
            DocumentController controller = new DocumentController(documentService.Object, authorizationService.Object);

            ActionResult<Signature> result = await controller.SignDocumentAsync("docId", null!);
        }

        [TestMethod]
        [ExpectedException(typeof(ApiArgumentException))]
        public async Task SignDocument_DocumentIdIsNotGuid_ThrowsApiArgumentException()
        {
            DocumentController controller = new DocumentController(documentService.Object, authorizationService.Object);

            ActionResult<Signature> result = await controller.SignDocumentAsync("not-a-guid", signature.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ApiArgumentException))]
        public async Task SignDocument_DocumentIdIsEmpty_ThrowsApiArgumentException()
        {
            DocumentController controller = new DocumentController(documentService.Object, authorizationService.Object);

            ActionResult<Signature> result = await controller.SignDocumentAsync(string.Empty, signature.Object);
        }

        [TestMethod]
        public async Task SignDocument_ReturnsExpectedSignature()
        {
            var docId = Guid.NewGuid();
            var signId = Guid.NewGuid();
            var signature = new Signature { Id = signId, IsSigned = true };
            documentService.Setup(ds => ds.SignDocumentAsync(docId, signature)).ReturnsAsync(signature);

            DocumentController controller = new DocumentController(documentService.Object, authorizationService.Object);
            ActionResult<Signature>? actionResult = await controller.SignDocumentAsync(docId.ToString(), signature);
            var result = (actionResult.Result as OkObjectResult);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Value, signature);
        }
    }
}

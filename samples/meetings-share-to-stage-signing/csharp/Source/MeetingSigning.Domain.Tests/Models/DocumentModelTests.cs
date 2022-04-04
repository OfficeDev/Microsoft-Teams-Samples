// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DocumentModelTests
    {
        [TestMethod]
        public async Task DocumentModelDocumentStateIsActiveWithActiveSigners()
        {
            var signature = new Signature { Id = Guid.NewGuid() };
            //default isSigned is false
            Assert.IsFalse(signature.IsSigned);
            var document = new Document { Id = default, Signatures = new List<Signature> { signature }, Viewers = new List<Viewer>() };
            Assert.AreEqual(document.DocumentState, DocumentState.Active);
        }

        [TestMethod]
        public async Task DocumentModelDocumentStateIsCompleteWithSigners()
        {
            var signature = new Signature { Id = Guid.NewGuid(), IsSigned = true };
            var document = new Document { Id = default, Signatures = new List<Signature> { signature }, Viewers = new List<Viewer>() };
            Assert.AreEqual(document.DocumentState, DocumentState.Complete);
        }

        [TestMethod]
        public async Task DocumentModelDocumentDeepCopyValuesMatch()
        {
            var signature = new Signature { Id = Guid.NewGuid(), IsSigned = true, Signer = new User { Name = "TestUser"} };
            var document = new Document { Id = default, Signatures = new List<Signature> { signature }, Viewers = new List<Viewer>() };
            var documentCopy = document.DeepCopy();
            Assert.AreNotSame(documentCopy, document);
            Assert.AreEqual(document.Id, documentCopy.Id);
            Assert.AreEqual(document.DocumentType, documentCopy.DocumentType);
            Assert.AreEqual(document.OwnerId, documentCopy.OwnerId);
            Assert.AreEqual(document.Signatures.First().Id, documentCopy.Signatures.First().Id);
            Assert.AreEqual(document.Signatures.First().Signer.Name, documentCopy.Signatures.First().Signer.Name);
            Assert.AreEqual(document.Signatures.First().IsSigned, documentCopy.Signatures.First().IsSigned);
        }
    }
}

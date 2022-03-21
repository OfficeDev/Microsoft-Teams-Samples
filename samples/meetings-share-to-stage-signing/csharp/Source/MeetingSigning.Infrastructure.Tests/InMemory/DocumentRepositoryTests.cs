// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;
using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Data.Repositories.InMemory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Tests.InMemory
{
    [TestClass]
    public class DocumentRepositoryTests
    {

        [TestMethod]
        public async Task DocumentRepositoryTestsCreateDocumentCreatesNewGuid()
        {
            var inputDocumentId = Guid.NewGuid();
            var inputDocument = new Document { Id = inputDocumentId };
            var documentRepository = new DocumentRepository();
            var createdDocument = await documentRepository.CreateDocument(inputDocument);
            Assert.AreNotEqual(inputDocumentId, createdDocument.Id);
        }

        [TestMethod]
        public async Task DocumentRepositoryTestsCreateDocumentMatchesTheInput()
        {
            var signature = new Signature { Id = Guid.NewGuid(), IsSigned = false };
            var inputDocument = new Document { DocumentType = "Test", Signatures = new List<Signature> { signature }, OwnerId = "10112" };
            var documentRepository = new DocumentRepository();
            Document? createdDocument = await documentRepository.CreateDocument(inputDocument);

            Assert.IsNotNull(createdDocument.Id);
            Assert.AreEqual(inputDocument.DocumentType, createdDocument.DocumentType);
            Assert.AreEqual(inputDocument.Signatures, createdDocument.Signatures);
            Assert.AreEqual(inputDocument.OwnerId, createdDocument.OwnerId);
        }

        [TestMethod]
        public async Task DocumentRepositoryTestsGetDocumentGetsExpectedCreatedDocument()
        {
            var signature = new Signature { Id = Guid.NewGuid(), IsSigned = false, Signer = new User { UserId = "333343" } };
            var inputDocument = new Document { DocumentType = "Test", Signatures = new List<Signature> { signature }, OwnerId = "10112" };
            var documentRepository = new DocumentRepository();
            Document? createdDocument = await documentRepository.CreateDocument(inputDocument).ConfigureAwait(false);

            Document? retrievedDocument = await documentRepository.GetDocument(createdDocument.Id).ConfigureAwait(false);

            Assert.AreEqual(retrievedDocument.DocumentType, createdDocument.DocumentType);
            Assert.AreEqual(retrievedDocument.OwnerId, createdDocument.OwnerId);
            Assert.AreEqual(retrievedDocument.Signatures.First().Id, createdDocument.Signatures.First().Id);
            Assert.AreEqual(retrievedDocument.Signatures.First().IsSigned, createdDocument.Signatures.First().IsSigned);
            Assert.AreEqual(retrievedDocument.Signatures.First().Signer.UserId, createdDocument.Signatures.First().Signer.UserId);
        }

        [TestMethod]
        public async Task DocumentRepositoryTestsGetDocumentsGetExpectedCreatedDocuments()
        {
            var signature = new Signature { Id = Guid.NewGuid(), IsSigned = false, Signer = new User { UserId = "333343" } };
            var signature2 = new Signature { Id = Guid.NewGuid(), IsSigned = true, Signer = new User { UserId = "3333432" } };
            string? ownerId = "10112";

            var inputDocument = new Document { DocumentType = "Test", Signatures = new List<Signature> { signature }, OwnerId = ownerId };
            var inputDocument2 = new Document { DocumentType = "Test2", Signatures = new List<Signature> { signature2 }, OwnerId = ownerId };

            var documentRepository = new DocumentRepository();
            Document? createdDocument = await documentRepository.CreateDocument(inputDocument);
            Document? createdDocument2 = await documentRepository.CreateDocument(inputDocument2);

            IList<Document>? retrievedDocuments = await documentRepository.GetDocuments(ownerId).ConfigureAwait(false);

            Assert.AreEqual(retrievedDocuments.Count, 2);
            Assert.AreEqual(retrievedDocuments[0].OwnerId, inputDocument.OwnerId);
            Assert.AreEqual(retrievedDocuments[1].OwnerId, inputDocument.OwnerId);

            if(retrievedDocuments[0].Id == createdDocument.Id)
            {
                Assert.AreEqual(retrievedDocuments[0].DocumentType, createdDocument.DocumentType);
                Assert.AreEqual(retrievedDocuments[0].OwnerId, createdDocument.OwnerId);
                Assert.AreEqual(retrievedDocuments[0].Signatures.First().Id, createdDocument.Signatures.First().Id);
                Assert.AreEqual(retrievedDocuments[0].Signatures.First().IsSigned, createdDocument.Signatures.First().IsSigned);
                Assert.AreEqual(retrievedDocuments[0].Signatures.First().Signer.UserId, createdDocument.Signatures.First().Signer.UserId);
            }
            else
            {
                Assert.AreEqual(retrievedDocuments[1].DocumentType, createdDocument2.DocumentType);
                Assert.AreEqual(retrievedDocuments[1].OwnerId, createdDocument2.OwnerId);
                Assert.AreEqual(retrievedDocuments[1].Signatures.First().Id, createdDocument2.Signatures.First().Id);
                Assert.AreEqual(retrievedDocuments[1].Signatures.First().IsSigned, createdDocument2.Signatures.First().IsSigned);
                Assert.AreEqual(retrievedDocuments[1].Signatures.First().Signer.UserId, createdDocument2.Signatures.First().Signer.UserId);
            }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions;
using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;
using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Data.Repositories.InMemory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Tests.InMemory
{
    [TestClass]
    public class SignatureRepositoryTests
    {

        [TestMethod]
        public async Task SignatureRepositoryTestsAddSignatureCreatesNewGuid()
        {
            var inputSignatureId = Guid.NewGuid();
            var inputSignature = new Signature { Id = inputSignatureId };
            var signatureRepository = new SignatureRepository();
            Signature? addedSignature = await signatureRepository.AddSignature(inputSignature);
            Assert.AreNotEqual(inputSignatureId, addedSignature.Id);
            Assert.IsFalse(addedSignature.IsSigned);
        }

        [TestMethod]
        [ExpectedException(typeof(ApiException))]
        public async Task SignatureRepositoryTestsGetSignatureThrowsApiExceptionForInvalidInput()
        {
            var inputSignature = new Signature { Signer = new User { UserId = "007007"} };
            var signatureRepository = new SignatureRepository();
            Signature? addedSignature = await signatureRepository.AddSignature(inputSignature);
            Assert.IsNotNull(addedSignature);

            await signatureRepository.GetSignature(Guid.NewGuid());
        }

        [TestMethod]
        public async Task SignatureRepositoryTestsGetSignatureRetrievesMatchingRecordForValidInput()
        {
            var inputSignature = new Signature { Signer = new User { UserId = "007007", Name = "Bill" } };
            var signatureRepository = new SignatureRepository();
            Signature? addedSignature = await signatureRepository.AddSignature(inputSignature);
            Signature? retrievedSignature = await signatureRepository.GetSignature(addedSignature.Id);

            Assert.IsNotNull(retrievedSignature);
            Assert.AreEqual(addedSignature.Id, retrievedSignature.Id);
            Assert.IsTrue(addedSignature.Signer.Equals(retrievedSignature.Signer));
        }

        [TestMethod]
        [ExpectedException(typeof(ApiException))]
        public async Task SignatureRepositoryTestsUpdateSignatureThrowsApiExceptionForInvalidId()
        {
            var inputSignature = new Signature { Signer = new User { UserId = "007007", Name = "Bill" } };
            var signatureRepository = new SignatureRepository();
            await signatureRepository.UpdateSignature(inputSignature);
        }

        [TestMethod]
        [ExpectedException(typeof(ApiException))]
        public async Task SignatureRepositoryTestsUpdateSignatureThrowsApiExceptionForSingingComplete()
        {
            var inputSignature = new Signature { Signer = new User { UserId = "007007", Name = "Bill"}, IsSigned = true };
            var signatureRepository = new SignatureRepository();
            Signature? addedSignature = await signatureRepository.AddSignature(inputSignature);

            Assert.IsNotNull(addedSignature);

            var updateSignatureInput = new Signature { Signer = new User { UserId = "007007", Name = "Bill" }, IsSigned = false };
            await signatureRepository.UpdateSignature(updateSignatureInput);
        }

        [TestMethod]
        public async Task SignatureRepositoryTestsUpdateSignatureWorksForValidInput()
        {
            var inputSignature = new Signature { Signer = new User { UserId = "007007", Name = "Bill" } };
            var signatureRepository = new SignatureRepository();
            Signature? addedSignature = await signatureRepository.AddSignature(inputSignature);
            bool initialSigningStatus = addedSignature.IsSigned;

            Assert.IsNotNull(addedSignature);
            var updateSignatureInput = new Signature {Id = addedSignature.Id, Signer = new User { UserId = "007007", Name = "Bill G" }, IsSigned = true, SignedDateTime = DateTime.UtcNow, Text = "Bill G"};
            Signature? updatedSignature = await signatureRepository.UpdateSignature(updateSignatureInput);

            Assert.AreEqual(updateSignatureInput.Id, updatedSignature.Id);
            Assert.AreEqual(updatedSignature.SignedDateTime, updateSignatureInput.SignedDateTime);
            Assert.AreEqual(updatedSignature.Text, updateSignatureInput.Text);

            // Signer shouldn't be updated in SingatureRepository
            Assert.IsTrue(addedSignature.Signer.Equals(updateSignatureInput.Signer));
            Assert.IsTrue(updatedSignature.IsSigned);
            Assert.AreNotEqual(updatedSignature.IsSigned, initialSigningStatus);
        }
    }
}

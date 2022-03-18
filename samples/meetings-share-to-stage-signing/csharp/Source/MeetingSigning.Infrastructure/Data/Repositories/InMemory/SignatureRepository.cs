// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Data.Repositories.InMemory
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    /// <summary>
    /// SignatureRepository is an inmemory implementation of ISignatureRepository
    /// </summary>
    public class SignatureRepository : ISignatureRepository
    {
        private readonly IDictionary<Guid, Signature> signatureDictionary;

        public SignatureRepository()
        {
            this.signatureDictionary = new Dictionary<Guid, Signature>();
        }

        /// <summary>
        /// AddSignature inserts a new Signature into the store
        /// </summary>
        /// <param name="signature"></param>
        /// <returns>Signature</returns>
        public async Task<Signature> AddSignature(Signature signature)
        {
            if (this.signatureDictionary.TryGetValue(signature.Id, out Signature existingSignature))
            {
                throw new ApiException(HttpStatusCode.NotFound, ErrorCode.SignatureNotFound, "Signature was not found.");
            }
            signature.Id = Guid.NewGuid();
            this.signatureDictionary.Add(signature.Id, signature);
            return signature;
        }

        /// <summary>
        /// GetSignature tries to find an existing signature
        /// </summary>
        /// <param name="signatureId"></param>
        /// <returns>Signature found or null</returns>
        public async Task<Signature> GetSignature(Guid signatureId)
        {
            if (this.signatureDictionary.TryGetValue(signatureId, out Signature existingSignature))
            {
                return existingSignature.DeepCopy();
            }
            throw new ApiException(HttpStatusCode.NotFound, ErrorCode.SignatureNotFound, "Signature was not found.");
        }

        /// <summary>
        /// UpdateSignature updates if a signature exists
        /// </summary>
        /// <param name="signature"></param>
        /// <returns>The signed signature</returns>
        /// <exception cref="ApiException">
        /// If the Id of the <paramref name="signature"/> does not match a Signature in the database.
        /// Or if the signature is already signed.
        ///</exception>
        public async Task<Signature> UpdateSignature(Signature signature)
        {
            if (!signatureDictionary.TryGetValue(signature.Id, out Signature existingSignature))
            {
                throw new ApiException(HttpStatusCode.NotFound, ErrorCode.SignatureNotFound, "Signature was not found.");
            }
            else if (existingSignature.IsSigned)
            {
                throw new ApiException(HttpStatusCode.UnprocessableEntity, ErrorCode.SignatureAlreadySigned, "Signature is already signed.");
            }

            existingSignature.Text = signature.Text;
            existingSignature.IsSigned = signature.IsSigned;
            existingSignature.SignedDateTime = signature.SignedDateTime;

            return existingSignature;
        }
    }
}

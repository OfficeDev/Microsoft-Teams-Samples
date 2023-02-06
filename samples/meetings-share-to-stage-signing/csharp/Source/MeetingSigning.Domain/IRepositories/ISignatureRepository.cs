// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    /// <summary>
    /// Repository for accessing, setting, modifying and other Signatures related data management
    /// </summary>
    public interface ISignatureRepository
    {
        /// <summary>
        /// Gets a Signature from the repository
        /// </summary>
        /// <param name="signatureId">The id of the signature, stored in the Id Property</param>
        /// <returns><see cref="SignatureEntity"/> associated with the <paramref name="signatureId"/></returns>
        Task<Signature> GetSignature(Guid signatureId);

        /// <summary>
        /// Takes a Signature, and updates it's value
        /// </summary>
        /// <param name="signature">Signature to replace. Will update the signature with the same id.</param>
        /// <returns>The signed signature</returns>
        Task<Signature> UpdateSignature(Signature signature);

        /// <summary>
        /// AddSignature inserts a given signature in the database
        /// </summary>
        /// <param name="signature"></param>
        /// <returns>Signature thus created</returns>
        Task<Signature> AddSignature(Signature signature);
    }
}

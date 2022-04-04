// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Data.Repositories
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entities;

    /// <inheritdoc />
    public class SignatureRepository : ISignatureRepository
    {
        private readonly MeetingSigningDbContext dbContext;
        private readonly IMapper mapper;

        public SignatureRepository(MeetingSigningDbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Gets a Signature from the database
        /// </summary>
        /// <param name="signatureId">The id of the signature, stored in the Id Property</param>
        /// <returns><see cref="SignatureEntity"/> associated with the <paramref name="signatureId"/></returns>
        /// <exception cref="ApiException">If the <paramref name="signatureId"/> does not match that of a Signature in the database.</exception>
        public async Task<Signature> GetSignature(Guid signatureId)
        {
            var signatureEntity = await dbContext.Signatures
                .FirstOrDefaultAsync(c => c.Id.Equals(signatureId));

            if (signatureEntity == null)
            {
                throw new ApiException(HttpStatusCode.NotFound, ErrorCode.SignatureNotFound, $"Signature '{signatureId}' was not found.");
            }
            return this.mapper.Map<Signature>(signatureEntity);
        }

        /// <summary>
        /// Takes a Signature, and updates it's value in the database
        /// </summary>
        /// <param name="signature">Signature to replace. Will update the signature with the same id.</param>
        /// <returns>The signed signature</returns>
        /// <exception cref="ApiException">
        /// If the Id of the <paramref name="signature"/> does not match a Signature in the database.
        /// Or if the signature is already signed.
        ///</exception>
        public async Task<Signature> UpdateSignature(Signature signature)
        {
            try
            {
                var signatureEntity = await dbContext.Signatures
                    .FirstOrDefaultAsync(c => c.Id.Equals(signature.Id));

                if (signatureEntity == null)
                {
                    throw new ApiException(HttpStatusCode.NotFound, ErrorCode.SignatureNotFound, $"Signature '{signature.Id}' was not found.");
                }
                else if (signatureEntity.IsSigned)
                {
                    throw new ApiException(HttpStatusCode.UnprocessableEntity, ErrorCode.SignatureAlreadySigned, "Signature is already signed.");
                }

                signatureEntity.Text = signature.Text;
                signatureEntity.SignedDateTime = signature.SignedDateTime;
                signatureEntity.IsSigned = signature.IsSigned;
                await dbContext.SaveChangesAsync();

                return mapper.Map<Signature>(signatureEntity);
            }
            catch (DbUpdateException exception)
            {
                string message = $"Failed to update signature: {signature.Id}";
                throw new ApiException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <summary>
        /// Takes a Signature, and adds it's value in the database
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        public async Task<Signature> AddSignature(Signature signature)
        {
            try
            {
                var userEntity = await this.dbContext.Users.FirstOrDefaultAsync( s=>s.UserId.Equals(signature.Signer.UserId));
                if (userEntity == null)
                {
                    throw new ApiException(HttpStatusCode.NotFound, ErrorCode.UserNotFound, $"User '{signature.Signer.UserId}' was not found.");
                }
                var signatureEntity = this.mapper.Map<SignatureEntity>(signature);
                signatureEntity.Signer = userEntity;
                dbContext.Signatures.Add(signatureEntity);
                await dbContext.SaveChangesAsync();
                return this.mapper.Map<Signature>(signatureEntity);
            }
            catch(Exception exception)
            {
                string message = $"Failed to add signature for user: {signature.Signer.UserId}";
                throw new ApiException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }
    }
}

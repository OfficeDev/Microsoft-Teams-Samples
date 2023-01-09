// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entities
{
    /// <summary>
    /// Model for a Signature, shared between EF and a requests
    /// </summary>
    public class SignatureEntity
    {
        /// <summary>
        /// The ID of the Signature
        /// </summary>
        /// <remarks>This will be created automatically by Entity Framework if not provided</remarks>

        public Guid Id { get; set; }

        /// <summary>
        /// Signer entity that contains information like AzureAD Object ID of the Signer
        /// </summary>
        public UserEntity Signer { get; set; }

        /// <summary>
        /// DateTime of the Signature, this will be set by the server
        /// </summary>
        public DateTime SignedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Text that makes up the signature. This may be stylised when displayed.
        /// </summary>
        /// <remarks>This value is null if the signature is not signed</remarks>
        public string? Text { get; set; }

        /// <summary>
        /// Flag indicating if the signature has been signed
        /// </summary>
        public bool IsSigned { get; set; }
    }
}

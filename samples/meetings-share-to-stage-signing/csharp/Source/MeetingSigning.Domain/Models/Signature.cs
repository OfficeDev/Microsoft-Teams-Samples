// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Models
{
    /// <summary>
    /// Model for a Signature
    /// </summary>
    public class Signature
    {
        /// <summary>
        /// The ID of the Signature
        /// </summary>
        /// <remarks>This will be created automatically by Entity Framework if not provided</remarks>

        public Guid Id { get; set; }

        /// <summary>
        /// Signer object that contains information like AzureAD User UserId of the Signer
        /// </summary>
        public User Signer { get; set; }

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

        public Signature DeepCopy()
        {
            Signature other = (Signature)this.MemberwiseClone();
            other.Signer = this.Signer.DeepCopy();
            return other;
        }
    }
}

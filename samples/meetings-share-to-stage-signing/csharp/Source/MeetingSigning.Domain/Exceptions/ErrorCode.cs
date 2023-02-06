// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions
{
    /// <summary>
    /// Common error codes.
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>
        /// Unknown error.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Unauthorized: When a valid Auth token is not provided
        /// </summary>
        Unauthorized,

        /// <summary>
        /// Forbidden: A valid Auth token was provided, but it does not have permission to access the desired resource
        /// </summary>
        Forbidden,

        /// <summary>
        /// When client performs invalid operation.
        /// </summary>
        InvalidOperation,

        /// <summary>
        /// A provided argument is not valid
        /// </summary>
        ArgumentNotValid,

        /// <summary>
        /// Signer/Viewer not found.
        /// </summary>
        UserNotFound,

        /// <summary>
        /// Document not found.
        /// </summary>
        DocumentNotFound,

        /// <summary>
        /// Signature not found.
        /// </summary>
        SignatureNotFound,

        /// <summary>
        /// Signature is signed already.
        /// </summary>
        SignatureAlreadySigned,

        /// <summary>
        /// Consent required to make authenticated call to Graph
        /// </summary>
        AuthConsentRequired,

        /// <summary>
        /// Graph Service Exception.
        /// </summary>
        GraphServiceException,
    }
}

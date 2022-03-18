// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Web.Authorization
{
    /// <summary>
    /// Authorization policies.
    /// </summary>
    public static class AuthZPolicy
    {
        /// <summary>
        /// Authorization policy to authorize a user to get a single document.
        /// </summary>
        public const string GetDocumentPolicy = "GetDocumentPolicy";

        /// <summary>
        /// Authorization policy to authorize a signer to sign a document.
        /// </summary>
        public const string SignDocumentPolicy = "SignDocumentPolicy";
    }
}

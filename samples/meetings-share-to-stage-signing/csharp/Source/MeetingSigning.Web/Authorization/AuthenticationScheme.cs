// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Web.Authorization
{
    /// <summary>
    /// Authentication schemes
    /// </summary>
    public static class AuthenticationScheme
    {
        /// <summary>
        /// Authentication scheme for Azure AD/MSAL. This is the default scheme.
        /// </summary>
        public const string Aad = "AAD";

        /// <summary>
        /// Authentication scheme for Sign in with Microsoft (MSA). Used for Anonymous users
        /// </summary>
        public const string Msa = "MSA";

        /// <summary>
        /// Authentication scheme for both AzureAD and Msa.
        /// </summary>
        public const string AadAndMsa = Aad + "," + Msa;
    }
}

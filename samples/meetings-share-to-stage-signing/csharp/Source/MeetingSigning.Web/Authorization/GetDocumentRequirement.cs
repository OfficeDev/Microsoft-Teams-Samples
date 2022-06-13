// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Web.Authorization
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Requirement authorizing users to read a document.
    /// Viewers and Signers are allowed to read a document.
    /// </summary>
    public class GetDocumentRequirement : IAuthorizationRequirement
    {
    }
}

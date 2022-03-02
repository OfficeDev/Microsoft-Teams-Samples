// <copyright file="SetupResponse.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Web.Controllers
{
    /// <summary>
    /// Review setup response object.
    /// </summary>
    public class SetupResponse
    {
        /// <summary>
        /// Join meeting link.
        /// </summary>
        public string JoinMeetingLink { get; set; }

        /// <summary>
        /// Meeting chat tab id.
        /// </summary>
        public string TabId { get; set; }
    }
}

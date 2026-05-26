// <copyright file="RequestInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabRequestApproval.Model
{
    using System;

    // Class with request info model.
    public class RequestInfo
    {
        /// <summary>
        /// Gets or sets the task id (request id).
        /// </summary>
        public string taskId { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        /// Gets or sets the personal name.
        /// </summary>
        public string personaName { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// Gets or sets the teams app installation scope id.
        /// </summary>
        public string teamsAppInstallationScopeId { get; set; }
    }
}
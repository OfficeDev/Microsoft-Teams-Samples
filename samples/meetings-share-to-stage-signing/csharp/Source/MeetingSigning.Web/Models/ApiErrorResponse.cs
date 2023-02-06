// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Web.Models
{
    using System;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions;

    /// <summary>
    /// Model containing Error details that is returned if any error occurs on the server
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>
        /// Human readable message explaining the error. This should not expose any internal logic
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Enum of possible errors, that can be used for client side error handling
        /// </summary>
        public ErrorCode ErrorCode { get; set; }

        /// <summary>
        /// DateTime that the error occurred
        /// </summary>
        public DateTime Time { get; set; } = DateTime.UtcNow;
    }
}

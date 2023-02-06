// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Web.Models;

using System;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Exceptions;

/// <summary>
/// Model containing Error details that is returned if any error occurs on the server
/// </summary>
public record ApiErrorResponse(
        /// <summary>
        /// Enum of possible errors, that can be used for client side error handling
        /// </summary>
        ErrorCode ErrorCode,

        /// <summary>
        /// DateTime that the error occurred
        /// </summary>
        DateTime Time,

        /// <summary>
        /// Human readable message explaining the error. This should not expose any internal logic
        /// </summary>
        string? Message
);

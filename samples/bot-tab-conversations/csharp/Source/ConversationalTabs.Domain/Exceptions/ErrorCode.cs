// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Exceptions;

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
    /// Consent required to make authenticated call to Graph
    /// </summary>
    AuthConsentRequired,

    /// <summary>
    /// Graph Service Exception.
    /// </summary>
    GraphServiceException,

    /// <summary>
    /// We were unable to find the requested Category.
    /// </summary>
    CategoryNotFound,

    /// <summary>
    /// We were unable to find the requested Item.
    /// </summary>
    ItemNotFound,

    /// <summary>
    /// We were unable to find the requested Channel's Activity. This can occur if the bot is not registered in this Channel.
    /// </summary>
    ChannelActivityNotFound,
}

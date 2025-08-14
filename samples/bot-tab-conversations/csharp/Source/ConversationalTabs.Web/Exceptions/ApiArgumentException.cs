﻿// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Web.Exceptions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Exceptions;

/// <summary>
/// The exception that is thrown when one of the arguments provided to an API call is not valid.
/// Throw this error only if you are ok with this information being shown to the user.
/// <summary>
/// <remarks>In <see cref="Middleware.JsonExceptionHandler" /> this exception leads to a 400 BadRequest error being returned to the user.</remarks>
public class ApiArgumentException : ApiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiArgumentException" /> class.
    /// </summary>
    public ApiArgumentException()
        : base(HttpStatusCode.BadRequest, ErrorCode.ArgumentNotValid)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiArgumentException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">
    /// The error message that explains the reason for the exception.
    /// ON EXCEPTION, THIS MESSAGE WILL BE SHOWN TO THE USER.
    /// </param>
    public ApiArgumentException(string message)
        : base(HttpStatusCode.BadRequest, ErrorCode.ArgumentNotValid, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiArgumentException" /> class with a specified
    /// error message and the name of the parameter that causes this exception.
    /// </summary>
    /// <param name="message">
    /// The error message that explains the reason for the exception.
    /// ON EXCEPTION, THIS MESSAGE WILL BE SHOWN TO THE USER.
    /// </param>
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    public ApiArgumentException(string message, string paramName)
        : base(HttpStatusCode.BadRequest, ErrorCode.ArgumentNotValid, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiArgumentException" /> class with a specified
    /// error message and a reference to the inner exception that is the cause of this
    /// exception.
    /// </summary>
    /// <param name="message">
    /// The error message that explains the reason for the exception.
    /// ON EXCEPTION, THIS MESSAGE WILL BE SHOWN TO THE USER.
    /// </param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception. If the innerException
    /// parameter is not a null reference, the current exception is raised in a catch
    /// block that handles the inner exception.
    /// </param>
    public ApiArgumentException(string message, Exception? innerException)
        : base(HttpStatusCode.BadRequest, ErrorCode.ArgumentNotValid, message, innerException)
    {
    }

    /// <summary>
    /// Throws an System.ArgumentNullException if argument is null or Throws an System.ArgumentException if argument is Empty.
    /// </summary>
    /// <param name="argument">The string argument to validate as non-null or empty.</param>
    /// <param name="paramName">The name of the parameter with which argument corresponds.</param>
    /// <exception cref="ApiArgumentNullException">argument is null</exception>
    /// <exception cref="ApiArgumentException">argument is empty</exception>
    public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression("argument")] string? paramName = null)
    {
        if (argument is null)
        {
            throw new ApiArgumentNullException($"'{paramName}' cannot be null", paramName);
        }

        if (string.IsNullOrEmpty(argument))
        {
            throw new ApiArgumentException($"'{paramName}' cannot be empty", paramName);
        }
    }
}

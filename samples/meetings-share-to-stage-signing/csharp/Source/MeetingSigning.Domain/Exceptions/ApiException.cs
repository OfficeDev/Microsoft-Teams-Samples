// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions
{
    using System;
    using System.Net;

    /// <summary>
    /// The exception that is thrown when one of the arguments provided to an API call is not valid.
    /// Throw this error only if you are ok with this information being shown to the user.
    /// <summary>
    /// <remarks>In <see cref="Middleware.JsonExceptionHandler" /> this exception leads to a <see cred="StatusCode"> being returned to the user.</remarks>
    public class ApiException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException" /> class.
        /// </summary>
        public ApiException()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class.
        /// </summary>
        /// <param name="statusCode">Http Status code.</param>
        /// <param name="code">Error Code.</param>
        public ApiException(HttpStatusCode statusCode, ErrorCode code)
            : this(statusCode, code, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// ON EXCEPTION, THIS MESSAGE WILL BE SHOWN TO THE USER.
        /// </param>
        public ApiException(string message)
            : this(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException" /> class with a specified
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
        public ApiException(string message, Exception innerException)
            : this(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class.
        /// </summary>
        /// <param name="statusCode">Http Status code.</param>
        /// <param name="code">Error Code.</param>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// ON EXCEPTION, THIS MESSAGE WILL BE SHOWN TO THE USER.
        /// </param>
        /// <param name="innerException">Inner Exception (Optional).</param>
        public ApiException(HttpStatusCode statusCode, ErrorCode code, string message, Exception? innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            Code = code;
        }

        /// <summary>
        /// Http Status Code that will be returned to the user when this exception is hit
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Error code
        /// </summary>
        public ErrorCode Code { get; }
    }
}

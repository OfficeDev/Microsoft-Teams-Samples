// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Web.Exceptions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// The exception that is thrown when one of the arguments provided to an API call is null.
    /// Throw this error only if you are ok with this information being shown to the user.
    /// <summary>
    /// <remarks>In <see cref="Middleware.JsonExceptionHandler" /> this exception leads to a 400 BadRequest error being returned to the user.</remarks>
    public class ApiArgumentNullException : ApiArgumentException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiArgumentNullException" /> class
        /// </summary>
        public ApiArgumentNullException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiArgumentNullException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// ON EXCEPTION, THIS MESSAGE WILL BE SHOWN TO THE USER.
        /// </param>
        public ApiArgumentNullException(string message) : base(message)
        {
        }

        //
        /// <summary>
        /// Initializes an instance of the <see cref="ApiArgumentNullException" /> class with a specified
        /// error message and the name of the parameter that causes this exception.
        /// </summary>
        /// <param name="message">
        /// A message that describes the error.
        /// ON EXCEPTION, THIS MESSAGE WILL BE SHOWN TO THE USER.
        ///</param>
        /// <param name="paramName">The name of the parameter that caused the exception.</param>
        public ApiArgumentNullException(string message, string paramName) : base(message, paramName)
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
        public ApiArgumentNullException(string message, Exception? innerException = null) : base(message, innerException)
        {
        }

        /// <summary>
        /// Throws an System.ArgumentNullException if argument is null.
        /// </summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which argument corresponds.</param>
        /// <exception cref="ApiArgumentNullException">argument is null</exception>
        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression("argument")] string? paramName = null)
        {
            if (argument is null)
            {
                throw new ApiArgumentNullException($"'{paramName}' cannot be null", paramName);
            }
        }
    }
}

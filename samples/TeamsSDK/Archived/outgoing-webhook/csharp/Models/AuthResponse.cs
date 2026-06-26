// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

namespace WebhookSampleBot.Models
{
    /// <summary>
    /// Encapsulates auth results.
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthResponse"/> class.
        /// </summary>
        /// <param name="authSuccessful">if set to <c>true</c> then [authentication was successful].</param>
        /// <param name="errorMessage">The error message.</param>
        public AuthResponse(bool authSuccessful, string? errorMessage)
        {
            AuthSuccessful = authSuccessful;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Gets a value indicating whether [authentication successful].
        /// </summary>
        public bool AuthSuccessful { get; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string? ErrorMessage { get; }
    }
}
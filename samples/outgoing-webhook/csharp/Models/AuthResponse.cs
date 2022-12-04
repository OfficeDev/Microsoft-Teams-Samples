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
        public AuthResponse(bool authSuccessful, string errorMessage)
        {
            this.AuthSuccessful = authSuccessful;
            this.ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Gets a value indicating whether [authentication successful].
        /// </summary>
        /// <value>
        /// <c>true</c> if [authentication successful]; otherwise, <c>false</c>.
        /// </value>
        public bool AuthSuccessful { get; private set; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage { get; private set; }
    }
}
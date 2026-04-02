// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Samples.LinkUnfurlerForReddit
{
    using System;

    /// <summary>
    /// The RedditUnauthorizedException is thrown when the Reddit API returns an
    /// unexpected response.
    /// </summary>
    public sealed class RedditUnauthorizedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedditUnauthorizedException"/> class.
        /// </summary>
        public RedditUnauthorizedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedditUnauthorizedException"/> class.
        /// </summary>
        /// <param name="reason">The reason returned from the Reddit API.</param>
        /// <param name="innerException">The underlying exception that occured.</param>
        public RedditUnauthorizedException(
            string reason,
            Exception innerException)
            : base(reason, innerException)
        {
            this.Reason = reason;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedditUnauthorizedException"/> class.
        /// </summary>
        /// <param name="reason">The reason returned from the Reddit API.</param>
        public RedditUnauthorizedException(string reason)
            : base(reason)
        {
            this.Reason = reason;
        }

        /// <summary>
        /// Gets the reason the request failed from the Reddit API.
        /// </summary>
        /// <value>The reason string.</value>
        public string Reason { get; }
    }
}
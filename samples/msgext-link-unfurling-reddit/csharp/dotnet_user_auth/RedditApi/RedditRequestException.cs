// <copyright file="RedditRequestException.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurlerForReddit
{
    using System;

    /// <summary>
    /// The RedditRequestException is thrown when the Reddit API returns an
    /// unexpected response.
    /// </summary>
    public sealed class RedditRequestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedditRequestException"/> class.
        /// </summary>
        public RedditRequestException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedditRequestException"/> class.
        /// </summary>
        /// <param name="reason">The reason returned from the Reddit API.</param>
        /// <param name="innerException">The underlying exception that occured.</param>
        public RedditRequestException(
            string reason,
            Exception innerException)
            : base(reason, innerException)
        {
            this.Reason = reason;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedditRequestException"/> class.
        /// </summary>
        /// <param name="reason">The reason returned from the Reddit API.</param>
        public RedditRequestException(string reason)
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
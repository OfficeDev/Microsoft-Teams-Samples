// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Samples.LinkUnfurlerForReddit
{
    using System;

    /// <summary>
    /// The RedditLinkModel is an internal representation of a Reddit Link.
    /// </summary>
    public sealed class RedditLinkModel
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the Subreddit name.
        /// </summary>
        public string Subreddit { get; set; }

        /// <summary>
        /// Gets or sets the Link Title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Thumbnail url.
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        /// Gets or sets the text content of a self-post.
        /// </summary>
        /// <value>The self post text, null if not a self-post.</value>
        public string SelfText { get; set; }

        /// <summary>
        /// Gets or sets the total number of comments on the Link.
        /// </summary>
        public int NumComments { get; set; }

        /// <summary>
        /// Gets or sets the score for the Link.
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Gets or sets the url for the link.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets the author of the post.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the time when the post was made (UTC).
        /// </summary>
        public DateTimeOffset Created { get; set; }
    }
}
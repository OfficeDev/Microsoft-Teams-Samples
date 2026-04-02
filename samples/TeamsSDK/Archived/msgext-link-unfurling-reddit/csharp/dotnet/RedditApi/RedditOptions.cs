// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Samples.LinkUnfurlerForReddit
{
    /// <summary>
    /// The Reddit options are used to configure the Reddit Link Unfurler.
    /// </summary>
    public sealed class RedditOptions
    {
        /// <summary>
        /// Gets or sets the Client User Agent is the user agent that will be reported to the Reddit API.
        /// </summary>
        /// <remarks>
        /// See https://github.com/reddit-archive/reddit/wiki/API#rules for rules around the user-agent string.
        /// </remarks>
        public string ClientUserAgent { get; set; }

        /// <summary>
        /// Gets or sets the BotFramework Connection Name for the registered auth providers.
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-authentication?view=azure-bot-service-4.0#about-the-bot-framework-token-service .
        /// </remarks>
        public string BotFrameworkConnectionName { get; set; }

        /// <summary>
        /// Gets or sets the AppId for Reddit.
        /// </summary>
        /// <value>The AppId.</value>
        /// <remarks>
        /// Apps registered at: https://www.reddit.com/prefs/apps .
        /// </remarks>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the App Password for Reddit.
        /// </summary>
        /// <value>The AppId.</value>
        /// <remarks>
        /// This is the 'secret' from https://www.reddit.com/prefs/apps .
        /// </remarks>
        public string AppPassword { get; set; }
    }
}
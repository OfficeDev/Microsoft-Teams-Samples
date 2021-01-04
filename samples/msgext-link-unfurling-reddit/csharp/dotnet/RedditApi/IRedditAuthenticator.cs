// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Samples.LinkUnfurlerForReddit
{
    using System.Threading.Tasks;

    /// <summary>
    /// An IRedditAuthenticator is responsible for getting an access token to api.reddit.com based on the current
    /// call context.
    /// </summary>
    public interface IRedditAuthenticator
    {
        /// <summary>
        /// Get an access token given the current call context.
        /// </summary>
        /// <returns>A task resolving to a token.</returns>
        Task<string> GetAccessTokenAsync();
    }
}
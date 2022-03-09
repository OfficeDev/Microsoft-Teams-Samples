// <copyright file="IUrlParser.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.ResourceServices
{
    /// <summary>
    /// Url Parser contract.
    ///
    /// Provides methods to validate and get resource id from a resource location url.
    /// </summary>
    public interface IUrlParser
    {
        /// <summary>
        /// Checks if the url is valid and has resource id information.
        /// </summary>
        /// <param name="url">Url.</param>
        /// <returns>boolean result.</returns>
        bool IsValidResourceUrl(string url);

        /// <summary>
        /// Parses the url and gets resource id from it.
        /// </summary>
        /// <param name="url">Url.</param>
        /// <returns>Resource id.</returns>
        string GetResourceId(string url);
    }
}

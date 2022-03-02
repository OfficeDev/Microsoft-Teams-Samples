// <copyright file="UrlParser.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.ResourceServices
{
    using System;
    using Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.TeamsServices;

    /// <summary>
    /// <see cref="IUrlParser"/> implementation.
    /// </summary>
    internal class UrlParser : IUrlParser
    {
        private readonly IAppSettings appsettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlParser"/> class.
        /// </summary>
        /// <param name="appsettings">App settings.</param>
        public UrlParser(IAppSettings appsettings)
        {
            this.appsettings = appsettings ?? throw new ArgumentNullException(nameof(appsettings));
        }

        /// <inheritdoc />
        public string GetResourceId(string url)
        {
            if (!this.IsValidResourceUrl(url))
            {
                return string.Empty;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                return string.Empty;
            }

            var resourceId = uri.Segments[^1];
            return resourceId;
        }

        /// <inheritdoc />
        public bool IsValidResourceUrl(string url)
        {
            // A valid resource url looks like this - "https://domain/resources/resourceId"
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            // Validation logic may differ based on requirements.
            // We could add logic to validate each segment and resource id format.
            Uri.TryCreate(this.appsettings.BaseUrl, UriKind.Absolute, out Uri baseUri);
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri) ||
                uri.Scheme != baseUri?.Scheme)
            {
                return false;
            }

            return true;
        }
    }
}

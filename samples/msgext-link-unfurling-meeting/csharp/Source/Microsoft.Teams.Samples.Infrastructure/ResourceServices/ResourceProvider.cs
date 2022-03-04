// <copyright file="ResourceProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.ResourceServices
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities;
    using Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.TeamsServices;

    /// <summary>
    /// <see cref="IResourceProvider"/> implementation.
    /// </summary>
    internal class ResourceProvider : IResourceProvider
    {
        private readonly IAppSettings appSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceProvider"/> class.
        /// </summary>
        /// <param name="appSettings">App settings.</param>
        public ResourceProvider(IAppSettings appSettings)
        {
            this.appSettings = appSettings ?? throw new System.ArgumentNullException(nameof(appSettings));
        }

        /// <inheritdoc />
        public Task<Resource> GetResourceAsync(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                throw new System.ArgumentException($"'{nameof(resourceId)}' cannot be null or empty.", nameof(resourceId));
            }

            var resourceUrl = $"{this.appSettings.BaseUrl}/resources/{resourceId}";

            // Should ideally call resource service and prepare resource object.
            var resource = new Resource()
            {
                Name = resourceId,
                PreviewImageUrl = $"{this.appSettings.BaseUrl}/images/image.png",
                Id = resourceId,
                Url = resourceUrl,
            };

            return Task.FromResult(resource);
        }
    }
}

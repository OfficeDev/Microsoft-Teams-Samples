// <copyright file="ICardFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.TeamsServices
{
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain.Entities;

    /// <summary>
    /// Card factory contract.
    ///
    /// Provides methods to create resource review and preview card attachments.
    /// </summary>
    public interface ICardFactory
    {
        /// <summary>
        /// Gets card attachment to review resource context.
        /// </summary>
        /// <param name="resource">Resource object.</param>
        /// <returns>Card attachment.</returns>
        Attachment GetResourceContentCard(Resource resource);

        /// <summary>
        /// Gets card attachment to preview resource.
        /// </summary>
        /// <param name="resource">Resource object.</param>
        /// <returns>Card attachment.</returns>
        Attachment GetResourcePreviewCard(Resource resource);
    }
}

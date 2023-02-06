// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Apps.QBot.Domain.Models
{
    /// <summary>
    /// Logical KnowledgeBase model class.
    /// </summary>
    /// <remarks>This is a custom KnowledgeBase definition. It does not map to QnA Maker Service Knowledge base.</remarks>
    public sealed class KnowledgeBase
    {
        /// <summary>
        /// Gets or sets KnowledgeBase's id.
        /// </summary>
        /// <remarks>This is an internal Id. This does not map to QnA Maker service knowledge base id.</remarks>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets KnowledgeBase's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user's AAD Id who owns the knowledge base.
        /// </summary>
        public string OwnerUserId { get; set; }
    }
}

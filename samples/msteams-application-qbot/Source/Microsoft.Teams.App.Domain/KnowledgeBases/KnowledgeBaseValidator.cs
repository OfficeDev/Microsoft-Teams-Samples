// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Apps.QBot.Domain.KnowledgeBases
{
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// KnowledgeBase validator.
    /// </summary>
    internal sealed class KnowledgeBaseValidator : IKnowledgeBaseValidator
    {
        /// <inheritdoc/>
        public bool IsValid(KnowledgeBase knowledgeBase)
        {
            if (knowledgeBase == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(knowledgeBase.Name))
            {
                return false;
            }

            if (string.IsNullOrEmpty(knowledgeBase.OwnerUserId))
            {
                return false;
            }

            return true;
        }
    }
}

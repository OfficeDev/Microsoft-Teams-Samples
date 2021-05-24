// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Microsoft.Bot.Schema.Teams;
using System.Collections.Generic;

namespace MessageExtension_SP.Models
{
    /// <summary>
    /// MEAction
    /// </summary>
    public class MEAction
    {
        public string commandId { get; set; }
        public MessageActionsPayload MessagePayload { get; set; }
    }

    /// <summary>
    /// MESearch
    /// </summary>
    public class MESearch
    {
        public string commandId { get; set; }
        public IList<MessagingExtensionParameter> Parameters { get; set; }
    }
}

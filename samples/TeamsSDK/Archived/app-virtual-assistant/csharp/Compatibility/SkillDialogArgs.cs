// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Solutions.Skills.Dialogs
{
    // Compatibility type retained for sample dialog handoff options.
    public class SkillDialogArgs
    {
        public string SkillId { get; set; }

        public string ActivityType { get; set; } = ActivityTypes.Message;

        public string Name { get; set; }

        public object Value { get; set; }
    }
}

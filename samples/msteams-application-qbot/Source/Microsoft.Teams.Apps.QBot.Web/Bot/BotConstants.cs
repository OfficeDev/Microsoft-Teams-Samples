// <copyright file="BotConstants.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Web.Bot
{
    /// <summary>
    /// Bot Constants.
    /// </summary>
    public sealed class BotConstants
    {
        /// <summary>
        /// Conversation type constant for "channel" in a team.
        /// </summary>
        public const string Channel = "channel";

        /// <summary>
        /// Conversation type constant for "personal" chat with a user.
        /// </summary>
        public const string Personal = "personal";

        /// <summary>
        /// UserRole type constant for "guest" type in a team.
        /// </summary>
        public const string Guest = "guest";

        /// <summary>
        /// Messaging extenion command id to select an answer.
        /// </summary>
        public const string SelectThisAnswerCommandId = "selectthisanswer";

        /// <summary>
        /// Command context constant for message actions.
        /// </summary>
        public const string MessageCommandContext = "message";

        /// <summary>
        /// Suggest Answer action message text.
        /// </summary>
        public const string SuggestAnswerActionMessage = "SuggestAnswerAction";

        /// <summary>
        /// Helpful action text.
        /// </summary>
        public const string HelpfulActionText = "Helpful";

        /// <summary>
        /// Not-Helpful action text.
        /// </summary>
        public const string NotHelpfulActionText = "NotHelpful";
    }
}

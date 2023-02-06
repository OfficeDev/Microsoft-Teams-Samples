namespace Microsoft.Teams.Apps.QBot.Web.Bot
{
    /// <summary>
    /// Bot Error Codes.
    /// </summary>
    public enum BotErrorCodes
    {
        /// <summary>
        /// Unknown error.
        /// </summary>
        Unknown,

        /// <summary>
        /// Command not supported.
        /// </summary>
        CommandNotSupported,

        /// <summary>
        /// Command context not supported.
        /// </summary>
        CommandContextNotSupported,

        /// <summary>
        /// This is expected when QBot app is not installed to a team.
        /// </summary>
        CourseNotFound,

        /// <summary>
        /// This is expected when question is not cached with the service.
        /// </summary>
        QuestionNotFound,

        /// <summary>
        /// This is expected when user is not authorized to select an answer.
        /// </summary>
        ForbidenToSelectAnswer,

        /// <summary>
        /// This is expected when user tries to answer a question that is already marked as answered.
        /// </summary>
        QuestionMarkedAsAnswered,
    }
}

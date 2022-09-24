namespace Microsoft.Teams.Apps.QBot.Domain.Errors
{
    /// <summary>
    /// QBot error codes.
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>
        /// Unknown error.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Unauthorized.
        /// </summary>
        Forbidden,

        /// <summary>
        /// When client performs invalid operation.
        /// </summary>
        InvalidOperation,

        /// <summary>
        /// User not found.
        /// </summary>
        UserNotFound,

        /// <summary>
        /// Course not found.
        /// </summary>
        CourseNotFound,

        /// <summary>
        /// Invalid course definition.
        /// </summary>
        InvalidCourse,

        /// <summary>
        /// Channel not found.
        /// </summary>
        ChannelNotFound,

        /// <summary>
        /// Invalid channel definition.
        /// </summary>
        InvalidChannel,

        /// <summary>
        /// Tutorial group not found.
        /// </summary>
        TutorialGroupNotFound,

        /// <summary>
        /// Question not found.
        /// </summary>
        QuestionNotFound,

        /// <summary>
        /// Question is marked as answered.
        /// </summary>
        QuestionMarkedAsAnswered,

        /// <summary>
        /// Invalid answer object.
        /// </summary>
        InvalidAnswer,

        /// <summary>
        /// Invalid knowledge base definition.
        /// </summary>
        InvalidKnowledgeBase,

        /// <summary>
        /// Knowledge base not found.
        /// </summary>
        KnowledgeBaseNotFound,

        /// <summary>
        /// Member not found.
        /// </summary>
        MemberNotFound,

        /// <summary>
        /// Invalid tutorigal group object.
        /// </summary>
        InvalidTutorialGroup,
    }
}

namespace Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum
{
    /// <summary>
    /// Enum represents types of feedback.
    /// </summary>
    public enum FeedbackType
    {
        /// <summary>
        /// This represents  type is general feedback.
        /// </summary>
        GeneralFeedback = 0,

        /// <summary>
        /// This represents the type LearningContentFeedback.
        /// </summary>
        LearningContentFeedback = 1,

        /// <summary>
        /// This represents the type FeedbackFromLearningPath.
        /// </summary>
        FeedbackFromLearningPath = 2,

        /// <summary>
        /// This represents the type FeedbackFromLearningPathCompleted.
        /// </summary>
        FeedbackFromLearningPathCompleted = 3,
    }
}
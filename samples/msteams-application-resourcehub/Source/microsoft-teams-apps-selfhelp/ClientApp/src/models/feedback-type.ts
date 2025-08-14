export enum FeedbackType {
    /**Indicates that status is not Internal */
    GeneralFeedback = 0,

    /** Indicates that status is external.
     */
    LearningContentFeedback = 1,
    FeedbackFromLearningPath = 2,
    FeedbackFromLearningPathCompleted = 3
}
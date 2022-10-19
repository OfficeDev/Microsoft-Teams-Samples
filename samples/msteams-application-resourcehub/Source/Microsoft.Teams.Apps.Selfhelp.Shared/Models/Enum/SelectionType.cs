namespace Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum
{
    /// <summary>
    /// Enum represents new learning item selection type. 0 = GettingStarted, 1= Scenarios, 2 = Trending Now
    /// </summary>
    public enum SelectionType
    {
        /// <summary>
        /// This represents type is GettingStarted.
        /// </summary>
        GettingStarted,

        /// <summary>
        /// This represents the type is Scenarios.
        /// </summary>
        Scenarios,

        /// <summary>
        /// This represents the type is Trending Now
        /// </summary>
        TrendingNow,

        /// <summary>
        /// This represents the type is learning path
        /// </summary>
        LearningPath,
    }
}
namespace Microsoft.Teams.Apps.QBot.Web.Authorization
{
    /// <summary>
    /// Authorization policies.
    /// </summary>
    public static class AuthZPolicy
    {
        /// <summary>
        /// Authorization policy to authorize all members of a course.
        /// </summary>
        public const string CourseMemberPolicy = "CourseMemberPolicy";

        /// <summary>
        /// Authorization policy to authorize users who can manage a course.
        /// </summary>
        public const string CourseManagerPolicy = "CourseManagerPolicy";

        /// <summary>
        /// Authorization policy to authorize admins.
        /// </summary>
        public const string AdminPolicy = "AdminPolicy";

        /// <summary>
        /// Authorization policy to restrict access to user's resources.
        /// </summary>
        public const string UserResourcePolicy = "UserResourcePolicy";

        /// <summary>
        /// Authorization policy to restrict who can post an answer to a question.
        /// </summary>
        public const string PostAnswerPolicy = "PostAnswerPolicy";
    }
}

namespace Microsoft.Teams.Apps.QBot.Web.Authorization
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Courses;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Post answer handler.
    /// </summary>
    public class PostAnswerHandler : AuthorizationHandler<PostAnswerRequirement, Question>
    {
        private readonly ICourseReader courseReader;
        private readonly ILogger<PostAnswerHandler> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostAnswerHandler"/> class.
        /// </summary>
        /// <param name="courseReader">Course reader.</param>
        /// <param name="logger">Logger.</param>
        public PostAnswerHandler(
            ICourseReader courseReader,
            ILogger<PostAnswerHandler> logger)
        {
            this.courseReader = courseReader ?? throw new ArgumentNullException(nameof(courseReader));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PostAnswerRequirement requirement, Question resource)
        {
            // Ensure user is authenticated.
            if (!context.User.Identity.IsAuthenticated)
            {
                throw new QBotException(HttpStatusCode.Unauthorized, ErrorCode.Forbidden, "User is not authenticated.");
            }

            // Question author can post an answer.
            var userId = context.User.GetUserId();
            if (resource.AuthorId == userId)
            {
                context.Succeed(requirement);
                return;
            }

            // Or course educator or tutor.
            try
            {
                var member = await this.courseReader.GetMemberAsync(resource.CourseId, userId);
                if (member.Role == MemberRole.Educator || member.Role == MemberRole.Tutor)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Failed to verify user's membership");
            }

            throw new QBotException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, "User is not authorized.");
        }
    }
}

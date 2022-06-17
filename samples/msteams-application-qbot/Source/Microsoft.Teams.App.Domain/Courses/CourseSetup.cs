namespace Microsoft.Teams.Apps.QBot.Domain.Courses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.IRepositories;
    using Microsoft.Teams.Apps.QBot.Domain.IServices;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Concrete Implementation of <see cref="ICourseSetup"/>.
    /// </summary>
    internal class CourseSetup : ICourseSetup
    {
        private readonly ICourseValidator courseValidator;
        private readonly ICourseRepository courseRepository;
        private readonly IChannelRepository channelRepository;
        private readonly IMemberRepository memberRepository;
        private readonly ITutorialGroupRepository tutorialGroupRepository;
        private readonly ITeamsMessageService teamsMessageService;
        private readonly ITutorialGroupValidator tutorialGroupValidator;
        private readonly ILogger<CourseSetup> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CourseSetup"/> class.
        /// </summary>
        /// <param name="courseRepository">Course repository.</param>
        /// <param name="courseValidator">Course validator.</param>
        /// <param name="channelRepository">Channel repository.</param>
        /// <param name="memberRepository">Member repository.</param>
        /// <param name="tutorialGroupRepository">Tutorial group repository.</param>
        /// <param name="teamsMessageService">Teams message service.</param>
        /// <param name="tutorialGroupValidator">Tutorial group validator.</param>
        /// <param name="logger">Logger.</param>
        public CourseSetup(
                            ICourseValidator courseValidator,
                            ICourseRepository courseRepository,
                            IChannelRepository channelRepository,
                            IMemberRepository memberRepository,
                            ITutorialGroupRepository tutorialGroupRepository,
                            ITeamsMessageService teamsMessageService,
                            ITutorialGroupValidator tutorialGroupValidator,
                            ILogger<CourseSetup> logger)
        {
            this.courseValidator = courseValidator ?? throw new ArgumentNullException(nameof(courseValidator));
            this.courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            this.channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
            this.memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
            this.tutorialGroupRepository = tutorialGroupRepository ?? throw new ArgumentNullException(nameof(tutorialGroupRepository));
            this.teamsMessageService = teamsMessageService ?? throw new ArgumentNullException(nameof(teamsMessageService));
            this.tutorialGroupValidator = tutorialGroupValidator ?? throw new ArgumentNullException(nameof(tutorialGroupValidator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task AddNewCourseAsync(Course course, IEnumerable<Channel> channels, IEnumerable<Member> members)
        {
            if (!this.courseValidator.IsValid(course))
            {
                throw new QBotException(HttpStatusCode.BadRequest, ErrorCode.InvalidCourse, $"{nameof(this.AddNewCourseAsync)} Invalid course object.");
            }

            await this.courseRepository.AddCourseAsync(course);
            await this.channelRepository.AddChannelsAsync(channels);
            await this.memberRepository.AddCourseMembersAsync(course.Id, members);

            // Notify educators to set-up course.
            // Note: App could do a batch request to do this efficiently.
            var educators = members.Where(m => m.Role == MemberRole.Educator);
            foreach (var educator in educators)
            {
                try
                {
                    await this.teamsMessageService.NotifyMemberToSetupCourseAsync(course, educator);
                }
                catch (QBotException exception)
                {
                    this.logger.LogWarning(exception, $"Failed to notify educator to setup course. Educator user id: {educator.AadId}");
                }
            }
        }

        /// <inheritdoc/>
        public async Task UpdateCourseAsync(Course course)
        {
            if (!this.courseValidator.IsValid(course))
            {
                throw new QBotException(HttpStatusCode.BadRequest, ErrorCode.InvalidCourse, $"{nameof(this.UpdateCourseAsync)} Invalid course object.");
            }

            await this.courseRepository.UpdateCourseAsync(course);
        }

        /// <inheritdoc/>
        public async Task DeleteCourseAsync(string courseId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentNullException(nameof(courseId));
            }

            await this.courseRepository.DeleteCourseAsync(courseId);
        }

        /// <inheritdoc/>
        public async Task AddChannelAsync(string courseId, Channel channel)
        {
            if (!this.courseValidator.IsValid(channel))
            {
                throw new QBotException(HttpStatusCode.BadRequest, ErrorCode.InvalidChannel, $"{nameof(this.AddChannelAsync)} Invalid channel object.");
            }

            await this.channelRepository.AddChannelAsync(channel);
        }

        /// <inheritdoc/>
        public async Task UpdateChannelAsync(string courseId, Channel channel)
        {
            if (!this.courseValidator.IsValid(channel))
            {
                throw new QBotException(HttpStatusCode.BadRequest, ErrorCode.InvalidChannel, "Invalid channel object.");
            }

            var cachedChannel = await this.channelRepository.GetChannelAsync(channel.Id);
            if (cachedChannel == null)
            {
                // Note: an alternate option could be to upsert.
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.ChannelNotFound, $"Channel {channel.Id} not found");
            }

            if (cachedChannel.CourseId != channel.CourseId)
            {
                throw new QBotException(HttpStatusCode.BadRequest, ErrorCode.InvalidOperation, $"Channel {channel.Id} is already mapped to another course {cachedChannel.CourseId}.");
            }

            // Only name updates allowed.
            if (!cachedChannel.Name.Equals(channel.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                await this.channelRepository.UpdateChannelAsync(channel);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteChannelAsync(string courseId, string channelId)
        {
            if (string.IsNullOrWhiteSpace(courseId))
            {
                throw new ArgumentException($"'{nameof(courseId)}' cannot be null or whitespace", nameof(courseId));
            }

            if (string.IsNullOrWhiteSpace(channelId))
            {
                throw new ArgumentException($"'{nameof(channelId)}' cannot be null or whitespace", nameof(channelId));
            }

            var channel = await this.channelRepository.GetChannelAsync(channelId);
            if (channel == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.ChannelNotFound, $"Channel {channelId} not found");
            }

            if (channel.CourseId != courseId)
            {
                throw new QBotException(HttpStatusCode.BadRequest, ErrorCode.InvalidOperation, $"Invalid course-channel {courseId}-{channelId} mapping.");
            }

            await this.channelRepository.DeleteChannelAsync(courseId);
        }

        /// <inheritdoc/>
        public Task AddMembersAsync(string courseId, IEnumerable<Member> members)
        {
            if (string.IsNullOrWhiteSpace(courseId))
            {
                throw new ArgumentNullException(nameof(courseId));
            }

            if (members is null)
            {
                throw new ArgumentNullException(nameof(members));
            }

            return this.memberRepository.AddCourseMembersAsync(courseId, members);
        }

        /// <inheritdoc/>
        public async Task RemoveMembersAsync(string courseId, IEnumerable<Member> members)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentException($"'{nameof(courseId)}' cannot be null or empty", nameof(courseId));
            }

            if (members is null)
            {
                throw new ArgumentNullException(nameof(members));
            }

            // Remove from course
            await this.memberRepository.RemoveCourseMembersAsync(courseId, members.Select(m => m.AadId));
        }

        /// <inheritdoc/>
        public Task AddTutorialGroupsAsync(string courseId, IEnumerable<TutorialGroup> tutorialGroups)
        {
            if (courseId is null)
            {
                throw new ArgumentNullException(nameof(courseId));
            }

            if (tutorialGroups is null)
            {
                throw new ArgumentNullException(nameof(tutorialGroups));
            }

            // Validate and generate ids.
            foreach (var tutorialGroup in tutorialGroups)
            {
                if (!this.tutorialGroupValidator.IsValid(tutorialGroup))
                {
                    throw new QBotException(HttpStatusCode.BadRequest, ErrorCode.InvalidTutorialGroup, "Invalid tutorial group definition.");
                }

                tutorialGroup.Id = Guid.NewGuid().ToString();
            }

            return this.tutorialGroupRepository.AddTutorialGroupsAsync(tutorialGroups);
        }

        /// <inheritdoc/>
        public async Task DeleteTutorialGroupsAsync(IEnumerable<string> tutorialGroupIds)
        {
            if (tutorialGroupIds is null)
            {
                throw new ArgumentNullException(nameof(tutorialGroupIds));
            }

            foreach (var id in tutorialGroupIds)
            {
                await this.tutorialGroupRepository.DeleteTutorialGroupAsync(id);
            }
        }

        /// <inheritdoc/>
        public async Task UpdateMemberAsync(string courseId, Member member)
        {
            await this.memberRepository.UpdateCourseMemberAsync(courseId, member);
        }
    }
}

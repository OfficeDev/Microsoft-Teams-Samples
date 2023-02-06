namespace Microsoft.Teams.Apps.QBot.Domain.Courses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.IRepositories;
    using Microsoft.Teams.Apps.QBot.Domain.IServices;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Read Course informaiton.
    /// </summary>
    internal class CourseReader : ICourseReader
    {
        private readonly ICourseRepository courseRepository;
        private readonly IChannelRepository channelRepository;
        private readonly IMemberRepository memberRepository;
        private readonly ITutorialGroupRepository tutorialGroupRepository;
        private readonly ITeamInfoService teamInfoService;
        private readonly ILogger<CourseReader> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CourseReader"/> class.
        /// </summary>
        /// <param name="courseRepository">Course repository.</param>
        /// <param name="channelRepository">Channel repository.</param>
        /// <param name="memberRepository">Member repository.</param>
        /// <param name="tutorialGroupRepository">Tutorial group repository.</param>
        /// <param name="teamInfoService">Team info service.</param>
        /// <param name="logger">Logger.</param>
        public CourseReader(
            ICourseRepository courseRepository,
            IChannelRepository channelRepository,
            IMemberRepository memberRepository,
            ITutorialGroupRepository tutorialGroupRepository,
            ITeamInfoService teamInfoService,
            ILogger<CourseReader> logger)
        {
            this.courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            this.channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
            this.memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
            this.tutorialGroupRepository = tutorialGroupRepository ?? throw new ArgumentNullException(nameof(tutorialGroupRepository));
            this.teamInfoService = teamInfoService ?? throw new ArgumentNullException(nameof(teamInfoService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<Course> GetCourseAsync(string courseId)
        {
            if (courseId is null)
            {
                throw new ArgumentNullException(nameof(courseId));
            }

            var course = await this.courseRepository.GetCourseAsync(courseId);
            try
            {
                course.ProfilePicUri = await this.teamInfoService.GetTeamPhotoAsync(course.TeamAadObjectId);
            }
            catch (QBotException exception)
            {
                this.logger.LogWarning(exception, $"Failed to fetch team photo. ErrorCode: {exception.Code}, Status Code: {exception.StatusCode}");
            }

            return course;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            var courses = await this.courseRepository.GetAllCoursesAsync();
            var teamAadIds = courses.Select(course => course.TeamAadObjectId);
            try
            {
                var photoDic = await this.teamInfoService.GetTeamsPhotoAsync(teamAadIds);
                foreach (var course in courses)
                {
                    photoDic.TryGetValue(course.TeamAadObjectId, out var photoPicUri);
                    course.ProfilePicUri = photoPicUri;
                }
            }
            catch (QBotException exception)
            {
                this.logger.LogWarning(exception, $"Failed to fetch team photo. ErrorCode: {exception.Code}, Status Code: {exception.StatusCode}");
            }

            return courses;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Course>> GetAllCoursesForUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var courses = await this.courseRepository.GetAllCoursesForUserAsync(userId);
            var teamAadIds = courses.Select(course => course.TeamAadObjectId);
            try
            {
                var photoDic = await this.teamInfoService.GetTeamsPhotoAsync(teamAadIds);
                foreach (var course in courses)
                {
                    photoDic.TryGetValue(course.TeamAadObjectId, out var photoPicUri);
                    course.ProfilePicUri = photoPicUri;
                }
            }
            catch (QBotException exception)
            {
                this.logger.LogWarning(exception, $"Failed to fetch team photo. ErrorCode: {exception.Code}, Status Code: {exception.StatusCode}");
            }

            return courses;
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Channel>> GetAllChannelsAsync(string courseId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentNullException(nameof(courseId));
            }

            return this.channelRepository.GetAllChannelsAsync(courseId);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Member>> GetAllMembersAsync(string courseId, IList<MemberRole> roles)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentNullException(nameof(courseId));
            }

            if (roles == null || roles.Count == 0)
            {
                return this.memberRepository.GetCourseMembersAsync(courseId);
            }
            else
            {
                return this.memberRepository.GetCourseMembersAsync(courseId, roles);
            }
        }

        /// <inheritdoc/>
        public Task<IEnumerable<TutorialGroup>> GetAllTutorialGroupsAsync(string courseId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentNullException(nameof(courseId));
            }

            return this.tutorialGroupRepository.GetAllTutorialGroupsAsync(courseId);
        }

        /// <inheritdoc/>
        public Task<Member> GetMemberAsync(string courseId, string memberId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentException($"'{nameof(courseId)}' cannot be null or empty.", nameof(courseId));
            }

            if (string.IsNullOrEmpty(memberId))
            {
                throw new ArgumentException($"'{nameof(memberId)}' cannot be null or empty.", nameof(memberId));
            }

            return this.memberRepository.GetCourseMemberAsync(courseId, memberId);
        }

        /// <inheritdoc/>
        public Task<Channel> GetChannelAsync(string courseId, string channelId)
        {
            if (courseId is null)
            {
                throw new ArgumentNullException(nameof(courseId));
            }

            if (channelId is null)
            {
                throw new ArgumentNullException(nameof(channelId));
            }

            return this.channelRepository.GetChannelAsync(channelId);
        }
    }
}

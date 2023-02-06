namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.IRepositories;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Member Repository implementation.
    /// </summary>
    internal sealed class MemberRepository : IMemberRepository
    {
        private readonly QBotDbContext dbContext;
        private readonly IMapper mapper;
        private readonly ILogger<MemberRepository> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberRepository"/> class.
        /// </summary>
        /// <param name="dbContext">Db Conext.</param>
        /// <param name="mapper">Auto-Mapper.</param>
        /// <param name="logger">Logger.</param>
        public MemberRepository(
            QBotDbContext dbContext,
            IMapper mapper,
            ILogger<MemberRepository> logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task AddCourseMembersAsync(string courseId, IEnumerable<Member> members)
        {
            var course = await this.dbContext.Courses.FindAsync(courseId);
            if (course == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.CourseNotFound, $"{nameof(this.AddCourseMembersAsync)}: Course {courseId} not found!");
            }

            try
            {
                foreach (var member in members)
                {
                    // Note: We could explore other options to improve perf.
                    var userEntity = this.mapper.Map<UserEntity>(member);
                    this.AddUserInternal(userEntity);

                    var memberEntity = this.ConvertToCourseMemberEntity(courseId, member);
                    this.dbContext.Add(memberEntity);
                }

                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to add members for course : {courseId}";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public Task<Member> GetCourseMemberAsync(string courseId, string memberId)
        {
            var course = this.dbContext.Courses
                .Include(c => c.Members.Where(m => m.UserId.Equals(memberId))).ThenInclude(m => m.User)
                .Include(c => c.Members.Where(m => m.UserId.Equals(memberId))).ThenInclude(m => m.TutorialGroupsMembership)
                .FirstOrDefault(c => c.Id.Equals(courseId));

            if (course == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.CourseNotFound, $"{nameof(this.GetCourseMemberAsync)}: Course {courseId} not found!");
            }

            var memberEntity = course.Members.FirstOrDefault(m => m.UserId.Equals(memberId));
            if (memberEntity == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.MemberNotFound, $"{nameof(this.GetCourseMemberAsync)}: Member {memberId} not found!");
            }

            var member = this.mapper.Map<Member>(memberEntity.User);
            member.Role = Enum.Parse<MemberRole>(memberEntity.MemberRole, true);
            member.TutorialGroupMembership = memberEntity.TutorialGroupsMembership.Select(tg => tg.TutorialGroupId);
            return Task.FromResult(member);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Member>> GetCourseMembersAsync(string courseId)
        {
            var course = this.dbContext.Courses
                .Include(c => c.Members).ThenInclude(m => m.User)
                .Include(c => c.Members).ThenInclude(m => m.TutorialGroupsMembership)
                .FirstOrDefault(c => c.Id.Equals(courseId));

            if (course == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.CourseNotFound, $"{nameof(this.GetCourseMembersAsync)}: Course {courseId} not found!");
            }

            var members = new List<Member>();
            foreach (var entity in course.Members)
            {
                var member = this.mapper.Map<Member>(entity.User);
                member.Role = Enum.Parse<MemberRole>(entity.MemberRole, true);
                member.TutorialGroupMembership = entity.TutorialGroupsMembership.Select(tg => tg.TutorialGroupId);
                members.Add(member);
            }

            return await Task.FromResult(members);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Member>> GetCourseMembersAsync(string courseId, IList<MemberRole> roles)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentException($"'{nameof(courseId)}' cannot be null or empty.", nameof(courseId));
            }

            if (roles is null || roles.Count == 0)
            {
                throw new ArgumentNullException(nameof(roles));
            }

            var rolesString = roles.Select(role => role.ToString());

            var course = this.dbContext.Courses
                .Include(c => c.Members.Where(m => rolesString.Contains(m.MemberRole)))
                .ThenInclude(m => m.User)
                .FirstOrDefault(c => c.Id.Equals(courseId));

            if (course == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.CourseNotFound, $"{nameof(this.GetCourseMembersAsync)}: Course {courseId} not found!");
            }

            var entities = course.Members;
            var members = new List<Member>();
            foreach (var entity in entities)
            {
                var member = this.mapper.Map<Member>(entity.User);
                member.Role = Enum.Parse<MemberRole>(entity.MemberRole, true);
                members.Add(member);
            }

            return Task.FromResult<IEnumerable<Member>>(members);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Member>> GetTutorialGroupMembersAsync(string tutorialGroupId)
        {
            var group = this.dbContext.TutorialGroups
                .Include(t => t.Members)
                .ThenInclude(tgm => tgm.User)
                .FirstOrDefault(t => t.Id == tutorialGroupId);

            if (group == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.TutorialGroupNotFound, $"{nameof(this.GetTutorialGroupMembersAsync)}: Tutorial group {tutorialGroupId} not found!");
            }

            var members = new List<Member>();
            foreach (var entity in group.Members)
            {
                // Note: We do not map user role.
                members.Add(this.mapper.Map<Member>(entity.User));
            }

            return await Task.FromResult(members);
        }

        /// <inheritdoc/>
        public async Task RemoveCourseMembersAsync(string courseId, IEnumerable<string> memberIds)
        {
            var membersIdSet = memberIds.ToHashSet();
            var course = this.dbContext.Courses
                .Include(c => c.Members.Where(m => membersIdSet.Contains(m.UserId)))
                .ThenInclude(m => m.TutorialGroupsMembership)
                .FirstOrDefault(c => c.Id == courseId);

            if (course == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.CourseNotFound, $"Course {courseId} not found!");
            }

            try
            {
                var members = course.Members;
                foreach (var member in members)
                {
                    foreach (var membership in member.TutorialGroupsMembership)
                    {
                        this.dbContext.Remove(membership);
                    }

                    this.dbContext.Remove(member);
                }

                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to remove members from course: {courseId}";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public async Task RemoveTutorialGroupMemberAsync(string tutorialGroupId, IEnumerable<string> memberIds)
        {
            var groupMembers = memberIds.Select(id => new TutorialGroupMemberEntity()
            {
                TutorialGroupId = tutorialGroupId,
                UserId = id,
            });

            try
            {
                this.dbContext.RemoveRange(groupMembers);
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to remove members from tutorial group: {tutorialGroupId}";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public async Task UpdateCourseMemberAsync(string courseId, Member member)
        {
            var course = this.dbContext.Courses
                            .Include(c => c.Members.Where(m => m.UserId.Equals(member.AadId))).ThenInclude(m => m.User)
                            .Include(c => c.Members.Where(m => m.UserId.Equals(member.AadId))).ThenInclude(m => m.TutorialGroupsMembership)
                            .FirstOrDefault(c => c.Id.Equals(courseId));

            if (course == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.CourseNotFound, $"{nameof(this.UpdateCourseMemberAsync)}: Course {courseId} not found!");
            }

            var cachedMember = course.Members.FirstOrDefault(m => m.UserId.Equals(member.AadId));
            if (cachedMember == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.MemberNotFound, $"{nameof(this.UpdateCourseMemberAsync)}: Member {member.AadId} not found!");
            }

            // Update member role.
            var memberEntity = this.ConvertToCourseMemberEntity(courseId, member);
            if (cachedMember.MemberRole != memberEntity.MemberRole)
            {
                cachedMember.MemberRole = memberEntity.MemberRole;
                this.dbContext.Update(cachedMember);
            }

            // Update tutorial group membership.
            var removeList = cachedMember.TutorialGroupsMembership.Where(tg => !member.TutorialGroupMembership.Contains(tg.TutorialGroupId)).ToList();
            foreach (var tg in removeList)
            {
                cachedMember.TutorialGroupsMembership.Remove(tg);
            }

            var existingMembership = cachedMember.TutorialGroupsMembership.Select(tg => tg.TutorialGroupId);
            var addedTo = member.TutorialGroupMembership.Except(existingMembership).ToHashSet<string>();
            foreach (var id in addedTo)
            {
                cachedMember.TutorialGroupsMembership.Add(new TutorialGroupMemberEntity()
                {
                    CourseId = courseId,
                    TutorialGroupId = id,
                    UserId = member.AadId,
                });
            }

            try
            {
                this.dbContext.Update(cachedMember);
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to update course member: {member.AadId}";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        private void AddUserInternal(UserEntity userEntity)
        {
            if (this.dbContext.Users.Find(userEntity.AadId) == null)
            {
                this.dbContext.Users.Add(userEntity);
            }
        }

        private CourseMemberEntity ConvertToCourseMemberEntity(string courseId, Member member)
        {
            return new CourseMemberEntity()
            {
                UserId = member.AadId,
                CourseId = courseId,
                MemberRole = member.Role.ToString(),
            };
        }
    }
}

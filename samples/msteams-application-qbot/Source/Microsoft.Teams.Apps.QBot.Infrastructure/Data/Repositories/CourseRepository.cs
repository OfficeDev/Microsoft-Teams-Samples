namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.IRepositories;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Course Repository.
    /// </summary>
    internal class CourseRepository : ICourseRepository
    {
        private readonly QBotDbContext dbContext;
        private readonly IMapper mapper;
        private readonly ILogger<CourseRepository> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CourseRepository"/> class.
        /// </summary>
        /// <param name="dbContext">Db Conext.</param>
        /// <param name="mapper">Auto-Mapper.</param>
        /// <param name="logger">Logger.</param>
        public CourseRepository(
            QBotDbContext dbContext,
            IMapper mapper,
            ILogger<CourseRepository> logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task AddCourseAsync(Course course)
        {
            if (course is null)
            {
                throw new ArgumentNullException(nameof(course));
            }

            var entity = this.mapper.Map<CourseEntity>(course);
            try
            {
                this.dbContext.Courses.Add(entity);
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to add course: {course.Id}";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteCourseAsync(string courseId)
        {
            var course = await this.dbContext.Courses.FindAsync(courseId);
            if (course == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.CourseNotFound, $"{nameof(this.DeleteCourseAsync)}: Course {courseId} not found!");
            }

            try
            {
                this.dbContext.Courses.Remove(course);
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to delete course: {courseId}";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            IEnumerable<CourseEntity> courseEntities = this.dbContext.Courses.Include(c => c.TutorialGroups);
            IEnumerable<Course> courses = this.mapper.Map<IEnumerable<Course>>(courseEntities);
            return Task.FromResult(courses);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Course>> GetAllCoursesForUserAsync(string userId)
        {
            var user = await this.dbContext.Users
                .Include(u => u.CourseMembership)
                .ThenInclude(cm => cm.Course)
                .ThenInclude(c => c.TutorialGroups)
                .FirstOrDefaultAsync(u => u.AadId.Equals(userId));

            if (user == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.UserNotFound, $"{nameof(this.GetAllCoursesForUserAsync)}: User {userId} not found!");
            }

            var courses = new List<Course>();
            foreach (var entity in user.CourseMembership)
            {
                courses.Add(this.mapper.Map<Course>(entity.Course));
            }

            return courses;
        }

        /// <inheritdoc/>
        public async Task<Course> GetCourseAsync(string courseId)
        {
            var courseEntity = await this.dbContext.Courses
                .Include(c => c.TutorialGroups)
                .FirstOrDefaultAsync(c => c.Id.Equals(courseId));

            if (courseEntity == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.CourseNotFound, $"{nameof(this.GetCourseAsync)}: Course {courseId} not found!");
            }

            return this.mapper.Map<Course>(courseEntity);
        }

        /// <inheritdoc/>
        public Task UpdateCourseAsync(Course course)
        {
            var cachedCourse = this.dbContext.Courses.Find(course.Id);

            // Throw if entity doesn't exist
            if (cachedCourse == null)
            {
                this.logger.LogWarning($"Course: {course.Id} not found");
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.CourseNotFound, $"{nameof(this.UpdateCourseAsync)}: Course {course.Id} not found!");
            }

            // Update if it exists.
            var courseEntity = this.mapper.Map<CourseEntity>(course);
            cachedCourse.Name = courseEntity.Name;
            cachedCourse.KnowledgeBaseId = courseEntity.KnowledgeBaseId;

            try
            {
                this.dbContext.Courses.Update(cachedCourse);
                return this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to update course: {course.Id}";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }
    }
}

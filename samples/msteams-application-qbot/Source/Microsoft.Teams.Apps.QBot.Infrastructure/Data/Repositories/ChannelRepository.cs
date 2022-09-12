namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.QBot.Domain;
    using Microsoft.Teams.Apps.QBot.Domain.Errors;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Channel Repository Impl.
    /// </summary>
    internal class ChannelRepository : IChannelRepository
    {
        private readonly QBotDbContext dbContext;
        private readonly IMapper mapper;
        private readonly ILogger<ChannelRepository> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelRepository"/> class.
        /// </summary>
        /// <param name="dbContext">Db Conext.</param>
        /// <param name="mapper">Auto-Mapper.</param>
        /// <param name="logger">Logger.</param>
        public ChannelRepository(
            QBotDbContext dbContext,
            IMapper mapper,
            ILogger<ChannelRepository> logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task AddChannelAsync(Channel channel)
        {
            try
            {
                var entity = this.mapper.Map<ChannelEntity>(channel);
                this.dbContext.Channels.Add(entity);
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to add channel: {channel.Id}";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public async Task AddChannelsAsync(IEnumerable<Channel> channels)
        {
            try
            {
                var entities = this.mapper.Map<IEnumerable<ChannelEntity>>(channels);
                this.dbContext.Channels.AddRange(entities);
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                this.logger.LogError(exception, "Failed to add channels");
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, "Failed to add channels.", exception);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteChannelAsync(string channelId)
        {
            var channel = await this.dbContext.Channels.FindAsync(channelId);
            if (channel == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.ChannelNotFound, $"{nameof(this.DeleteChannelAsync)}: Channel {channelId} not found!");
            }

            try
            {
                this.dbContext.Channels.Remove(channel);
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to add channel: {channelId}";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Channel>> GetAllChannelsAsync(string courseId)
        {
            var course = await this.dbContext.Courses
                .Include(c => c.Channels)
                .FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.CourseNotFound, $"{nameof(this.GetAllChannelsAsync)}: Course {courseId} not found!");
            }

            return this.mapper.Map<IEnumerable<Channel>>(course.Channels);
        }

        /// <inheritdoc/>
        public async Task<Channel> GetChannelAsync(string channelId)
        {
            var channelEntity = await this.dbContext.Channels.FindAsync(channelId);
            if (channelEntity == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.ChannelNotFound, $"{nameof(this.GetChannelAsync)}: Channel {channelId} not found!");
            }

            return this.mapper.Map<Channel>(channelEntity);
        }

        /// <inheritdoc/>
        public Task UpdateChannelAsync(Channel channel)
        {
            var cachedChannel = this.dbContext.Channels.Find(channel.Id);

            // Throw if entity doesn't exist
            if (cachedChannel == null)
            {
                throw new QBotException(HttpStatusCode.NotFound, ErrorCode.ChannelNotFound, $"{nameof(this.UpdateChannelAsync)}: Channel {channel.Id} not found!");
            }

            // Update if it exists.
            var channelEntity = this.mapper.Map<ChannelEntity>(channel);
            cachedChannel.Name = channelEntity.Name;

            try
            {
                this.dbContext.Channels.Update(cachedChannel);
                return this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var message = $"Failed to update channel: {channel.Id}";
                this.logger.LogError(exception, message);
                throw new QBotException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, message, exception);
            }
        }
    }
}
